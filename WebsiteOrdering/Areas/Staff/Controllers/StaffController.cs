using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteOrdering.Enums;
using WebsiteOrdering.Models;
using WebsiteOrdering.Repositories;

namespace WebsiteOrdering.Areas.Staff.Controllers
{
    //[Authorize(Roles = "Staff")]
    [Area("Staff")]
    [Route("[area]/[controller]/[action]")]
    public class StaffController : Controller
    {
        private readonly AppDbContext _appDbContext;
        private readonly IAccountRepository _accountRepository;
        public StaffController(AppDbContext context, IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
            _appDbContext = context;
        }
        //Hàm tạo đơn đặt bàn
        private static string GenerateRandomId(int length = 5)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public async Task<IActionResult> Index(string maDon,string sđt,string tenNguoiDat)
        {
            var chiNhanhId = User.FindFirst("ChiNhanhId")?.Value;
            if (string.IsNullOrEmpty(chiNhanhId)) return Unauthorized();
            // Lấy chi nhánh
            var chiNhanh = _appDbContext.chinhanh.FirstOrDefault(c => c.Idchinhanh == chiNhanhId);
            ViewBag.TenChiNhanh = chiNhanh?.Tencnhanh ?? "Không rõ";
            List<Datban> result = new List<Datban>();

            if (!string.IsNullOrWhiteSpace(maDon) || !string.IsNullOrWhiteSpace(sđt) || !string.IsNullOrWhiteSpace(tenNguoiDat))
            {
                var query = _appDbContext.Datbans
                    .Include(d => d.Nguoidung)
                    .Include(d => d.IdchinhanhNavigation)
                    .Include(d => d.Chitietdatbans)
                        .ThenInclude(ct => ct.IdbanNavigation)
                    .Where(d => d.Idchinhanh == chiNhanhId)
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
        [HttpPost]
        public async Task<IActionResult> LogoutStaff()
        {
            await _accountRepository.LogoutAsync();
            //return Redirect("/");
            return RedirectToAction("LoginStaff", "Admin", new { area = "Admin" });
        }

        //Lấy danh sách khu vực theo chi nhánh
        [HttpGet]
        public IActionResult GetKhuvucByChinhanh()
        {
            var staffChiNhanhId = User.FindFirst("ChiNhanhId")?.Value;

            var khuvucs = _appDbContext.bans
                .Where(b => b.Idchinhanh == staffChiNhanhId)
                .Select(b => b.Khuvuc)
                .Distinct()
                .ToList();

            return Json(khuvucs);
        }
        //Lấy danh sách bàn theo khu vực
        [HttpGet]
        public IActionResult GetBanByKhuvuc(string idChinhanh, string khuvuc)
        {
            var staffChiNhanhId = User.FindFirst("ChiNhanhId")?.Value;
            var bans = _appDbContext.bans
                .Where(b => b.Idchinhanh == staffChiNhanhId && b.Khuvuc == khuvuc)
                .Select(b => new { b.Idban, b.Tenban, b.Songuoi, b.X, b.Y })
                .ToList();

            return Json(bans);
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

    }
}
