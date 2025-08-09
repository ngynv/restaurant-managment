using Lucene.Net.Store;
using IO_Directory = System.IO.Directory;
using LuceneDirectory = Lucene.Net.Store.Directory;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebsiteOrdering.Helper;
using WebsiteOrdering.Models;
using WebsiteOrdering.Repositories;
using WebsiteOrdering.Services;
using WebsiteOrdering.Enums;

namespace WebsiteOrdering.Areas.Staff.Controllers
{
    [Area("Staff")]
    [Route("[area]/[controller]/[action]")]
    public class BookingStaffController : Controller
    {
        private readonly IAccountRepository _accountRepository;
        private readonly AppDbContext _appDbContext;
        private readonly IEmailService _emailService;
        public BookingStaffController(AppDbContext context, IEmailService emailService, IAccountRepository accountRepository)
        {
            _emailService = emailService;
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

       

        public async Task<IActionResult> Index(TrangThai? trangThai, string idChiNhanh = "", string tuNgay = "")
        {
            var staffChiNhanhId = User.FindFirst("ChiNhanhId")?.Value;

            if (string.IsNullOrEmpty(staffChiNhanhId))
            {
                return RedirectToAction("Login", "Account");
            }

            var query = _appDbContext.Datbans
                .Include(d => d.Chitietdatbans)
                .ThenInclude(ct => ct.IdbanNavigation)
                .Include(d => d.IdchinhanhNavigation)
                .Where(d => d.Idchinhanh == staffChiNhanhId);

            if (!string.IsNullOrEmpty(trangThai.ToString()))
            {
                query = query.Where(d => d.Trangthaidatban == trangThai);
            }

            if (!string.IsNullOrEmpty(tuNgay) && DateOnly.TryParse(tuNgay, out var ngayBatDau))
            {
                query = query.Where(d => d.Ngaydat >= ngayBatDau);
            }

            var datbans = await query.OrderByDescending(d => d.Ngaydat)
                .ThenByDescending(d => d.Giobatdau)
                .ToListAsync();

            // QUAN TRỌNG: Load danh sách bàn cho EditForm
            ViewBag.BanList = await _appDbContext.bans
                .Where(b => b.Idchinhanh == staffChiNhanhId)
                .OrderBy(b => b.Khuvuc)
                .ThenBy(b => b.Tenban)
                .ToListAsync();

            // Debug: Kiểm tra số lượng bàn
            var banCount = ViewBag.BanList?.Count ?? 0;
            ViewBag.Debug = $"Chi nhánh: {staffChiNhanhId}, Số bàn: {banCount}";

            // Count statistics
            var allDatbans = await _appDbContext.Datbans
                .Where(d => d.Idchinhanh == staffChiNhanhId)
                .ToListAsync();

            ViewBag.CountChoXacNhan = allDatbans.Count(d => d.Trangthaidatban == TrangThai.Pending);
            ViewBag.CountDaXacNhan = allDatbans.Count(d => d.Trangthaidatban == TrangThai.Confirmed);
            ViewBag.CountDaHuy = allDatbans.Count(d => d.Trangthaidatban == TrangThai.Cancelled);
            ViewBag.CountHoanThanh = allDatbans.Count(d => d.Trangthaidatban == TrangThai.Completed);

            ViewBag.SelectedTrangThai = trangThai;
            ViewBag.SelectedChiNhanh = idChiNhanh;
            ViewBag.TuNgay = tuNgay;

            return View(datbans);
        }

        //Chi tiết đơn đặt bàn
        public async Task<IActionResult> DetailDonDatBan(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            id = id.Trim();

            var datban = await _appDbContext.Datbans
              .Include(d => d.Nguoidung)
              .Include(d => d.IdchinhanhNavigation)
              .Include(d => d.Chitietdatbans)
                  .ThenInclude(ct => ct.IdbanNavigation)
              .ToListAsync(); // await được vì trả về Task<List<>>

            var foundDatban = datban
                .FirstOrDefault(d => d.Iddatban?.Trim().Equals(id, StringComparison.OrdinalIgnoreCase) == true);

            if (foundDatban == null)
            {
                return NotFound();
            }

            return View("DetailDonDatBan", foundDatban);

        }
        //Xác nhận đơn đặt bàn
        [HttpPost]
        public async Task<IActionResult> XacNhanDatBan(string id)
        {
            var datban = await _appDbContext.Datbans
                .Include(d => d.Nguoidung)
                .Include(d => d.IdchinhanhNavigation)
                .FirstOrDefaultAsync(d => d.Iddatban == id);

            if (datban == null) return NotFound();

            datban.Trangthaidatban = TrangThai.Confirmed;
            await _appDbContext.SaveChangesAsync();

            var email = datban.Nguoidung?.Email;
            if (!string.IsNullOrEmpty(email))
            {
                var placeholders = new Dictionary<string, string>
                {
                    ["MaDonDatBan"] = datban.Iddatban,
                    ["TenNguoiDat"] = datban.Tenngdat ?? datban.Nguoidung?.FullName ?? "Khách hàng",
                    ["TenChiNhanh"] = datban.IdchinhanhNavigation.Tencnhanh,
                    ["NgayDat"] = datban.Ngaydat.ToString("dd/MM/yyyy"),
                    ["GioBatDau"] = datban.Giobatdau.ToString(),
                    ["GioKetThuc"] = datban.Gioketthuc.ToString()
                };

                //var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "EmailXacNhanDatBan.html");
                var templatePath = Path.Combine(IO_Directory.GetCurrentDirectory(), "Templates", "EmailXacNhanDatBan.html");

                var body = EmailTemplateHelper.PopulateTemplate(templatePath, placeholders);

                await _emailService.SendEmailAsync(email, $"Xác nhận đặt bàn thành công - Mã đơn đặt bàn: {datban.Iddatban}", body);
            }

            return RedirectToAction("Index", new { idChiNhanh = datban.Idchinhanh, trangThai = "Chờ xác nhận" });
        }
        //Nhân viên hủy đặt bàn 
        [HttpPost]
        public async Task<IActionResult> HuyDatBan(string id, string lyDo, string lyDoChiTiet)
        {
            var datban = await _appDbContext.Datbans
            .Include(d => d.Nguoidung)
            .Include(d => d.IdchinhanhNavigation)
            .FirstOrDefaultAsync(d => d.Iddatban == id);
            if (datban == null) return NotFound();

            datban.Trangthaidatban = TrangThai.Cancelled;
            // Nếu chọn "Khác" thì lưu lý do chi tiết
            if (lyDo == "Khác" && !string.IsNullOrWhiteSpace(lyDoChiTiet))
            {
                datban.Lydo = lyDoChiTiet;
            }
            else
            {
                datban.Lydo = lyDo;
            }
            _appDbContext.Entry(datban).State = EntityState.Modified;
            await _appDbContext.SaveChangesAsync();
            var email = datban.Nguoidung?.Email;
            if (!string.IsNullOrEmpty(email))
            {
                var placeholders = new Dictionary<string, string>
                {
                    ["MaDonDatBan"] = datban.Iddatban,
                    ["TenNguoiDat"] = datban.Tenngdat ?? datban.Nguoidung?.FullName ?? "Khách hàng",
                    ["TenChiNhanh"] = datban.IdchinhanhNavigation.Tencnhanh,
                    ["NgayDat"] = datban.Ngaydat.ToString("dd/MM/yyyy"),
                    ["GioBatDau"] = datban.Giobatdau.ToString(),
                    ["GioKetThuc"] = datban.Gioketthuc.ToString(),
                    ["LyDo"] = datban.Lydo.ToString()
                };

                //var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "EmailHuyDatBan.html");
                var templatePath = Path.Combine(IO_Directory.GetCurrentDirectory(), "Templates", "EmailXacNhanDatBan.html");

                var body = EmailTemplateHelper.PopulateTemplate(templatePath, placeholders);

                await _emailService.SendEmailAsync(email, $"Xác nhận hủy đặt bàn thành công - Mã đơn đặt bàn: {datban.Iddatban}", body);
            }

            return RedirectToAction("Index", new { idChiNhanh = datban.Idchinhanh, trangThai = "Đã hủy" });
        }
        //Khách đã đến
        [HttpPost]
        public async Task<IActionResult> KhachDaDen(string id)
        {
            var datban = await _appDbContext.Datbans
                .Include(d => d.Chitietdatbans)
                .FirstOrDefaultAsync(d => d.Iddatban == id);

            if (datban == null)
                return NotFound();

            var today = DateOnly.FromDateTime(DateTime.Today);
            if (datban.Ngaydat != today)
            {
                TempData["Error"] = "Chỉ có thể xác nhận khách đã đến trong ngày đặt bàn.";
                return RedirectToAction("DanhSachKhachDaDen");
            }

            // Cập nhật trạng thái
            datban.Trangthaidatban = TrangThai.Came;

            // Giờ vào (lấy giờ hiện tại)
            var gioVao = TimeOnly.FromDateTime(DateTime.Now);

            foreach (var chitiet in datban.Chitietdatbans)
            {
                chitiet.Giovao = gioVao;
            }

            await _appDbContext.SaveChangesAsync();

            TempData["Success"] = "Đã xác nhận khách đã đến.";
            return RedirectToAction("DanhSachKhachDaDen");
        }
        //Hiển thị danh sách khách đã đến
        public async Task<IActionResult> DanhSachKhachDaDen()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            var list = await _appDbContext.Datbans
                .Where(d =>
                    (d.Trangthaidatban == TrangThai.Came || d.Trangthaidatban == TrangThai.Eating || d.Trangthaidatban == TrangThai.Ordered)
                    && d.Ngaydat == today)
                
                .Include(d => d.Nguoidung)
                .Include(d => d.IdchinhanhNavigation)
                .Include(d => d.Chitietdatbans)
                    .ThenInclude(ct => ct.IdbanNavigation)
                .ToListAsync();

            return View(list);
        }

