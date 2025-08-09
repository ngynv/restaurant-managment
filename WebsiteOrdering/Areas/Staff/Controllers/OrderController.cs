using MediatR;
using Microsoft.AspNetCore.Mvc;
using IO_Directory = System.IO.Directory;
using Microsoft.EntityFrameworkCore;
using System.Text;
using WebsiteOrdering.Areas.Staff.ViewModels;
using WebsiteOrdering.Helper;
using WebsiteOrdering.Models;
using WebsiteOrdering.Product.GetProductById;
using WebsiteOrdering.Repositories;
using WebsiteOrdering.Services;
using WebsiteOrdering.ViewModels;
using WebsiteOrdering.Enums;

namespace WebsiteOrdering.Areas.Staff.Controllers
{
    [Area("Staff")]
    [Route("[area]/[controller]/[action]")]
    public class OrderController : Controller
    {
        private readonly IMediator _mediator;
        private readonly AppDbContext _appDbContext;
        private const string SESSION_KEY = "AllOrderSessions";
        private readonly IEmailService _emailService;
        private readonly IProductRepository _productRepository;
        public OrderController(IMediator mediator, AppDbContext context, IEmailService emailService, IProductRepository productRepository)
        {
            _emailService = emailService;
            _mediator = mediator;
            _appDbContext = context;
            _productRepository = productRepository;
        }

        private static string GenerateRandomId(int length = 5)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }


        private List<string> GetAllChildCategoryIds(List<Loaimonan> categories, string parentId)
        {
            var result = new List<string>();
            var children = categories.Where(c => c.IdloaimanCha == parentId).ToList();
            foreach (var child in children)
            {
                result.Add(child.Idloaimonan);
                result.AddRange(GetAllChildCategoryIds(categories, child.Idloaimonan));
            }
            return result;
        }

        public async Task<IActionResult> Index(string? idLoai = null, string? idDatBan = null)
        {
            var loaiMonList = _appDbContext.Category.ToList();
            var monAnList = new List<Monan>();

            if (!string.IsNullOrEmpty(idLoai))
            {
                var allChildrenIds = GetAllChildCategoryIds(loaiMonList, idLoai);
                allChildrenIds.Add(idLoai);

                monAnList = _appDbContext.SanPhams
                    .Where(m => allChildrenIds.Contains(m.Idloaimonan))
                    .ToList();
            }

            // Lấy đúng session từ dictionary
            var allSessions = HttpContext.Session.Get<Dictionary<string, OrderSessionViewModel>>(SESSION_KEY);
            OrderSessionViewModel? orderSession = null;

            if (!string.IsNullOrEmpty(idDatBan) && allSessions != null && allSessions.ContainsKey(idDatBan))
            {
                orderSession = allSessions[idDatBan];
            }
            else if (allSessions != null && allSessions.Any())
            {
                orderSession = allSessions.Values.Last(); // Lấy session cuối
            }
            else
            {
                orderSession = new OrderSessionViewModel();
            }


            var vm = new OrderTaiChoViewModel
            {
                LoaiMonList = loaiMonList,
                MonAnList = monAnList,
                OrderSession = orderSession,
                PizzaGhepList = new List<Monan>()
            };

            return View(vm);
        }

