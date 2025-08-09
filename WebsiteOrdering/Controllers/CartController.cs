using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteOrdering.Helper;
using WebsiteOrdering.Models;
using WebsiteOrdering.Repositories;
using WebsiteOrdering.ViewModels;

namespace WebsiteOrdering.Controllers
{
    public class CartController : Controller
    {
        private readonly AppDbContext _appDbContext;
        private readonly IAccountRepository _accountRepository;

        public CartController(AppDbContext appDbContext, IAccountRepository accountRepository)
        {
            _appDbContext = appDbContext;
            _accountRepository = accountRepository;
        }
        public UserLocationSessionViewModel? GetUserLocationFromSession()
        {
            var sessionData = HttpContext.Session.GetString("UserLocationInfo");
            if (!string.IsNullOrEmpty(sessionData))
            {
                return JsonSerializer.Deserialize<UserLocationSessionViewModel>(sessionData);
            }
            return null;
        }
        [HttpPost]
        public async Task<IActionResult> AddToCart(IFormCollection form)
        {
            // Debug: In ra tất cả dữ liệu từ form
            foreach (var key in form.Keys)
            {
                Console.WriteLine($"{key}: {form[key]}");
            }

            // Lấy dữ liệu từ form
            string idmonan = form["idmonan"];
            string idsize = form["SelectedSizeId"];
            string iddebanh = form["SelectedDeBanhId"];
            string ghichu = form["ghichu"];
            string idmonan2 = form["SelectedPizzaGhepId"];
          
            // Parse các giá trị số
            int soluong = 1;
            if (form.ContainsKey("soluong") && !string.IsNullOrEmpty(form["soluong"]))
            {
                int.TryParse(form["soluong"], out soluong);
            }

            // Lấy danh sách toppings
            List<string> toppings = new List<string>();
            if (form.ContainsKey("SelectedToppingIds"))
            {
                toppings = form["SelectedToppingIds"].ToList();
            }

            // Kiểm tra dữ liệu đầu vào
            if (string.IsNullOrEmpty(idmonan))
            {
                return BadRequest("ID món ăn không hợp lệ");
            }

            var product = await _appDbContext.SanPhams
                .Include(p=>p.IdloaimonanNavigation)
                .Where(p => p.Idmonan == idmonan).FirstOrDefaultAsync();

           
            // Kiểm tra product có tồn tại không
            if (product == null)
            {
                return BadRequest("Sản phẩm không tồn tại");
            }

            // Tìm size và đế bánh
            var size = await _appDbContext.Sizes.FindAsync(idsize);
            var debanh = await _appDbContext.debanh.FindAsync(iddebanh);

            // Tính giá
            int giacoban = product.Giamonan;
            string tenmonan2 = null;
            if (!string.IsNullOrEmpty(idmonan2))
            {
                var product2 = await _appDbContext.SanPhams.FindAsync(idmonan2);
                if (product2 != null)
                {
                    giacoban = (giacoban + product2.Giamonan) / 2;
                    tenmonan2 = product2.Tenmonan;
                }
            }

            int giasize = 0;
            if (size != null)
            {
                giasize = await _appDbContext.ListGiaSizes
                   .Where(l => l.Idloaimonan == product.Idloaimonan && l.Idsize == idsize)
                   .Select(l => l.Giasize)
                   .FirstOrDefaultAsync();
            }
            int giadebanh = debanh?.Giadebanh ?? 0;

            // Lấy thông tin toppings
            var toppingObjs = new List<Topping>();
            if (toppings != null && toppings.Count > 0)
            {
                toppingObjs = await _appDbContext.Topping
                    .Where(t => toppings.Contains(t.Idtopping))
                    .Select(t => new Topping
                    {
                        Idtopping = t.Idtopping,
                        Tentopping = t.Tentopping,
                        Giatopping = t.Giatopping
                    }).ToListAsync();
            }

            // Lấy giỏ hàng từ session
            var cart = HttpContext.Session.Get<List<CartItem>>("Cart") ?? new List<CartItem>();

            // Kiểm tra sản phẩm đã tồn tại trong giỏ hàng chưa
            var existingItem = cart.FirstOrDefault(c =>
               c.IDMONAN == idmonan &&
               c.IDMMONAN2 == idmonan2 &&
               c.Size == (size?.Tensize ?? null) &&
               c.DeBanh == (debanh?.Tendebanh ?? null) &&
               c.Topping.Select(t => t.Idtopping).OrderBy(x => x)
                   .SequenceEqual(toppingObjs.Select(t => t.Idtopping).OrderBy(x => x)));

            if (existingItem != null)
            {
                // Nếu đã tồn tại, tăng số lượng
                existingItem.SoLuong += soluong;
            }
            else
            {
                // Nếu chưa tồn tại, thêm mới vào giỏ hàng
                cart.Add(new CartItem
                {
                    IDMONAN = product.Idmonan,
                    IDMMONAN2 = idmonan2,
                    TENSANPHAM = product.Tenmonan,
                    TENSANPHAM2 = tenmonan2,
                    ANHSANPHAM = product.Anhmonan,
                    Size = size?.Tensize,
                    DeBanh = debanh?.Tendebanh,
                    SoLuong = soluong,
                    GhiChu = ghichu,
                    GiaCoBan = giacoban,
                    GiaSize = giasize,
                    GiaDeBanh = giadebanh,
                    Topping = toppingObjs
                });

            }
            var addedItemId = $"{idmonan}_{idmonan2}_{size?.Tensize}_{debanh?.Tendebanh}_{string.Join(",", toppingObjs.Select(t => t.Idtopping))}";
            TempData["SelectedIds"] = new List<string> { addedItemId };
            // Lưu giỏ hàng vào session
            HttpContext.Session.Set("Cart", cart);

            return RedirectToAction("Index", "Products");
        }