        //Cập nhật đơn đặt bàn
        [HttpPost]
        public async Task<IActionResult> EditDatBan(string Iddatban, string Idban, TimeOnly Giobatdau, TimeOnly Gioketthuc)
        {
            try
            {
                var datban = await _appDbContext.Datbans
                    .Include(d => d.Chitietdatbans)
                    .FirstOrDefaultAsync(d => d.Iddatban == Iddatban);

                if (datban == null)
                {
                    TempData["Error"] = "Không tìm thấy đơn đặt bàn.";
                    return RedirectToAction("Index");
                }

                var idChiNhanh = User.FindFirst("ChiNhanhId")?.Value;
                if (datban.Idchinhanh != idChiNhanh)
                {
                    TempData["Error"] = "Bạn không có quyền sửa đơn đặt bàn này.";
                    return RedirectToAction("Index");
                }

                // Nếu giá trị không truyền lên (người dùng không chọn lại), giữ nguyên giá trị cũ
                if (string.IsNullOrEmpty(Idban))
                {
                    Idban = datban.Chitietdatbans.FirstOrDefault()?.Idban;
                }
                if (Giobatdau == default)
                {
                    Giobatdau = datban.Giobatdau;
                }

                // Luôn tính Giờ kết thúc = Giờ bắt đầu + 2 giờ
                Gioketthuc = Giobatdau.AddHours(2);

                // Nếu vượt quá 23h59, bạn có thể tự giới hạn (tùy logic)
                if (Gioketthuc.Hour >= 23)
                {
                    Gioketthuc = new TimeOnly(23, 59);
                }

                // Kiểm tra bàn có bị trùng giờ
                var hasConflict = await _appDbContext.chitietdatbans
                    .Where(ct => ct.Idban == Idban &&
                                ct.IddatbanNavigation.Ngaydat == datban.Ngaydat &&
                                ct.IddatbanNavigation.Iddatban != Iddatban)
                    .AnyAsync(ct =>
                        (Giobatdau < ct.IddatbanNavigation.Gioketthuc && Gioketthuc > ct.IddatbanNavigation.Giobatdau)
                    );

                if (hasConflict)
                {
                    TempData["Error"] = "Bàn hoặc giờ đã được đặt. Vui lòng chọn bàn hoặc giờ khác.";
                    return RedirectToAction("Index");
                }

                // Cập nhật thông tin đặt bàn
                await _appDbContext.Datbans
                    .Where(d => d.Iddatban == Iddatban)
                    .ExecuteUpdateAsync(d => d
                        .SetProperty(x => x.Giobatdau, Giobatdau)
                        .SetProperty(x => x.Gioketthuc, Gioketthuc)
                    );

                // Cập nhật chi tiết bàn
                await _appDbContext.chitietdatbans
                    .Where(ct => ct.Iddatban == Iddatban)
                    .ExecuteDeleteAsync();

                _appDbContext.chitietdatbans.Add(new Chitietdatban
                {
                    Iddatban = Iddatban,
                    Idban = Idban,
                    Giovao = Giobatdau,
                    Giora = Gioketthuc,
                });

                await _appDbContext.SaveChangesAsync();

                TempData["Success"] = "Cập nhật đơn đặt bàn thành công.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi cập nhật: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        //Hiển thị lại các thông tin đã chọn của đơn đó
        [HttpGet]
        public async Task<IActionResult> GetEditForm(string Iddatban)
        {
            var datban = await _appDbContext.Datbans
                .Include(d => d.Chitietdatbans)
                .ThenInclude(ct => ct.IdbanNavigation)
                .Include(d => d.IdchinhanhNavigation)
                .FirstOrDefaultAsync(d => d.Iddatban == Iddatban);

            if (datban == null) return NotFound();

            var idChiNhanh = User.FindFirst("ChiNhanhId")?.Value;
            if (datban.Idchinhanh != idChiNhanh)
            {
                return Forbid();
            }

            // Load danh sách bàn theo chi nhánh
            ViewBag.BanList = await _appDbContext.bans
                .Where(b => b.Idchinhanh == idChiNhanh)
                .ToListAsync();

            return PartialView("_EditForm", datban);
        }

        //[HttpGet]
        //public async Task<IActionResult> CreateDatBanTaiCho()
        //{
        //    var idChiNhanh = HttpContext.Session.GetString("ChiNhanhId");
        //    if (string.IsNullOrEmpty(idChiNhanh))
        //    {
        //        return RedirectToAction("Login", "Account");
        //    }

        //    var banList = await _appDbContext.bans
        //        .Where(b => b.Idchinhanh == idChiNhanh)
        //        .ToListAsync();

        //    ViewBag.BanList = banList;
        //    ViewBag.ChiNhanhTen = (await _appDbContext.chinhanh.FindAsync(idChiNhanh))?.Tencnhanh;
        //    ViewBag.Idchinhanh = idChiNhanh;
        //    ViewBag.IsNhanVien = User.IsInRole("Staff");

        //    // Tạo model với giờ hiện tại (làm tròn lên 30 phút gần nhất)
        //    var now = DateTime.Now;
        //    var roundedMinutes = (now.Minute < 30) ? 30 : 60;
        //    var defaultTime = now.AddMinutes(roundedMinutes - now.Minute);

        //    var model = new Datban
        //    {
        //        Giobatdau = TimeOnly.FromDateTime(defaultTime),
        //        Songuoidat = 1,
        //        Ngaydat = DateOnly.FromDateTime(DateTime.Today)
        //    };

        //    return View(model);
        //}


        //[HttpPost]
        //public async Task<IActionResult> CreateDatBanTaiCho(Datban model, string Idban)
        //{
        //    var idChiNhanh = HttpContext.Session.GetString("ChiNhanhId");
        //    if (string.IsNullOrEmpty(idChiNhanh))
        //    {
        //        return RedirectToAction("Login", "Account");
        //    }

        //    if (string.IsNullOrEmpty(Idban))
        //    {
        //        ModelState.AddModelError("Idban", "Vui lòng chọn bàn.");
        //        ViewBag.BanList = await _appDbContext.bans.Where(b => b.Idchinhanh == idChiNhanh).ToListAsync();
        //        ViewBag.ChiNhanhTen = (await _appDbContext.chinhanh.FindAsync(idChiNhanh))?.Tencnhanh;
        //        return View(model);
        //    }

        //    // Xác định ngày đặt
        //    var ngayDat = model.Ngaydat != default ? model.Ngaydat : DateOnly.FromDateTime(DateTime.Today);

        //    // Giờ kết thúc (tự động +2h, tối đa 23:59)
        //    var gioKetThuc = model.Giobatdau.AddHours(2);
        //    if (gioKetThuc.Hour >= 23)
        //    {
        //        gioKetThuc = new TimeOnly(23, 59);
        //    }

        //    // Kiểm tra bàn có bị trùng không
        //    var hasConflict = await _appDbContext.chitietdatbans
        //        .Where(ct => ct.Idban == Idban && ct.IddatbanNavigation.Ngaydat == ngayDat && ct.IddatbanNavigation.Trangthaidatban != "Đã hủy")
        //        .AnyAsync(ct =>
        //            (model.Giobatdau < ct.IddatbanNavigation.Gioketthuc && gioKetThuc > ct.IddatbanNavigation.Giobatdau)
        //        );

        //    if (hasConflict)
        //    {
        //        ModelState.AddModelError("Giobatdau", "Bàn này đã được đặt trong khung giờ này.");
        //        ViewBag.BanList = await _appDbContext.bans.Where(b => b.Idchinhanh == idChiNhanh).ToListAsync();
        //        ViewBag.ChiNhanhTen = (await _appDbContext.chinhanh.FindAsync(idChiNhanh))?.Tencnhanh;
        //        return View(model);
        //    }

        //    // Sinh ID
        //    var idDatBan = GenerateRandomId();

        //    var datban = new Datban
        //    {
        //        Iddatban = idDatBan,
        //        Idchinhanh = idChiNhanh,
        //        Ngaydat = ngayDat,
        //        Giobatdau = model.Giobatdau,
        //        Gioketthuc = gioKetThuc,
        //        Songuoidat = model.Songuoidat,
        //        Tenngdat = string.IsNullOrEmpty(model.Tenngdat) ? "Khách vãng lai" : model.Tenngdat,
        //        Sđtngdat = model.Sđtngdat,
        //        Trangthaidatban = model.Trangthaidatban,
        //        Ghichu = model.Ghichu
        //    };

        //    _appDbContext.Datbans.Add(datban);

        //    _appDbContext.chitietdatbans.Add(new Chitietdatban
        //    {
        //        Iddatban = idDatBan,
        //        Idban = Idban,
        //        Giovao = model.Giobatdau,
        //        Giora = gioKetThuc
        //    });

        //    await _appDbContext.SaveChangesAsync();

        //    TempData["Success"] = "Đã tạo đơn đặt bàn thành công.";

        //    if (model.Trangthaidatban == "Khách đã đến")
        //        return RedirectToAction("DanhSachKhachDaDen");
        //    else
        //        return RedirectToAction("Index");
        //}
        // GET: Hiển thị form tạo đơn đặt bàn
        [HttpGet]
        public async Task<IActionResult> CreateDatBanTaiCho()
        {
            var idChiNhanh = User.FindFirst("ChiNhanhId")?.Value;
            if (string.IsNullOrEmpty(idChiNhanh))
            {
                TempData["Error"] = "Không xác định được chi nhánh nhân viên.";
                return RedirectToAction("LoginStaff", "Admin", new { area = "Admin" });
            }

            var banList = await _appDbContext.bans
                .Where(b => b.Idchinhanh == idChiNhanh)
                .ToListAsync();

            ViewBag.Ban = banList;
            ViewBag.ChiNhanhTen = (await _appDbContext.chinhanh.FindAsync(idChiNhanh))?.Tencnhanh;
            ViewBag.Idchinhanh = idChiNhanh;
            ViewBag.IsNhanVien = User.IsInRole("Staff");

            var model = new Datban
            {
               // Giobatdau = TimeOnly.FromDateTime(defaultTime),
                Songuoidat = 1,
               // Ngaydat = DateOnly.FromDateTime(DateTime.Today)
            };
            return View(model);
        }

        // POST: Lưu đơn đặt bàn
        [HttpPost]
        public async Task<IActionResult> CreateDatBanTaiCho(Datban datban, string selectedIdban)
        {
            string? idNguoiDung = null;
            string tenNguoiDat = datban.Tenngdat;
            string sdtNguoiDat = datban.Sđtngdat;

            // Lấy user hiện tại
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var currentUser = await _accountRepository.GetUserByIdAsync(userId);
            var roles = await _accountRepository.GetUserRolesAsync(currentUser);

            // Nếu là nhân viên
            if (roles.Contains("Staff"))
            {
                if (string.IsNullOrWhiteSpace(tenNguoiDat))
                {
                    TempData["Error"] = "Nhân viên cần nhập tên người đặt.";
                    return View(datban);
                }
                if (string.IsNullOrWhiteSpace(sdtNguoiDat))
                {
                    TempData["Error"] = "Nhân viên cần nhập số điện thoại.";
                    return View(datban);
                }

                // Gán id nhân viên làm người tạo
                idNguoiDung = currentUser.Id;
            }
            else
            {
                // Người chưa đăng nhập, không cho phép tạo
                TempData["Error"] = "Bạn cần đăng nhập để đặt bàn.";
                return RedirectToAction("Login", "Account");
            }

            // Kiểm tra trùng lịch
            var gioKetThuc = datban.Giobatdau.Add(TimeSpan.FromHours(2));
            var isBanDaDat = await _appDbContext.chitietdatbans
                .Include(c => c.IddatbanNavigation)
                .AnyAsync(c => c.Idban == selectedIdban
                    && c.IddatbanNavigation.Ngaydat == datban.Ngaydat
                    && c.IddatbanNavigation.Trangthaidatban != TrangThai.Cancelled
                    && (
                        (datban.Giobatdau >= c.Giovao && datban.Giobatdau < c.Giora)
                        || (gioKetThuc > c.Giovao && gioKetThuc <= c.Giora)
                        || (datban.Giobatdau <= c.Giovao && gioKetThuc >= c.Giora)
                    )
                );

            if (isBanDaDat)
            {
                TempData["Error"] = "Bàn đã được đặt trong khung giờ này!";
                ViewBag.ChiNhanh = _appDbContext.chinhanh.ToList();
                ViewBag.Ban = _appDbContext.bans.ToList();
                return View(datban);
            }

            var ban = await _appDbContext.bans.FirstOrDefaultAsync(b => b.Idban == selectedIdban);
            if (ban == null)
            {
                TempData["Error"] = "Không tìm thấy bàn đã chọn!";
                return View(datban);
            }

            if (datban.Songuoidat > ban.Songuoi)
            {
                TempData["Error"] = $"Bàn chỉ chứa tối đa {ban.Songuoi} người. Vui lòng chọn bàn khác hoặc giảm số lượng.";
                ViewBag.ChiNhanh = _appDbContext.chinhanh.ToList();
                ViewBag.Ban = _appDbContext.bans.ToList();
                return View(datban);
            }

            var datBanMoi = new Datban
            {
                Iddatban = GenerateRandomId(),
                Ngaydat = datban.Ngaydat,
                Giobatdau = datban.Giobatdau,
                Gioketthuc = gioKetThuc,
                Songuoidat = datban.Songuoidat,
                Ghichu = datban.Ghichu ?? "",
                Trangthaidatban = datban.Trangthaidatban, // nhân viên chọn: "Đã xác nhận" hoặc "Khách đã đến"
                Idngdung = idNguoiDung,
                Idchinhanh = datban.Idchinhanh,
                Tenngdat = tenNguoiDat,
                Sđtngdat = sdtNguoiDat
            };
            _appDbContext.Datbans.Add(datBanMoi);

            var chitiet = new Chitietdatban
            {
                Iddatban = datBanMoi.Iddatban,
                Idban = selectedIdban,
                Giovao = datban.Giobatdau,
                Giora = gioKetThuc
            };
            _appDbContext.chitietdatbans.Add(chitiet);

            await _appDbContext.SaveChangesAsync();

            TempData["Success"] = "Tạo đơn đặt bàn thành công!";

            // Điều hướng dựa trên trạng thái
            if (datban.Trangthaidatban == TrangThai.Confirmed)
            {
                return RedirectToAction("Index");
            }
            else if (datban.Trangthaidatban == TrangThai.Came)
            {
                return RedirectToAction("DanhSachKhachDaDen");
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult GetBanDaDat(string ngay, string gio, string idChinhanh, string idKhuvuc)
        {
            var gioBatDau = TimeOnly.Parse(gio);
            var gioKetThuc = gioBatDau.Add(TimeSpan.FromHours(2));

            // Lấy danh sách bàn đã được đặt, trùng ngày và giao nhau khung giờ
            var danhSachBanDaDat = _appDbContext.chitietdatbans
                .Include(c => c.IddatbanNavigation)
                .Include(c => c.IdbanNavigation)
                .Where(c =>
                    c.IddatbanNavigation.Ngaydat == DateOnly.Parse(ngay) &&
                    c.IddatbanNavigation.Idchinhanh == idChinhanh &&
                    c.IdbanNavigation.Khuvuc == idKhuvuc &&
                    c.IddatbanNavigation.Trangthaidatban != TrangThai.Cancelled &&
                    (
                        (gioBatDau >= c.Giovao && gioBatDau < c.Giora) ||
                        (gioKetThuc > c.Giovao && gioKetThuc <= c.Giora) ||
                        (gioBatDau <= c.Giovao && gioKetThuc >= c.Giora)
                    )
                )
                .Select(c => c.IdbanNavigation.Idban)
                .Distinct()
                .ToList();

            // Lấy tất cả bàn trong khu vực này
            var danhSachBan = _appDbContext.bans
                .Where(b => b.Idchinhanh == idChinhanh && b.Khuvuc == idKhuvuc)
                .Select(b => new
                {
                    idban = b.Idban,
                    tenban = b.Tenban,
                    songuoi = b.Songuoi,
                    trangthai = danhSachBanDaDat.Contains(b.Idban) ? "Đã đặt" : "Trống"
                })
                .ToList();

            return Json(danhSachBan);
        }

        [HttpGet]
        public async Task<IActionResult> GetBanByKhuvuc(string idChinhanh, string khuvuc)
        {
            var banList = await _appDbContext.bans
                .Where(b => b.Idchinhanh == idChinhanh && b.Khuvuc == khuvuc)
                .Select(b => new {
                    idban = b.Idban,
                    tenban = b.Tenban,
                    songuoi = b.Songuoi
                })
                .ToListAsync();

            return Json(banList);
        }


        // Ajax: lấy khu vực theo chi nhánh
        [HttpGet]
        public IActionResult GetKhuvucByChinhanh(string idChinhanh)
        {
            try
            {
                if (string.IsNullOrEmpty(idChinhanh))
                {
                    return Json(new List<string>());
                }

                var khuvucs = _appDbContext.bans
                    .Where(b => b.Idchinhanh == idChinhanh)
                    .Select(b => b.Khuvuc)
                    .Distinct()
                    .ToList();

                return Json(khuvucs);
            }
            catch (Exception ex)
            {
                return Json(new List<string>());
            }
        }
    }
}
