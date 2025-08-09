using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteOrdering.Enums;
using WebsiteOrdering.Helper;
using WebsiteOrdering.Models;
using WebsiteOrdering.Repositories;
using WebsiteOrdering.Services;

namespace WebsiteOrdering.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("[area]/[controller]/[action]")]
    public class BookingAdminController : Controller
    {
        private readonly AppDbContext _appDbContext;
        private readonly IEmailService _emailService;
        public BookingAdminController(AppDbContext context, IEmailService emailService)
        {
            _emailService = emailService;
            _appDbContext = context;
        }
        // Hiển thị danh sách đơn đặt bàn
        public async Task<IActionResult> Index(string idChiNhanh = null, TrangThai? trangThai = null, DateTime? tuNgay = null)
        {
            ViewBag.ChiNhanhList = await _appDbContext.chinhanh.ToListAsync();

            var query = _appDbContext.Datbans
                .Include(d => d.IdchinhanhNavigation)
                .Include(d => d.Nguoidung)
                .Include(d => d.Chitietdatbans)
                    .ThenInclude(ct => ct.IdbanNavigation)
                .AsQueryable();

            var baseFilter = _appDbContext.Datbans.AsQueryable();

            // Lọc chi nhánh nếu có
            if (!string.IsNullOrEmpty(idChiNhanh))
            {
                query = query.Where(d => d.Idchinhanh == idChiNhanh);
                baseFilter = baseFilter.Where(d => d.Idchinhanh == idChiNhanh);
            }

            // Lọc theo ngày nếu có
            if (tuNgay.HasValue)
            {
                var tuNgayOnly = DateOnly.FromDateTime(tuNgay.Value);
                query = query.Where(d => d.Ngaydat == tuNgayOnly);
                baseFilter = baseFilter.Where(d => d.Ngaydat == tuNgayOnly);
            }

            // Lọc trạng thái nếu có
            if (!string.IsNullOrEmpty(trangThai.ToString()))
            {
                query = query.Where(d => d.Trangthaidatban == trangThai);
            }

            // Đếm đơn theo trạng thái (chỉ dùng bộ lọc chi nhánh + ngày nếu có)
            ViewBag.CountChoXacNhan = await baseFilter.CountAsync(d => d.Trangthaidatban == TrangThai.Pending);
            ViewBag.CountDaXacNhan = await baseFilter.CountAsync(d => d.Trangthaidatban == TrangThai.Confirmed);
            ViewBag.CountDaHuy = await baseFilter.CountAsync(d => d.Trangthaidatban == TrangThai.Cancelled);

            var result = await query.OrderBy(d => d.Ngaydat).ToListAsync();

            // Truyền giá trị lọc lại để hiển thị
            ViewBag.SelectedChiNhanh = idChiNhanh;
            ViewBag.SelectedTrangThai = trangThai;
            ViewBag.TuNgay = tuNgay?.ToString("yyyy-MM-dd");

            return View(result);
        }
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
                    ["TenNguoiDat"] = datban.Tenngdat ?? datban.Nguoidung?.FullName ?? "Khách hàng",
                    ["TenChiNhanh"] = datban.IdchinhanhNavigation.Tencnhanh,
                    ["NgayDat"] = datban.Ngaydat.ToString("dd/MM/yyyy"),
                    ["GioBatDau"] = datban.Giobatdau.ToString(),
                    ["GioKetThuc"] = datban.Gioketthuc.ToString(),
                    ["MaDonDatBan"] = datban.Iddatban.ToString()
                };

                var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "EmailXacNhanDatBan.html");

                var body = EmailTemplateHelper.PopulateTemplate(templatePath, placeholders);

                await _emailService.SendEmailAsync(email, $"Xác nhận đặt bàn thành công - Mã đơn hàng {datban.Iddatban}", body);
            }

            return RedirectToAction("Index", new { idChiNhanh = datban.Idchinhanh, trangThai = "Chờ xác nhận" });
        }

        [HttpPost]
        public async Task<IActionResult> HuyDatBan(string id, string lyDo, string lyDoChiTiet)
        {
            var datban = await _appDbContext.Datbans
            .Include(d => d.Nguoidung)
            .Include(d => d.IdchinhanhNavigation)
            .Include(d => d.Chitietdatbans)
                  .ThenInclude(ct => ct.IdbanNavigation)
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
            // ✅ Cập nhật trạng thái bàn
            // Duyệt qua tất cả chi tiết bàn
            if (datban.Chitietdatbans != null && datban.Chitietdatbans.Any())
            {
                foreach (var ct in datban.Chitietdatbans)
                {
                    if (ct.IdbanNavigation != null)
                    {
                        ct.IdbanNavigation.Trangthaiban = "Còn"; // hoặc "Sẵn sàng"
                        _appDbContext.Entry(ct.IdbanNavigation).State = EntityState.Modified;
                    }
                }
            }

            _appDbContext.Entry(datban).State = EntityState.Modified;
            await _appDbContext.SaveChangesAsync();
            var email = datban.Nguoidung?.Email;
            if (!string.IsNullOrEmpty(email))
            {
                var placeholders = new Dictionary<string, string>
                {
                    ["TenNguoiDat"] = datban.Tenngdat ?? datban.Nguoidung?.FullName ?? "Khách hàng",
                    ["TenChiNhanh"] = datban.IdchinhanhNavigation.Tencnhanh,
                    ["NgayDat"] = datban.Ngaydat.ToString("dd/MM/yyyy"),
                    ["GioBatDau"] = datban.Giobatdau.ToString(),
                    ["GioKetThuc"] = datban.Gioketthuc.ToString(),
                    ["LyDo"] = datban.Lydo.ToString()
                };

                var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "EmailHuyDatBan.html");

                var body = EmailTemplateHelper.PopulateTemplate(templatePath, placeholders);

                await _emailService.SendEmailAsync(email, "Xác nhận hủy đặt bàn thành công", body);
            }

            return RedirectToAction("Index", new { idChiNhanh = datban.Idchinhanh, trangThai = "Đã hủy" });
        }

    }

}
