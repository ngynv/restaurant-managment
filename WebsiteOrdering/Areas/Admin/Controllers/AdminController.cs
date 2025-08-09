using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteOrdering.Models;
using WebsiteOrdering.Repositories;
using WebsiteOrdering.Services;
using WebsiteOrdering.ViewModels;
using WebsiteOrdering.Enums;

namespace WebsiteOrdering.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Route("Account")]
    [Route("[area]/[controller]/[action]")]
   // [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IAccountRepository _accountRepository;
        private readonly AppDbContext _appDbContext;
        public AdminController(IAccountRepository accountRepository,AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
            _accountRepository = accountRepository;
          
        }
        public async Task<IActionResult> Index(string maDon, string sđt, string tenNguoiDat)
        {
            List<Datban> result = new List<Datban>();
            var dsChiNhanh = _appDbContext.chinhanh.ToList();
            ViewBag.DanhSachChiNhanh = dsChiNhanh;
            if (!string.IsNullOrWhiteSpace(maDon) || !string.IsNullOrWhiteSpace(sđt) || !string.IsNullOrWhiteSpace(tenNguoiDat))
            {
                var query = _appDbContext.Datbans
                    .Include(d => d.Nguoidung)
                    .Include(d => d.IdchinhanhNavigation)
                    .Include(d => d.Chitietdatbans)
                        .ThenInclude(ct => ct.IdbanNavigation)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(maDon))
                    query = query.Where(d => d.Iddatban != null && d.Iddatban.Trim().ToLower() == maDon.Trim().ToLower());

                if (!string.IsNullOrWhiteSpace(sđt))
                    query = query.Where(d => d.Sđtngdat != null && d.Sđtngdat.Contains(sđt.Trim()));

                if (!string.IsNullOrWhiteSpace(tenNguoiDat))
                    query = query.Where(d => d.Tenngdat != null && d.Tenngdat.Contains(tenNguoiDat.Trim()));

                result = await query.OrderByDescending(d => d.Ngaydat).ToListAsync();
            }

            return View(result);
        }


        [HttpGet]
        public IActionResult LoginStaff()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LoginStaff(LoginViewModel model)
        {
            Console.WriteLine("ModelState.IsValid = " + ModelState.IsValid);
            Console.WriteLine("Email = " + model?.Email);
            Console.WriteLine("Password = " + model?.Password);
            if (!ModelState.IsValid) return View(model);

            var result = await _accountRepository.LoginAsync(model);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Tài khoản hoặc mật khẩu không đúng.");
                return View(model);
            }

            var user = await _accountRepository.GetUserByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Không tìm thấy người dùng.");
                return View(model);
            }

            var roles = await _accountRepository.GetUserRolesAsync(user);
            Console.WriteLine("User roles: " + string.Join(", ", roles));
            // Điều hướng theo vai trò
            if (roles.Contains("Admin"))
            {
                return RedirectToAction("Index", "Admin", new { area = "Admin" });
            }
            else if (roles.Contains("Staff"))
            {
                if (user.IdchinhanhNavigation?.Idchinhanh == null)
                {
                    ModelState.AddModelError("", "Nhân viên chưa được gán chi nhánh.");
                    return View(model);
                }

                // Lưu ID chi nhánh vào session để xử lý sau này
                //HttpContext.Session.SetString("ChiNhanhId", user.IdchinhanhNavigation.Idchinhanh.ToString());

                var signInSuccess = await _accountRepository.SignInStaffWithClaimsAsync(user);
                if (!signInSuccess)
                {
                    ModelState.AddModelError("", "Đăng nhập không thành công.");
                    return View(model);
                }

                return RedirectToAction("Index", "Staff", new { area = "Staff" });
            }
            else
            {
                ModelState.AddModelError("", "Tài khoản không có quyền truy cập vào khu vực này.");
                return View(model);
            }
        }


      //Logout admin chưa được
        [HttpPost]
        public async Task<IActionResult> LogoutAdmin()
        {
            await _accountRepository.LogoutAsync();
            //return Redirect("/");
            return RedirectToAction("Login", "Account");
        }

        //Lấy danh sách khu vực theo chi nhánh
        [HttpGet]
        public IActionResult GetKhuvucByChinhanh(string idChinhanh)
        {
            var khuvucs = _appDbContext.bans
                .Where(b => b.Idchinhanh == idChinhanh)
                .Select(b => b.Khuvuc)
                .Distinct()
                .ToList();

            return Json(khuvucs);
        }
        //Lấy danh sách bàn theo khu vực
        [HttpGet]
        public IActionResult GetBanByKhuvuc(string idChinhanh, string khuvuc)
        {
            var bans = _appDbContext.bans
                .Where(b => b.Idchinhanh == idChinhanh && b.Khuvuc == khuvuc)
                .Select(b => new { b.Idban, b.Tenban, b.Songuoi, b.X, b.Y })
                .ToList();

            return Json(bans);
        }

        private static string GenerateRandomId(int length = 5)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        [HttpGet]
        public IActionResult GetBanDaDat(string ngay, string gio, string idChinhanh, string idKhuvuc)
        {
            var gioBatDau = TimeOnly.Parse(gio);
            var gioKetThuc = gioBatDau.Add(TimeSpan.FromHours(2));

            var danhSachBan = _appDbContext.chitietdatbans
                .Include(c => c.IddatbanNavigation)
                .Include(c => c.IdbanNavigation)
                .Where(c =>
                    c.IddatbanNavigation.Ngaydat == DateOnly.Parse(ngay) &&
                    c.IddatbanNavigation.Idchinhanh == idChinhanh &&
                    c.IdbanNavigation.Khuvuc == idKhuvuc &&
                    c.IddatbanNavigation.Trangthaidatban != TrangThai.Cancelled &&
                    c.IddatbanNavigation.Trangthaidatban != TrangThai.Confirmed &&
                    (
                        (gioBatDau >= c.Giovao && gioBatDau < c.Giora) ||
                        (gioKetThuc > c.Giovao && gioKetThuc <= c.Giora) ||
                        (gioBatDau <= c.Giovao && gioKetThuc >= c.Giora)
                    )
                )
                .Select(c => new
                {
                    idban = c.IdbanNavigation.Idban,
                    trangThai = c.IddatbanNavigation.Trangthaidatban
                })
                .ToList();

            return Json(danhSachBan);
        }

        // Thêm các method này vào Staff Controller

        // Lấy danh sách ban lock theo bàn 
        [HttpGet]
        public IActionResult GetBanLockAllByBan(string idBan)
        {
            var banLocks = _appDbContext.Banlock
                .Where(bl => bl.IdBan == idBan)
                .OrderBy(bl => bl.Ngay)
                .ThenBy(bl => bl.BatDau)
                .Select(bl => new {
                    bl.IdBanLock,
                    bl.IdBan,
                    BatDau = bl.BatDau.ToString("HH:mm"),
                    KetThuc = bl.KetThuc.ToString("HH:mm"),
                    Ngay = bl.Ngay.ToString("yyyy-MM-dd")
                })
                .ToList();

            return Json(banLocks);
        }


        [HttpPost]
        public IActionResult AddBanLock(string idBan, TimeOnly batDau, TimeOnly ketThuc, DateOnly ngay)
        {
            try
            {

                var BatDau = TimeOnly.Parse(batDau.ToString());
                var KetThuc = TimeOnly.Parse(ketThuc.ToString());
                var Ngay = DateOnly.Parse(ngay.ToString());

                if (BatDau >= KetThuc)
                {
                    return Json(new { success = false, message = "Giờ kết thúc phải sau giờ bắt đầu" });
                }

                var existingLock = _appDbContext.Banlock
                    .Where(bl => bl.IdBan == idBan && bl.Ngay == Ngay)
                    .Where(bl => (BatDau < bl.KetThuc && KetThuc > bl.BatDau))
                    .FirstOrDefault();

                if (existingLock != null)
                {
                    return Json(new { success = false, message = "Khung giờ này đã bị trùng với khung giờ khác" });
                }

                var banLock = new Banlock
                {
                    IdBanLock = GenerateRandomId(),
                    IdBan = idBan,
                    BatDau = BatDau,
                    KetThuc = KetThuc,
                    Ngay = Ngay
                };

                _appDbContext.Banlock.Add(banLock);
                _appDbContext.SaveChanges();

                return Json(new { success = true, message = "Thêm khung giờ lock thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }


        // Xóa ban lock
        [HttpDelete]
        public IActionResult DeleteBanLock(string idBanLock)
        {
            try
            {
                var banLock = _appDbContext.Banlock.Find(idBanLock);
                if (banLock == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy khung giờ lock" });
                }

                _appDbContext.Banlock.Remove(banLock);
                _appDbContext.SaveChanges();

                return Json(new { success = true, message = "Xóa khung giờ lock thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }


        // Cập nhật method GetBanTheoTrangThai để bao gồm trạng thái lock
        [HttpGet]
        public IActionResult GetBanTheoTrangThai()
        {
            var chiNhanhId = User.FindFirst("ChiNhanhId")?.Value;
            var currentTime = TimeOnly.FromDateTime(DateTime.Now);
            var currentDate = DateOnly.FromDateTime(DateTime.Now);

            var dsBan = _appDbContext.bans
                .Where(b => b.Idchinhanh == chiNhanhId)
                .Select(b => new
                {
                    b.Idban,
                    b.Tenban,
                    b.X,
                    b.Y,
                    // Kiểm tra trạng thái đặt bàn
                    TrangThaiDatBan = _appDbContext.chitietdatbans
                        .Include(ct => ct.IddatbanNavigation)
                        .Where(ct => ct.Idban == b.Idban
                            && ct.IddatbanNavigation.Ngaydat == currentDate
                            && ct.IddatbanNavigation.Trangthaidatban != TrangThai.Cancelled
                            && ct.IddatbanNavigation.Trangthaidatban != TrangThai.Confirmed)
                        .OrderByDescending(ct => ct.IddatbanNavigation.Ngaydat)
                        .Select(ct => ct.IddatbanNavigation.Trangthaidatban)
                        .FirstOrDefault(),
                    // Kiểm tra trạng thái lock
                    IsLocked = _appDbContext.Banlock
                        .Any(bl => bl.IdBan == b.Idban
                            && bl.Ngay == currentDate
                            && bl.BatDau <= currentTime
                            && bl.KetThuc > currentTime)
                })
                .ToList();

            return Json(dsBan);
        }
        [HttpGet]
        public IActionResult GetBanLockTrongKhoang(string idChinhanh, string idKhuvuc, DateOnly ngay, TimeOnly gio)
        {
            try
            {
                // Lấy danh sách bàn trong khu vực và chi nhánh
                var banIdsTrongKhuVuc = _appDbContext.bans
                    .Where(b => b.Idchinhanh == idChinhanh && b.Khuvuc == idKhuvuc)
                    .Select(b => b.Idban)
                    .ToList();

                // Tìm các bàn bị lock trong khung giờ đó
                var lockedBans = _appDbContext.Banlock
                    .Where(bl =>
                        banIdsTrongKhuVuc.Contains(bl.IdBan) &&
                        bl.Ngay == ngay &&
                        gio >= bl.BatDau && gio < bl.KetThuc)
                    .Select(bl => bl.IdBan)
                    .Distinct()
                    .ToList();

                return Json(lockedBans);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi khi xử lý: {ex.Message}" });
            }
        }
        //[HttpGet]
        //public async Task<IActionResult> SearchDonDatBan(string maDon,string sđt, string tenNguoiDat)
        //{
        //    if(string.IsNullOrWhiteSpace(maDon)&& string.IsNullOrWhiteSpace(sđt) && string.IsNullOrWhiteSpace(tenNguoiDat))
        //    {
        //        ViewBag.Message = "Vui lòng nhập ít nhất 1 trường để tìm kiếm.";
        //           return View(new List<Datban>() );
        //    }
        //    var query = _appDbContext.Datbans
        //        .Include(d => d.Nguoidung)
        //        .Include(d => d.IdchinhanhNavigation)
        //        .Include(d => d.Chitietdatbans)
        //           .ThenInclude(ct => ct.IdbanNavigation)
        //        .AsQueryable();
        //    if(!string.IsNullOrWhiteSpace(maDon))
        //        query = query.Where(d=>d.Iddatban.Trim().Equals(maDon.Trim(),StringComparison.OrdinalIgnoreCase));
        //    if (!string.IsNullOrWhiteSpace(sđt))
        //        query = query.Where(d => d.Sđtngdat != null && d.Sđtngdat.Contains(sđt.Trim()));
        //    if (!string.IsNullOrWhiteSpace(tenNguoiDat))
        //        query = query.Where(d => d.Tenngdat != null && d.Tenngdat.Contains(tenNguoiDat.Trim()));
        //    var result = await query.OrderByDescending(d=>d.Ngaydat).ToListAsync();
        //    return View(result);
        //}

    }
}
