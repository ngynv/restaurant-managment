using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;
using System.Security.Claims;
using WebsiteOrdering.Enums;
using WebsiteOrdering.Helper;
using WebsiteOrdering.Models;
using WebsiteOrdering.Repositories;
using WebsiteOrdering.Services;

namespace WebsiteOrdering.Controllers
{
    public class BookingController : Controller
    {
        private readonly IAccountRepository _accountRepository;
        private readonly AppDbContext _appDbContext;
        private readonly IEmailService _emailService;
        public BookingController(AppDbContext context, IAccountRepository accountRepository, IEmailService emailService)
        {
            _emailService = emailService;
            _accountRepository = accountRepository;
            _appDbContext = context;
        }
        //Tạo mã iddondatban
        private static string GenerateRandomId(int length = 5)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public IActionResult Index()
        {
            ViewBag.ChiNhanh = _appDbContext.chinhanh.ToList();
            ViewBag.Ban = _appDbContext.bans.ToList();
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = _appDbContext.Users.FirstOrDefault(u => u.Id == userId);
                ViewBag.UserInfo = user;
            }

            return View(new Datban());
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

        //Hàm đặt bàn 
        [HttpPost]
        public async Task<IActionResult> DatBan(Datban datban, string EmailNguoiDung, string SdtNguoiDung, string FullNameNguoiDung, string selectedIdban)
        {

            string? idNguoiDung = null;
            string tenNguoiDat = datban.Tenngdat;
            string sdtNguoiDat = datban.Sđtngdat;

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                // Đã đăng nhập lấy user hiện tại
                idNguoiDung = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var currentUser = await _accountRepository.GetUserByIdAsync(idNguoiDung);

                // Nếu người dùng không sửa thông tin  gán mặc định từ user
                if (string.IsNullOrWhiteSpace(FullNameNguoiDung))
                {
                    tenNguoiDat = currentUser?.FullName ?? currentUser?.UserName ?? currentUser?.Email ?? "";
                }

                if (string.IsNullOrWhiteSpace(sdtNguoiDat) && !string.IsNullOrEmpty(currentUser?.PhoneNumber))
                {
                    sdtNguoiDat = currentUser.PhoneNumber;
                }
            }
            else
            {
                // Chưa đăng nhập kiểm tra user theo email
                var existingUser = await _accountRepository.GetUserByEmailAsync(EmailNguoiDung);

                if (existingUser != null)
                {
                    idNguoiDung = existingUser.Id;
                }
                else
                {
                    // Tạo user mới giả lập
                    var newUser = new ApplicationUser
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserName = EmailNguoiDung,
                        Email = EmailNguoiDung,
                        PhoneNumber = SdtNguoiDung,
                        FullName = FullNameNguoiDung,
                        EmailConfirmed = false
                    };

                    _appDbContext.Users.Add(newUser);
                    await _appDbContext.SaveChangesAsync();

                    idNguoiDung = newUser.Id;
                }

                // Gán tên và SĐT từ form (user.FullName & PhoneNumber)
                if (string.IsNullOrWhiteSpace(tenNguoiDat))
                {
                    tenNguoiDat = FullNameNguoiDung ?? "";
                }

                if (string.IsNullOrWhiteSpace(sdtNguoiDat) && !string.IsNullOrEmpty(sdtNguoiDat))
                {
                    sdtNguoiDat = sdtNguoiDat;
                }

            }

            // Tự động cộng 2 giờ
            var gioKetThuc = datban.Giobatdau.Add(TimeSpan.FromHours(2));

            // Kiểm tra bàn đã được đặt trong khoảng thời gian đó chưa
            var isBanDaDat = await _appDbContext.chitietdatbans
                .Include(c => c.IddatbanNavigation)
                .AnyAsync(c => c.Idban == selectedIdban
                    && c.IddatbanNavigation.Ngaydat == datban.Ngaydat
                      && c.IddatbanNavigation.Trangthaidatban !=  TrangThai.Cancelled 
                    && (
                        (datban.Giobatdau >= c.Giovao && datban.Giobatdau < c.Giora) ||     // giao nhau
                        (gioKetThuc > c.Giovao && gioKetThuc <= c.Giora) ||
                        (datban.Giobatdau <= c.Giovao && gioKetThuc >= c.Giora)            // bao phủ toàn bộ
                    )
                );