        //Hiển thị giỏ hàng
        public async Task<IActionResult> Index()
        {
            var cart = HttpContext.Session.Get<List<CartItem>>("Cart") ?? new List<CartItem>();
            foreach (var item in cart)
            {
                if (!string.IsNullOrEmpty(item.IDMMONAN2))
                {
                    var monan2 = await _appDbContext.SanPhams
                        .Where(p => p.Idmonan == item.IDMMONAN2)
                        .Select(p => new { p.Tenmonan, p.Anhmonan })
                        .FirstOrDefaultAsync();

                    if (monan2 != null)
                    {
                        item.TENSANPHAM2 = monan2.Tenmonan;
                        item.ANHSANPHAM2 = monan2.Anhmonan;
                    }
                }
            }
            var model = new CartPageViewModel
            {
                CartItems = cart,
                UserInfo = new UserCheckoutInfoViewModel()

            };
            model.UserInfo.PaymentInfo = "COD";
            await _accountRepository.FillUserInfoIfAuthenticated(model.UserInfo, User);
            // Lấy địa chỉ từ session
            if (HttpContext.Session.TryGetValue("UserAddress", out var addrBytes))
            {
                model.UserInfo.Address = Encoding.UTF8.GetString(addrBytes);
            }
            else
            {
                model.UserInfo.Address = "Chưa có vị trí";
            }
            var userLocation = GetUserLocationFromSession();
            if (userLocation != null)
            {
                model.UserInfo.DistanceKm = userLocation.DistanceKm;
                model.UserInfo.EstimatedMinutes = userLocation.EstimatedMinutes;
                model.UserInfo.DeliveryMethod = userLocation.DeliveryMethod;
                var branch = await _appDbContext.chinhanh.FindAsync(userLocation.NearestBranchId);
                if (branch != null)
                {
                    model.UserInfo.BranchId = userLocation.NearestBranchId;
                    model.UserInfo.BranchName = branch.Tencnhanh;
                }
            }
            ViewBag.AllSizes = await _appDbContext.Sizes.ToListAsync();
            ViewBag.AllDeBanh = await _appDbContext.debanh.ToListAsync();
            ViewBag.AllToppings = await _appDbContext.Topping
                    .Select(t => new Topping
                    {
                        Idtopping = t.Idtopping,
                        Tentopping = t.Tentopping,
                        Giatopping = t.Giatopping,
                        Idloaimonan = t.Idloaimonan
                    })
                    .ToListAsync();
            // hoặc lọc theo loại nếu cần
            return View(model);
        }