        public async Task<IActionResult> ChiTietMon(string idMon, string? idDatBan, string? idPizzaGhep = null, string? idSize = null, string? idDeBanh = null, string? toppings = null, int? soLuong = null, string? ghiChu = null, bool isUpdate = false, string? idPizzaGhepCu = null)
        {
            // Lấy chi tiết món bằng MediatR
            var monAn = await _mediator.Send(new GetProductsByIdQuery(idMon));

            if (monAn == null)
            {
                TempData["Error"] = "Không tìm thấy món ăn!";
                return RedirectToAction("Index", new { idDatBan });
            }

            // Lấy loại món ăn
            var loaiMonList = _appDbContext.Category.ToList();

            // Lấy pizza ghép (nếu là pizza)
            List<Monan> pizzaGhepList = new();
            List<Debanh>? listDeBanh = null;
            if (monAn.Idloaimonan == "LMA01")
            {
                pizzaGhepList = _appDbContext.SanPhams
                                .Where(m => m.Idloaimonan == "LMA01" && m.Idmonan != idMon)
                                .ToList();

                listDeBanh = _appDbContext.debanh.ToList();
            }

            // Lấy session bàn
            var allSessions = HttpContext.Session.Get<Dictionary<string, OrderSessionViewModel>>(SESSION_KEY);
            OrderSessionViewModel? orderSession = null;
            if (!string.IsNullOrEmpty(idDatBan) && allSessions != null && allSessions.ContainsKey(idDatBan))
            {
                orderSession = allSessions[idDatBan];
            }
            else
            {
                orderSession = new OrderSessionViewModel { IdDatBan = idDatBan };
            }

            // Tạo viewmodel
            var model = new OrderTaiChoViewModel
            {
                LoaiMonList = loaiMonList,
                ChiTietMon = monAn,
                PizzaGhepList = pizzaGhepList,
                OrderSession = orderSession,
                IsUpdate = isUpdate
            };

            // Các giá trị selected
            if (isUpdate)
            {
                model.IdSizeSelected = idSize;
                model.IdDeBanhSelected = idDeBanh;
                model.IdPizzaGhepSelected = idPizzaGhep;
                model.ToppingsSelected = toppings?.Split(',').ToList() ?? new List<string>();
                model.SoLuongSelected = soLuong ?? 1;
                model.GhiChuSelected = ghiChu;

            }
            else
            {
                if (monAn.ListGiaSizes != null && monAn.ListGiaSizes.Any())
                    model.IdSizeSelected = monAn.ListGiaSizes.First().Idsize;

                if (listDeBanh != null && listDeBanh.Any())
                    model.IdDeBanhSelected = listDeBanh.First().Iddebanh;

                model.ToppingsSelected = new List<string>();
                model.SoLuongSelected = 1;
                model.GhiChuSelected = "";
            }

            return View("Index", model);
        }

        [HttpPost]
        public IActionResult TaoDonHangTaiCho(string idDatBan)
        {
            var newSession = new OrderSessionViewModel
            {
                IdDatBan = idDatBan,
                ChiTietMonAn = new List<CartItem>(),
                TrangThai = "Đang dùng bữa"
            };

            // Lấy all sessions
            var allSessions = HttpContext.Session.Get<Dictionary<string, OrderSessionViewModel>>(SESSION_KEY) ?? new();

            // Thêm hoặc cập nhật session của bàn
            allSessions[idDatBan] = newSession;

            HttpContext.Session.Set(SESSION_KEY, allSessions);

            var datban = _appDbContext.Datbans.FirstOrDefault(d => d.Iddatban == idDatBan);
            if (datban != null)
            {
                datban.Trangthaidatban = TrangThai.Eating;
                _appDbContext.SaveChanges();
            }

            return RedirectToAction("Index", new { idDatBan });
        }