            if (isBanDaDat)
            {
                TempData["Error"] = "Bàn đã được đặt trong khung giờ này!";
                ViewBag.ChiNhanh = _appDbContext.chinhanh.ToList();
                return View(datban);
            }
            // Lấy thông tin bàn
            var ban = await _appDbContext.bans.FirstOrDefaultAsync(b => b.Idban == selectedIdban);
            if (ban == null)
            {
                TempData["Error"] = "Không tìm thấy bàn đã chọn!";
                return View(datban);
            }

            // So sánh số người đặt với sức chứa
            if (datban.Songuoidat > ban.Songuoi)
            {
                TempData["Error"] = $"Bàn chỉ chứa tối đa {ban.Songuoi} người. Vui lòng chọn bàn khác hoặc giảm số lượng.";
                ViewBag.ChiNhanh = _appDbContext.chinhanh.ToList();
                return View(datban);
            }

            var datBan = new Datban
            {
                Iddatban = GenerateRandomId(),
                Ngaydat = datban.Ngaydat,
                Giobatdau = datban.Giobatdau,
                Gioketthuc = gioKetThuc,
                Songuoidat = datban.Songuoidat,
                Ghichu = datban.Ghichu ?? "",
                Trangthaidatban = TrangThai.Pending,
                Idngdung = idNguoiDung,
                Idchinhanh = datban.Idchinhanh,
                Tenngdat = tenNguoiDat,
                Sđtngdat = sdtNguoiDat
            };
            _appDbContext.Datbans.Add(datBan);

            var chitiet = new Chitietdatban
            {
                Iddatban = datBan.Iddatban,
                Idban = selectedIdban,
                Giovao = datban.Giobatdau,  // ban đầu là giờ khách chọn, sau có thể cập nhật nếu đến trễ
                Giora = gioKetThuc
            };
            _appDbContext.chitietdatbans.Add(chitiet);



            //if (ban != null)
            //{
            //    ban.Trangthaiban = "Đã đặt";
            //}

            await _appDbContext.SaveChangesAsync();