        //Cập nhật số lượng
        [HttpPost]
        public IActionResult UpdateCart(string idmonan, string idmonan2, string? size, string? debanh, List<string>? toppings, int soluong)
        {

            var cart = HttpContext.Session.Get<List<CartItem>>("Cart") ?? new List<CartItem>();

            var item = cart.FirstOrDefault(c =>
                c.IDMONAN == idmonan &&
                c.IDMMONAN2 == idmonan2 &&
                c.Size == (size ?? null) &&
                c.DeBanh == (debanh ?? null) &&
                (c.Topping?.Select(t => t.Idtopping).OrderBy(x => x)
                    .SequenceEqual((toppings ?? new List<string>()).OrderBy(x => x)) ?? toppings == null));

            if (item != null)
            {
                if (soluong <= 0)
                {
                    cart.Remove(item);
                }
                else
                {
                    item.SoLuong = soluong;
                }

                HttpContext.Session.Set("Cart", cart);

                return Json(new
                {
                    success = true,
                    tongTienMoi = item.TongTien // ✅ Tự động tính đúng
                });
            }

            return Json(new { success = false });
        }


        [HttpPost]
        public IActionResult DeleteItem(string idmonan, string idmonan2, string? size, string? debanh, List<string>? toppings)
        {
            var cart = HttpContext.Session.Get<List<CartItem>>("Cart") ?? new List<CartItem>();

            var itemToRemove = cart.FirstOrDefault(c =>
                c.IDMONAN == idmonan &&
                c.IDMMONAN2 == idmonan2 &&
                c.Size == (size ?? null) &&
                c.DeBanh == (debanh ?? null) &&
                (c.Topping?.Select(t => t.Idtopping).OrderBy(x => x)
                    .SequenceEqual((toppings ?? new List<string>()).OrderBy(x => x)) ?? toppings == null));

            if (itemToRemove != null)
            {
                cart.Remove(itemToRemove);
                HttpContext.Session.Set("Cart", cart);
            }
            return RedirectToAction("Index", "Products");
            //return View("Index", cart);
        }

        // DELETE: Xoá toàn bộ giỏ hàng
        [HttpPost]
        public IActionResult CartEmpty()
        {
            HttpContext.Session.Remove("Cart");
            TempData["SuccessMessage"] = "Đã xoá toàn bộ giỏ hàng.";
            return RedirectToAction("Index");
        }

        //Hàm tính số sản phẩm trong icon giỏ hàng
        public IActionResult CartCountPartial()
        {
            var cart = HttpContext.Session.Get<List<CartItem>>("Cart") ?? new List<CartItem>();
            return PartialView("_CartCount", cart);
        }