        [HttpPost]
        public IActionResult ThemMonVaoSession(string idMon, string? idSize, string? idDeBanh, string[] toppings, int soLuong, string? ghiChu, string? idPizzaGhep = null)
        {
            var allSessions = HttpContext.Session.Get<Dictionary<string, OrderSessionViewModel>>(SESSION_KEY);
            if (allSessions == null)
            {
                TempData["Error"] = "Không tìm thấy danh sách đơn tạm. Vui lòng tạo đơn hàng trước.";
                return RedirectToAction("Index");
            }

            var currentSession = allSessions.Values.LastOrDefault();
            if (currentSession == null || string.IsNullOrEmpty(currentSession.IdDatBan))
            {
                TempData["Error"] = "Không tìm thấy phiên đặt bàn. Vui lòng tạo đơn hàng trước.";
                return RedirectToAction("Index");
            }

            var idDatBan = currentSession.IdDatBan;
            var session = allSessions[idDatBan];

            var monAn = _appDbContext.SanPhams
                .Include(m => m.Toppings)
                .FirstOrDefault(m => m.Idmonan == idMon);

            if (monAn == null)
            {
                TempData["Error"] = "Không tìm thấy món ăn.";
                return RedirectToAction("Index");
            }

            // Lấy danh sách size theo Idloaimonan
            var listGiaSizes = _appDbContext.ListGiaSizes
                .Include(l => l.IdsizeNavigation)
                .Where(l => l.Idloaimonan == monAn.Idloaimonan)
                .ToList();

            // Nếu món này có size nhưng chưa chọn size → gán mặc định
            if (listGiaSizes.Any() && string.IsNullOrEmpty(idSize))
            {
                idSize = listGiaSizes.First().Idsize;
            }

            //Tìm size được chọn từ listGiaSizes
            var selectedSize = listGiaSizes.FirstOrDefault(x => x.Idsize == idSize);

            if (listGiaSizes.Any() && selectedSize == null)
            {
                TempData["Error"] = "Size không tồn tại cho món này!";
                return RedirectToAction("Index");
            }

            var selectedToppings = _appDbContext.Topping.Where(t => toppings.Contains(t.Idtopping)).ToList();
            var debanh = _appDbContext.debanh.FirstOrDefault(d => d.Iddebanh == idDeBanh);

            Monan? monGhep = null;
            if (!string.IsNullOrEmpty(idPizzaGhep))
                monGhep = _appDbContext.SanPhams.FirstOrDefault(m => m.Idmonan == idPizzaGhep);

            var cartItem = new CartItem(monAn, idSize, listGiaSizes, selectedToppings, debanh, ghiChu ?? "", monGhep)
            {
                SoLuong = soLuong
            };

            if (selectedSize != null && selectedSize.IdsizeNavigation != null)
            {
                cartItem.TenSize = selectedSize.IdsizeNavigation.Tensize;
            }

            if (monGhep != null)
            {
                cartItem.IDMMONAN2 = monGhep.Idmonan;
                cartItem.TENSANPHAM2 = monGhep.Tenmonan;
            }

            session.ChiTietMonAn.Add(cartItem);
            allSessions[idDatBan] = session;
            HttpContext.Session.Set(SESSION_KEY, allSessions);

            TempData["Success"] = "Đã thêm món vào giỏ hàng thành công!";
            return RedirectToAction("Index", new { idLoai = monAn.Idloaimonan, idDatBan = idDatBan });
        }

        [HttpPost]
        public IActionResult LuuDonTaiCho(string idDatBan)
        {
            var allSessions = HttpContext.Session.Get<Dictionary<string, OrderSessionViewModel>>(SESSION_KEY);

            if (allSessions == null || !allSessions.ContainsKey(idDatBan))
                return NotFound("Không tìm thấy phiên đặt bàn.");

            var orderSession = allSessions[idDatBan];

            var datban = _appDbContext.Datbans.FirstOrDefault(d => d.Iddatban == idDatBan);
            if (datban != null)
            {
                datban.Trangthaidatban = TrangThai.Ordered;
                _appDbContext.SaveChanges();
            }

            TempData["Success"] = "Lưu đơn tạm thành công! Khi khách thanh toán, sẽ lưu đơn chính thức.";
            return RedirectToAction("DanhSachKhachDaDen", "BookingStaff");

        }



        public IActionResult XemHoaDonDatBan(string idDatBan)
        {
            var allSessions = HttpContext.Session.Get<Dictionary<string, OrderSessionViewModel>>(SESSION_KEY);

            if (allSessions == null || !allSessions.ContainsKey(idDatBan))
            {
                TempData["Error"] = "Không tìm thấy hóa đơn tạm hoặc hóa đơn đã được thanh toán.";
                return RedirectToAction("DanhSachKhachDaDen", "BookingStaff");
            }

            var orderSession = allSessions[idDatBan];

            var vm = new OrderTaiChoViewModel
            {
                LoaiMonList = _appDbContext.Category.ToList(),
                MonAnList = new List<Monan>(),
                ChiTietMon = null,
                OrderSession = orderSession,
                AllSessions = allSessions
            };

            return View("Index", vm);
        }