            //TempData["Success"] = "Đặt bàn thành công!";
            return RedirectToAction("ChitietDatBan", new { id = datBan.Iddatban });
        }
        //Lấy danh sách bàn đã đặt theo ngày và giờ vào, giờ ra theo chi nhánh , khu vực
        [HttpGet]
        public IActionResult GetBanDaDat(string ngay, string gio, string idChinhanh, string idKhuvuc)
        {
            //Dùng TimeOnly.Parse thay vì TimeSpan
            var gioBatDau = TimeOnly.Parse(gio);
            var gioKetThuc = gioBatDau.Add(TimeSpan.FromHours(2));

            var danhSachBan = _appDbContext.chitietdatbans
                .Include(c => c.IddatbanNavigation)
                .Include(c => c.IdbanNavigation) // ⚠️ Bổ sung Include để dùng c.IdbanNavigation.Khuvuc
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
                .Select(c => new
                {
                    idban = c.IdbanNavigation.Idban,
                    ngay = c.IddatbanNavigation.Ngaydat.ToString("yyyy-MM-dd"),
                    gio = c.Giovao.ToString(@"HH\:mm"),
                    idchinhanh = c.IddatbanNavigation.Idchinhanh,
                    idkhuvuc = c.IdbanNavigation.Khuvuc
                })
                .ToList();

            return Json(danhSachBan);
        }


        //Hiển thị chi tiết đơn đặt bàn theo id
        public async Task<IActionResult> ChitietDatBan(string id)
        {
            var datBan = await _appDbContext.Datbans
                .Include(d => d.Nguoidung)
                .Include(d => d.IdchinhanhNavigation)
                .Include(d => d.Chitietdatbans)
                   .ThenInclude(ct => ct.IdbanNavigation)
        .FirstOrDefaultAsync(d => d.Iddatban == id);

            if (datBan == null)
            {
                return NotFound("Không tìm thấy đơn đặt bàn.");
            }

            try
            {
                return View("ChitietDatBan", datBan);
            }
            catch (Exception ex)
            {
                return Content("Lỗi hiển thị view: " + ex.Message);
            }
        }


        [HttpGet]
        public async Task<IActionResult> SearchDonDatBan(string maDon, string sdt, string tenNguoiDat, string emailNguoiDat)
        {
            // Check nếu không nhập gì hết thì báo lỗi
            if (string.IsNullOrWhiteSpace(maDon) && string.IsNullOrWhiteSpace(sdt) && string.IsNullOrWhiteSpace(tenNguoiDat) && string.IsNullOrWhiteSpace(emailNguoiDat))
            {
                ViewBag.Message = "Vui lòng nhập ít nhất 1 trường để tìm kiếm.";
                return View(new List<Datban>());
            }

            var query = _appDbContext.Datbans
                .Include(d => d.Nguoidung)
                .Include(d => d.IdchinhanhNavigation)
                .Include(d => d.Chitietdatbans)
                    .ThenInclude(ct => ct.IdbanNavigation)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(maDon))
                query = query.Where(d => d.Iddatban != null && d.Iddatban.Trim().ToLower() == maDon.Trim().ToLower());

            if (!string.IsNullOrWhiteSpace(sdt))
                query = query.Where(d => d.Sđtngdat != null && d.Sđtngdat.Contains(sdt.Trim()));

            if (!string.IsNullOrWhiteSpace(tenNguoiDat))
                query = query.Where(d => d.Tenngdat != null && d.Tenngdat.Contains(tenNguoiDat.Trim()));
            //if (!string.IsNullOrWhiteSpace(emailNguoiDat))
            //    query = query.Where(d => d.Nguoidung != null && d.Nguoidung.Email != null && d.Nguoidung.Email.Contains(emailNguoiDat.Trim()));

            if (!string.IsNullOrWhiteSpace(emailNguoiDat))
            {
                var email = emailNguoiDat.Trim();

                // Kiểm tra xem chuỗi nhập có chứa @gmail.com không
                if (!email.Contains("@gmail.com", StringComparison.OrdinalIgnoreCase))
                {
                    ViewBag.Message = "Vui lòng nhập đúng định dạng email (ví dụ: example@gmail.com).";
                    return View(new List<Datban>());
                }

                query = query.Where(d => d.Nguoidung != null && d.Nguoidung.Email != null && d.Nguoidung.Email.Contains(email));
            }


            var result = await query.OrderByDescending(d => d.Ngaydat).ToListAsync();

            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> LichSuDatBan()
        {
            // Kiểm tra đăng nhập
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account"); // Hoặc trả về view thông báo đăng nhập
            }

            // Lấy Id người dùng
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return NotFound("Không tìm thấy thông tin người dùng.");
            }

            // Lấy danh sách đặt bàn của người dùng đó
            var datBans = await _appDbContext.Datbans
                .Include(d => d.IdchinhanhNavigation)
                .Include(d => d.Chitietdatbans)
                    .ThenInclude(ct => ct.IdbanNavigation)
                .Where(d => d.Idngdung == userId)
                .OrderByDescending(d => d.Ngaydat)
                .ToListAsync();

            return View("LichSuDatBan", datBans);
        }
        [HttpGet]
        public async Task<IActionResult> CapNhatDatBan(string id)
        {
            var datBan = await _appDbContext.Datbans
                .Include(d => d.Chitietdatbans)
                    .ThenInclude(ct => ct.IdbanNavigation)
                .Include(d => d.Nguoidung) // Thêm để lấy email, phone, fullname
                .FirstOrDefaultAsync(d => d.Iddatban == id);

            if (datBan == null)
                return NotFound("Không tìm thấy đơn đặt bàn.");

            if (datBan.Trangthaidatban != TrangThai.Pending)
                return BadRequest("Chỉ có thể cập nhật đơn đang chờ xác nhận.");

            // Lấy danh sách chi nhánh
            ViewBag.ChiNhanh = _appDbContext.chinhanh.ToList();

            var idKhuvuc = datBan.Chitietdatbans.FirstOrDefault()?.IdbanNavigation.Khuvuc;
            var idChiNhanh = datBan.Idchinhanh;

            // Lấy danh sách bàn theo chi nhánh và khu vực hiện tại
            ViewBag.Ban = _appDbContext.bans
                .Where(b => b.Idchinhanh == idChiNhanh && b.Khuvuc == idKhuvuc)
                .ToList();

            // Truyền thêm thông tin người đặt
            ViewBag.UserInfo = datBan.Nguoidung;

            // Truyền khu vực hiện tại
            ViewBag.KhuvucHienTai = idKhuvuc;
            ViewBag.IdBanHienTai = datBan.Chitietdatbans.FirstOrDefault()?.Idban;

            return View("Index", datBan);
        }


        //[HttpPost]
        //public async Task<IActionResult> CapNhatDatBan(Datban datban, string selectedIdban)
        //{
        //    var datBan = await _appDbContext.Datbans
        //        .Include(d => d.Chitietdatbans)
        //        .FirstOrDefaultAsync(d => d.Iddatban == datban.Iddatban);

        //    if (datBan == null)
        //        return NotFound("Không tìm thấy đơn.");

        //    if (datBan.Trangthaidatban != "Chờ xác nhận")
        //        return BadRequest("Chỉ cập nhật đơn đang chờ xác nhận.");

        //    var gioKetThuc = datban.Giobatdau.Add(TimeSpan.FromHours(2));

        //    // Check bàn đã được đặt chưa
        //    var isBanDaDat = await _appDbContext.chitietdatbans
        //        .Include(c => c.IddatbanNavigation)
        //        .AnyAsync(c => c.Idban == selectedIdban
        //            && c.IddatbanNavigation.Ngaydat == datban.Ngaydat
        //            && c.IddatbanNavigation.Trangthaidatban != "Đã hủy"
        //            && c.Iddatban != datBan.Iddatban
        //            && (
        //                (datban.Giobatdau >= c.Giovao && datban.Giobatdau < c.Giora) ||
        //                (gioKetThuc > c.Giovao && gioKetThuc <= c.Giora) ||
        //                (datban.Giobatdau <= c.Giovao && gioKetThuc >= c.Giora)
        //            )
        //        );

        //    if (isBanDaDat)
        //    {
        //        TempData["Error"] = "Bàn đã được đặt trong khung giờ này!";
        //        ViewBag.ChiNhanh = _appDbContext.chinhanh.ToList();
        //        return View("Index", datban);
        //    }

        //    // Update thông tin đơn
        //    datBan.Ngaydat = datban.Ngaydat;
        //    datBan.Giobatdau = datban.Giobatdau;
        //    datBan.Gioketthuc = gioKetThuc;
        //    datBan.Songuoidat = datban.Songuoidat;
        //    datBan.Ghichu = datban.Ghichu ?? "";
        //    datBan.Idchinhanh = datban.Idchinhanh;
        //    datBan.Tenngdat = datban.Tenngdat;
        //    datBan.Sđtngdat= datban.Sđtngdat;
        //    // Xử lý chi tiết
        //    var chiTiet = datBan.Chitietdatbans.FirstOrDefault();
        //    if (chiTiet != null)
        //    {
        //        // Remove chi tiết cũ
        //        _appDbContext.chitietdatbans.Remove(chiTiet);
        //        await _appDbContext.SaveChangesAsync();

        //        // Add chi tiết mới
        //        var newChiTiet = new Chitietdatban
        //        {
        //            Iddatban = datBan.Iddatban,
        //            Idban = selectedIdban,
        //            Giovao = datban.Giobatdau,
        //            Giora = gioKetThuc
        //        };
        //        _appDbContext.chitietdatbans.Add(newChiTiet);
        //        await _appDbContext.SaveChangesAsync();
        //    }

        //    TempData["Success"] = "Cập nhật thành công!";
        //    return RedirectToAction("ChitietDatBan", new { id = datBan.Iddatban });
        //}

        [HttpPost]
        public async Task<IActionResult> CapNhatDatBan(Datban datban, string selectedIdban)
        {
            var datBan = await _appDbContext.Datbans
                .Include(d => d.Chitietdatbans)
                .FirstOrDefaultAsync(d => d.Iddatban == datban.Iddatban);

            if (datBan == null)
                return NotFound("Không tìm thấy đơn.");

            if (datBan.Trangthaidatban != TrangThai.Pending)
                return BadRequest("Chỉ cập nhật đơn đang chờ xác nhận.");

            var chiTiet = datBan.Chitietdatbans.FirstOrDefault();
            if (chiTiet == null)
            {
                TempData["Error"] = "Không tìm thấy chi tiết bàn.";
                return RedirectToAction("ChitietDatBan", new { id = datBan.Iddatban });
            }

            if (string.IsNullOrEmpty(selectedIdban))
            {
                TempData["Error"] = "Vui lòng chọn bàn.";
                return RedirectToAction("ChitietDatBan", new { id = datBan.Iddatban });
            }

            var gioKetThuc = datban.Giobatdau.Add(TimeSpan.FromHours(2));

            bool isChangeBan = selectedIdban != chiTiet.Idban;
            bool isChangeNgay = datban.Ngaydat != datBan.Ngaydat;
            bool isChangeGio = datban.Giobatdau != datBan.Giobatdau;

            if (isChangeBan)
            {
                // Check trùng bàn
                var isBanDaDat = await _appDbContext.chitietdatbans
                    .Include(c => c.IddatbanNavigation)
                    .AnyAsync(c => c.Idban == selectedIdban
                        && c.IddatbanNavigation.Ngaydat == datban.Ngaydat
                        && c.IddatbanNavigation.Trangthaidatban != TrangThai.Cancelled
                        && c.Iddatban != datBan.Iddatban
                        && (
                            (datban.Giobatdau >= c.Giovao && datban.Giobatdau < c.Giora) ||
                            (gioKetThuc > c.Giovao && gioKetThuc <= c.Giora) ||
                            (datban.Giobatdau <= c.Giovao && gioKetThuc >= c.Giora)
                        )
                    );

                if (isBanDaDat)
                {
                    TempData["Error"] = "Bàn mới đã được đặt trong khung giờ này!";
                    return RedirectToAction("ChitietDatBan", new { id = datBan.Iddatban });
                }

                // Xóa chi tiết cũ
                _appDbContext.chitietdatbans.Remove(chiTiet);
                await _appDbContext.SaveChangesAsync();

                // Tạo chi tiết mới
                var newChiTiet = new Chitietdatban
                {
                    Iddatban = datBan.Iddatban,
                    Idban = selectedIdban,
                    Giovao = datban.Giobatdau,
                    Giora = gioKetThuc
                };
                _appDbContext.chitietdatbans.Add(newChiTiet);
            }
            else
            {
                // Chỉ đổi ngày hoặc giờ => update chi tiết
                chiTiet.Giovao = datban.Giobatdau;
                chiTiet.Giora = gioKetThuc;
            }

            // Update các thông tin khác
            datBan.Ngaydat = datban.Ngaydat;
            datBan.Giobatdau = datban.Giobatdau;
            datBan.Gioketthuc = gioKetThuc;
            datBan.Songuoidat = datban.Songuoidat;
            datBan.Ghichu = datban.Ghichu ?? "";
            datBan.Idchinhanh = datban.Idchinhanh;
            datBan.Tenngdat = datban.Tenngdat;
            datBan.Sđtngdat = datban.Sđtngdat;

            await _appDbContext.SaveChangesAsync();

            TempData["Success"] = "Cập nhật thành công!";
            return RedirectToAction("ChitietDatBan", new { id = datBan.Iddatban });
        }

        [HttpPost]
        public async Task<IActionResult> HuyDatBan(string id, string lyDo, string lyDoChiTiet)
        {
            var datban = await _appDbContext.Datbans
                .Include(d => d.Nguoidung)
                .Include(d => d.IdchinhanhNavigation)
                .FirstOrDefaultAsync(d => d.Iddatban == id);

            if (datban == null)
                return NotFound("Không tìm thấy đơn đặt bàn.");

            // Chỉ cho phép hủy nếu trạng thái là Đã xác nhận
            if (datban.Trangthaidatban != TrangThai.Confirmed)
                return BadRequest("Chỉ có thể hủy đơn ở trạng thái Đã xác nhận.");

            // Cập nhật trạng thái
            datban.Trangthaidatban = TrangThai.Cancelled;

            // Lưu lý do
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

            // Gửi email thông báo nếu có
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
                    ["LyDo"] = datban.Lydo
                };

                var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "EmailHuyDatBan.html");
                var body = EmailTemplateHelper.PopulateTemplate(templatePath, placeholders);

                await _emailService.SendEmailAsync(email, $"Xác nhận hủy đặt bàn thành công - Mã đơn: {datban.Iddatban}", body);
            }

            TempData["Success"] = "Hủy đơn đặt bàn thành công!";
            return RedirectToAction("LichSuDatBan");
        }

        //Hiển thị bàn lock 
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