        [HttpPost]
        public async Task<IActionResult> UpdateCartItem(
         string originalIdmonan, string originalIdmonan2, string originalSizeId, string originalDeBanhId, string originalToppings,
         string idmonan,  string SelectedPizzaGhepId, string selectedSizeId, string selectedDeBanhId,
         List<string> selectedToppingIds, string ghichu, int soluong, IFormCollection form)
        {
            var idmonan2 = form["SelectedPizzaGhepId"];
            var cart = HttpContext.Session.Get<List<CartItem>>("Cart") ?? new List<CartItem>();
           
            // Parse topping cũ
            var originalToppingIds = !string.IsNullOrEmpty(originalToppings)
                ? originalToppings.Split(',').ToList()
                : new List<string>();

            // Tìm sản phẩm cũ trong giỏ hàng (so sánh theo ID + toppings)
            var oldItem = cart.FirstOrDefault(c =>
                c.IDMONAN == originalIdmonan &&
                c.IDMMONAN2 == originalIdmonan2 &&
                c.Size == originalSizeId &&
                c.DeBanh == originalDeBanhId &&
                (
                    (c.Topping != null && originalToppingIds != null &&
                     c.Topping.Select(t => t.Idtopping).OrderBy(x => x)
                              .SequenceEqual(originalToppingIds.OrderBy(x => x)))
                    ||
                    (c.Topping == null && originalToppingIds.Count == 0)
                )
            );

            if (oldItem == null)
                return Json(new { success = false, message = "Không tìm thấy sản phẩm trong giỏ hàng." });

            cart.Remove(oldItem); // Xoá bản cũ
            //  Lấy sản phẩm mới
            var product = await _appDbContext.SanPhams
                .Include(p => p.IdloaimonanNavigation)
                .FirstOrDefaultAsync(p => p.Idmonan == idmonan);

            if (product == null)
                return Json(new { success = false, message = "Sản phẩm không tồn tại." });
       
            //  Lấy size và đế bánh
            var size = await _appDbContext.Sizes.FindAsync(selectedSizeId);
            var debanh = await _appDbContext.debanh.FindAsync(selectedDeBanhId);

            //  Tính giá cơ bản
            int giacoban = product.Giamonan;
            string? tenmonan2 = null;

            if (!string.IsNullOrEmpty(idmonan2))
            {
                var product2 = await _appDbContext.SanPhams.FindAsync(idmonan2);
                if (product2 != null)
                {
                    tenmonan2 = product2.Tenmonan;
                    giacoban = (giacoban + product2.Giamonan) / 2;
                }
            }

            //  Tính giá size theo Idloaimonan
            int giasize = 0;
            if (!string.IsNullOrEmpty(selectedSizeId))
            {
                var giasizeQuery = await _appDbContext.ListGiaSizes
                    .Where(l => l.Idloaimonan == product.Idloaimonan && l.Idsize == selectedSizeId)
                    .Select(l => (int?)l.Giasize)
                    .FirstOrDefaultAsync();

                if (giasizeQuery == null)
                    return Json(new { success = false, message = "Không tìm thấy giá cho size đã chọn." });

                giasize = giasizeQuery.Value;
            }

            int giadebanh = debanh?.Giadebanh ?? 0;

            //  Lấy topping theo ID và loại món ăn
            var toppingObjs = new List<Topping>();
            if (selectedToppingIds != null && selectedToppingIds.Count > 0)
            {
                toppingObjs = await _appDbContext.Topping
                    .Where(t => selectedToppingIds.Contains(t.Idtopping) && t.Idloaimonan == product.Idloaimonan)
                    .Select(t => new Topping
                    {
                        Idtopping = t.Idtopping,
                        Tentopping = t.Tentopping,
                        Giatopping = t.Giatopping,
                        Idloaimonan = t.Idloaimonan
                    })
                    .ToListAsync();

            }
            //  Kiểm tra sản phẩm tương tự đã có trong giỏ hàng chưa
            var existingItem = cart.FirstOrDefault(c =>
                c.IDMONAN == idmonan &&
                c.IDMMONAN2 == idmonan2 &&
                c.Size == selectedSizeId&&
                c.DeBanh == selectedDeBanhId &&
                (
                    (c.Topping != null && selectedToppingIds != null &&
                     c.Topping.Select(t => t.Idtopping).OrderBy(x => x)
                              .SequenceEqual(selectedToppingIds.OrderBy(x => x)))
                    ||
                    (c.Topping == null && (selectedToppingIds == null || selectedToppingIds.Count == 0))
                )
            );

            if (existingItem != null)
            {
                existingItem.SoLuong += soluong;
            }
            else
            {
                var newCartItem = new CartItem
                {
                    IDMONAN = idmonan,
                    TENSANPHAM = product.Tenmonan,
                    IDMMONAN2 = idmonan2,
                    TENSANPHAM2 = tenmonan2,
                    ANHSANPHAM = product.Anhmonan,
                   // Size = selectedSizeId,
                    Size= size?.Tensize ?? null,
                    DeBanh= debanh?.Tendebanh ?? null,
                   // DeBanh = selectedDeBanhId,
                    SoLuong = soluong,
                    GhiChu = ghichu,
                    GiaCoBan = giacoban,
                    GiaSize = giasize,
                    GiaDeBanh = giadebanh,
                    Topping = toppingObjs ?? null
                };

                cart.Add(newCartItem);
            }
            // Lưu lại session
            HttpContext.Session.Set("Cart", cart);
            //return Json(new { success = true});
            return RedirectToAction("Index", "Products");
        }
        //Hàm để lấy thuộc tính theo loại
        [HttpGet]
        public async Task<IActionResult> GetOptionsByMonAnId(string idmonan)
        {
            var monAn = await _appDbContext.SanPhams.FindAsync(idmonan);
            if (monAn == null) return NotFound();

            var idLoai = monAn.Idloaimonan;

            var sizes = await _appDbContext.ListGiaSizes
                .Where(l => l.Idloaimonan == idLoai)
                .Select(l => new {
                    Idsize = l.Idsize,
                    Ten = l.IdsizeNavigation.Tensize
                }).ToListAsync();

            List<object> debanhs = new();
            if (idLoai == "LMA01")
            {
                debanhs = await _appDbContext.debanh
                    .Select(d => new {
                        Iddebanh = d.Iddebanh,
                        Ten = d.Tendebanh
                    }).ToListAsync<object>();
            }

            var toppings = await _appDbContext.Topping
                .Where(t => t.Idloaimonan == idLoai)
                .Select(t => new {
                    Idtopping = t.Idtopping,
                    Ten = t.Tentopping,
                    Gia = t.Giatopping
                }).ToListAsync();

            return Json(new
            {
                sizes,
                debanhs,
                toppings
            });
        }
 

    }
}