        [HttpPost]
        public IActionResult XoaMonTrongSession(string idDatBan, string idMon, string? idPizzaGhep = null, string? idSize = null)
        {
            var allSessions = HttpContext.Session.Get<Dictionary<string, OrderSessionViewModel>>(SESSION_KEY);
            if (allSessions == null || !allSessions.ContainsKey(idDatBan))
                return NotFound("Không tìm thấy session.");

            var session = allSessions[idDatBan];

            // Tìm món theo id
            var itemToRemove = session.ChiTietMonAn.FirstOrDefault(x =>
                                                                    x.IDMONAN == idMon &&
                                                                    (x.IDMMONAN2 ?? "") == (idPizzaGhep ?? "") &&
                                                                    (x.Size ?? "") == (idSize ?? ""));

            if (itemToRemove != null)
            {
                session.ChiTietMonAn.Remove(itemToRemove);
                allSessions[idDatBan] = session;
                HttpContext.Session.Set(SESSION_KEY, allSessions);

                TempData["Success"] = "Xóa món thành công!";
            }
            else
            {
                TempData["Error"] = "Không tìm thấy món để xóa!";
            }

            return RedirectToAction("Index", new { idDatBan });
        }

        [HttpPost]
        public async Task<IActionResult> CapNhatMonTrongSession(string idDatBan, string idMon, int soLuong, string? ghiChu, string? idPizzaGhep = null, string? idSize = null, string? idDeBanh = null, string[]? toppings = null)
        {
            var allSessions = HttpContext.Session.Get<Dictionary<string, OrderSessionViewModel>>(SESSION_KEY);
            if (allSessions == null || !allSessions.ContainsKey(idDatBan))
                return NotFound("Không tìm thấy session.");

            var session = allSessions[idDatBan];
            var monAn = await _mediator.Send(new GetProductsByIdQuery(idMon));

            // Map idSize => tên
            string? tenSize = null;
            if (!string.IsNullOrEmpty(idSize) && monAn.ListGiaSizes != null)
            {
                var size = monAn.ListGiaSizes.FirstOrDefault(s => s.Idsize == idSize);
                if (size != null)
                {
                    tenSize = size.IdsizeNavigation.Tensize;
                }
            }

            // Map idDeBanh => tên
            string? tenDeBanh = null;
            var listDeBanh = _appDbContext.debanh.ToList();
            if (!string.IsNullOrEmpty(idDeBanh))
            {
                var de = listDeBanh.FirstOrDefault(d => d.Iddebanh == idDeBanh);
                if (de != null)
                {
                    tenDeBanh = de.Tendebanh;
                }
            }
            // Tìm món
            var itemToUpdate = session.ChiTietMonAn.FirstOrDefault(x => x.IDMONAN == idMon);

            // Load topping
            var selectedToppings = new List<Topping>();
            if (toppings != null && toppings.Any())
            {
                selectedToppings = _appDbContext.Topping
                                  .Where(t => toppings.Contains(t.Idtopping))
                                  .ToList();
            }

            // Load pizza ghép
            Monan? monGhep = null;
            if (!string.IsNullOrEmpty(idPizzaGhep))
            {
                monGhep = _appDbContext.SanPhams.FirstOrDefault(m => m.Idmonan == idPizzaGhep);
            }


            if (itemToUpdate != null)
            {
                itemToUpdate.SoLuong = soLuong;
                itemToUpdate.GhiChu = ghiChu ?? itemToUpdate.GhiChu;
                itemToUpdate.Size = tenSize;
                itemToUpdate.DeBanh = tenDeBanh;
                itemToUpdate.IDMMONAN2 = idPizzaGhep;
                if (monGhep != null)
                {
                    itemToUpdate.TENSANPHAM2 = monGhep.Tenmonan;
                }
                itemToUpdate.Topping = selectedToppings;

                allSessions[idDatBan] = session;
                HttpContext.Session.Set(SESSION_KEY, allSessions);

                TempData["Success"] = "Cập nhật món thành công!";
            }
            else
            {
                TempData["Error"] = "Không tìm thấy món để cập nhật!";
            }

            return RedirectToAction("Index", new { idDatBan });
        }

