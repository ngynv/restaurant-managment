
using Microsoft.EntityFrameworkCore;
using WebsiteOrdering.Services;
using WebsiteOrdering.Helper;
using WebsiteOrdering.Models;
using WebsiteOrdering.Enums;

namespace WebsiteOrdering.Services
{
    public class DatBanBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public DatBanBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var _appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var _emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                    var now = DateTime.Now;

                    // Xử lý nhắc nhở trước 30p
                    var datbansNhac = await _appDbContext.Datbans
                        .Where(d => d.Trangthaidatban == TrangThai.Confirmed
                            && d.Ngaydat == DateOnly.FromDateTime(now.Date)
                            && d.Giobatdau <= TimeOnly.FromDateTime(now.AddMinutes(30))
                            && d.Giobatdau > TimeOnly.FromDateTime(now))
                        .Include(d => d.Nguoidung)
                        .Include(d => d.IdchinhanhNavigation)
                        .ToListAsync();

                    foreach (var datban in datbansNhac)
                    {
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

                            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "EmailNhacDatBan.html");
                            var body = EmailTemplateHelper.PopulateTemplate(templatePath, placeholders);

                            await _emailService.SendEmailAsync(email, "Nhắc nhở đặt bàn sắp đến", body);
                        }
                    }

                    // Xử lý hủy sau 15p nếu không đến
                    var datbansHuy = await _appDbContext.Datbans
                        .Where(d => d.Trangthaidatban == TrangThai.Confirmed
                            && d.Ngaydat == DateOnly.FromDateTime(now.Date)
                            && d.Giobatdau.AddMinutes(15) <= TimeOnly.FromDateTime(now))
                        .Include(d => d.Nguoidung)
                        .Include(d => d.IdchinhanhNavigation)
                        .ToListAsync();

                    foreach (var datban in datbansHuy)
                    {
                        datban.Trangthaidatban = TrangThai.Cancelled;
                        datban.Lydo = "Khách không đến sau 15 phút";

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

                            await _emailService.SendEmailAsync(email, $"Hủy đặt bàn do không đến - Mã đơn đặt bàn :{datban.Iddatban}", body);
                        }
                    }

                    await _appDbContext.SaveChangesAsync();
                }

                // Chờ 1 phút rồi chạy lại
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