        //Hàm này để lấy lại chỗ cập nhật món ăn để hiển thị lại chỗ Index
        [HttpPost]
        public async Task<IActionResult> PrepareUpdateMon(string idDatBan, string idMon, string? idPizzaGhep = null, string? idSize = null)
        {
            var allSessions = HttpContext.Session.Get<Dictionary<string, OrderSessionViewModel>>(SESSION_KEY);
            if (allSessions == null || !allSessions.ContainsKey(idDatBan))
                return NotFound("Không tìm thấy session.");

            var session = allSessions[idDatBan];

            var item = session.ChiTietMonAn.FirstOrDefault(x =>
                x.IDMONAN == idMon &&
                (x.IDMMONAN2 ?? "") == (idPizzaGhep ?? "") &&
                (x.Size ?? "") == (idSize ?? ""));

            if (item == null)
            {
                TempData["Error"] = "Không tìm thấy món để chỉnh sửa!";
                return RedirectToAction("Index", new { idDatBan });
            }

            // Lấy monAn từ DB
            var monAn = await _mediator.Send(new GetProductsByIdQuery(item.IDMONAN));

            if (monAn == null)
            {
                TempData["Error"] = "Không tìm thấy món ăn trong DB!";
                return RedirectToAction("Index", new { idDatBan });
            }

            // Lấy danh sách đế bánh nếu là pizza
            List<Debanh>? listDeBanh = null;
            if (monAn.Idloaimonan == "LMA01")
            {
                listDeBanh = _appDbContext.debanh.ToList();
            }

            // Map lại idSize (vì trong CartItem bạn đang lưu tên)
            string? idSizeFinal = null;
            if (!string.IsNullOrEmpty(item.Size) && monAn.ListGiaSizes != null)
            {
                var size = monAn.ListGiaSizes.FirstOrDefault(s => s.IdsizeNavigation.Tensize == item.Size);
                if (size != null)
                {
                    idSizeFinal = size.Idsize;
                }
            }

            // Map lại idDeBanh (vì trong CartItem bạn đang lưu tên)
            string? idDeBanhFinal = null;
            if (!string.IsNullOrEmpty(item.DeBanh) && listDeBanh != null)
            {
                var de = listDeBanh.FirstOrDefault(d => d.Tendebanh == item.DeBanh);
                if (de != null)
                {
                    idDeBanhFinal = de.Iddebanh;
                }
            }

            var toppingIds = item.Topping?.Select(t => t.Idtopping).ToArray() ?? Array.Empty<string>();

            return RedirectToAction("ChiTietMon", new
            {
                idMon = item.IDMONAN,
                idDatBan = idDatBan,
                idPizzaGhep = item.IDMMONAN2,
                idSize = idSizeFinal,
                idDeBanh = idDeBanhFinal,
                toppings = string.Join(",", toppingIds),
                soLuong = item.SoLuong,
                ghiChu = item.GhiChu,
                isUpdate = true
            });
        }



        public IActionResult ChuanBiThanhToan(string idDatBan)
        {
            var allSessions = HttpContext.Session.Get<Dictionary<string, OrderSessionViewModel>>(SESSION_KEY);
            if (allSessions == null || !allSessions.ContainsKey(idDatBan))
                return NotFound("Không tìm thấy session.");

            var orderSession = allSessions[idDatBan];

            var datban = _appDbContext.Datbans
                .Include(d => d.Chitietdatbans)
                    .ThenInclude(ct => ct.IdbanNavigation)

                .Include(d => d.IdchinhanhNavigation)
                .FirstOrDefault(d => d.Iddatban == idDatBan);

            if (datban == null) return NotFound();

            var chitietBan = datban.Chitietdatbans.FirstOrDefault();

            var vm = new HoaDonThanhToanViewModel
            {
                OrderSession = orderSession,
                Datban = datban,
                Chitietdatban = chitietBan,
            };

            return View("HoaDonTaiCho", vm);
        }

        [HttpPost]
        public async Task<IActionResult> ThanhToanTaiCho(string idDatBan, HoaDonThanhToanViewModel model)
        {
            var allSessions = HttpContext.Session.Get<Dictionary<string, OrderSessionViewModel>>(SESSION_KEY);
            if (allSessions == null || !allSessions.ContainsKey(idDatBan))
                return NotFound("Không tìm thấy session.");

            var orderSession = allSessions[idDatBan];
            var tienKhachDua = model.TienKhachDua;
            var phuongThucThanhToan = model.PhuongThucThanhToan;

            var datban = await _appDbContext.Datbans
                                .Include(d => d.Nguoidung)
                                .Include(d => d.Chitietdatbans)
                                    .ThenInclude(ct => ct.IdbanNavigation)
                                .Include(d => d.IdchinhanhNavigation)
                                .FirstOrDefaultAsync(d => d.Iddatban == idDatBan);


            if (datban == null) return NotFound();

            var chitietBan = datban.Chitietdatbans.FirstOrDefault();
            if (chitietBan == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin bàn.";
                return RedirectToAction("Index");
            }

            // Gán giờ ra thực tế
            chitietBan.Giora = TimeOnly.FromDateTime(DateTime.Now);

            var tongTien = orderSession.TongTien;
            int tienThua = 0;

            if (phuongThucThanhToan == "TienMat" && tienKhachDua.HasValue)
            {
                tienThua = tienKhachDua.Value - (int)tongTien;
                if (tienThua < 0)
                {
                    TempData["Error"] = "Tiền khách đưa không đủ!";
                    return RedirectToAction("XemHoaDonDatBan", new { idDatBan });
                }
            }

            // Tạo đơn hàng
            var donhang = new Donhang
            {
                Iddonhang = GenerateRandomId(), // Sử dụng hàm tạo ID
                Iddatban = datban.Iddatban,
                Ngaydat = DateTime.Now,
                Tenkh = datban.Tenngdat,
                Sdtkh = datban.Sđtngdat,
                Diachidh = datban.Idchinhanh, // Để null, không bắt buộc lấy ghi chú
                Songuoi = datban.Songuoidat,
                Trangthai = TrangThai.Completed,
                Tongtien = tongTien,
                Ptttoan = phuongThucThanhToan,
                Idchinhanh = datban.Idchinhanh
            };

            _appDbContext.dhang.Add(donhang);
            await _appDbContext.SaveChangesAsync();
            var monAnHtml = new StringBuilder();
            // Lưu chi tiết món
            foreach (var item in orderSession.ChiTietMonAn)
            {
                var Iddebanh = await _appDbContext.debanh
            .Where(d => d.Tendebanh == item.DeBanh)
            .Select(d => d.Iddebanh)
            .FirstOrDefaultAsync();
                var idSize = await _appDbContext.Sizes
           .Where(d => d.Tensize == item.Size)
           .Select(d => d.Idsize)
           .FirstOrDefaultAsync();
                var ctdh = new Chitietdonhang
                {
                    IdChitiet = GenerateRandomId(),
                    Iddonhang = donhang.Iddonhang,
                    Idmonan = item.IDMONAN,
                    Idmonan2 = item.IDMMONAN2,
                    Idsize = idSize,
                    Iddebanh = Iddebanh,
                    Kieupizza = item.IDMMONAN2 != null ? "Ghép" : "Nguyên",
                    Soluong = item.SoLuong,
                    Dongia = item.GiaCoBan,
                    Tongtiendh = item.TongTien,
                    Ghichu = item.GhiChu
                };
                _appDbContext.ctdh.Add(ctdh);

                foreach (var topping in item.Topping)
                {
                    var ctTopping = new Chitiettopping
                    {
                        IdChitiet = ctdh.IdChitiet,
                        Idtopping = topping.Idtopping
                    };
                    _appDbContext.cttopping.Add(ctTopping);
                }

                var tenSanPham = item.TENSANPHAM;
                if (!string.IsNullOrEmpty(item.TENSANPHAM2))
                {
                    tenSanPham += $" (Ghép với: {item.TENSANPHAM2})";
                }
                if (!string.IsNullOrEmpty(item.GhiChu))
                {
                    tenSanPham += $"<br/><i>Ghi chú: {item.GhiChu}</i>";
                }

                var danhSachTopping = item.Topping?.Select(t => t.Tentopping).ToList();
                var toppingText = (danhSachTopping != null && danhSachTopping.Any())
                    ? string.Join(", ", danhSachTopping)
                    : "Không";

                monAnHtml.AppendLine($@"
                <tr>
                    <td>{tenSanPham}</td>
                    <td>{item.Size}</td>
                    <td>{item.DeBanh}</td>
                    <td>{toppingText}</td>
                    <td>{item.SoLuong}</td>
                    <td>{item.TongTien:N0} đ</td>
                </tr>");

            }
            // Cập nhật trạng thái bàn
            datban.Trangthaidatban = TrangThai.Completed;
            await _appDbContext.SaveChangesAsync();
            //Cập nhật số lượng bán được món ăn và cập nhật nếu có pizza ghép
            await _productRepository.CapNhatSoLuongBanVaGhepAsync(donhang.Iddonhang);
            var email = datban.Nguoidung?.Email;
            if (!string.IsNullOrEmpty(email))
            {
                var placeholders = new Dictionary<string, string>
                {
                    ["MaDonHang"] = donhang.Iddonhang,
                    ["TenKhachHang"] = donhang.Tenkh ?? "Khách hàng",
                    ["SDT"] = donhang.Sdtkh,
                    ["DiaChi"] = datban.IdchinhanhNavigation.Tencnhanh,
                    ["SoNguoi"] = datban.Songuoidat.ToString(),
                    ["TenBan"] = chitietBan.IdbanNavigation?.Tenban,
                    ["NgayDat"] = donhang.Ngaydat.ToString("dd/MM/yyyy HH:mm"),
                    ["GioVao"] = datban.Giobatdau.ToString(),
                    ["GioRa"] = chitietBan.Giora.ToString(),
                    ["MonAnHtml"] = monAnHtml.ToString(),
                    ["TrangThaiThanhToan"] = datban.Trangthaidatban.ToString(),
                    ["TongTien"] = $"{tongTien:N0} đ",
                    ["PhuongThucThanhToan"] = phuongThucThanhToan == "TienMat" ? "Tiền mặt" : "Chuyển khoản",
                    ["IsTienMat"] = (phuongThucThanhToan == "TienMat").ToString().ToLower(),
                    ["TienKhachDua"] = (phuongThucThanhToan == "TienMat" && tienKhachDua.HasValue) ? $"{tienKhachDua.Value:N0} đ" : "",
                    ["TienThua"] = (phuongThucThanhToan == "TienMat" && tienKhachDua.HasValue) ? $"{tienThua:N0} đ" : ""
                };

                var templatePath = Path.Combine(IO_Directory.GetCurrentDirectory(), "Templates", "EmailHoaDonDatBan.html");
                var body = EmailTemplateHelper.PopulateTemplate(templatePath, placeholders);

                await _emailService.SendEmailAsync(email, $"Hóa đơn thanh toán - Mã đơn: {donhang.Iddonhang}", body);
            }
            // Xóa session
            allSessions.Remove(idDatBan);
            HttpContext.Session.Set(SESSION_KEY, allSessions);

          //  TempData["Success"] = $"Thanh toán thành công! {(tienKhachDua.HasValue ? $"Tiền thừa: {tienThua:N0} đ" : "")}";
            return RedirectToAction("Index", "BookingStaff");

        }

        public async Task<IActionResult> DanhSachHoaDon()
        {
            var chiNhanhId = User.FindFirst("ChiNhanhId")?.Value;
            if (string.IsNullOrEmpty(chiNhanhId))
            {
                TempData["Error"] = "Không xác định được chi nhánh nhân viên.";
                return RedirectToAction("LoginStaff", "Admin", new { area = "Admin" });
            }

            var danhSach = await _appDbContext.dhang
                .Include(d => d.IdchinhanhNavigation)
                .Where(d => d.Trangthai == TrangThai.Completed && d.Idchinhanh == chiNhanhId)
                .OrderByDescending(d => d.Ngaydat)
                .ToListAsync();

            return View("DanhSachHoaDon", danhSach);
        }


    }
}
