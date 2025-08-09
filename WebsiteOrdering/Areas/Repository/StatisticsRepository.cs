using Microsoft.EntityFrameworkCore;
using WebsiteOrdering.Areas.Staff.ViewModels;
using WebsiteOrdering.Areas.ViewModelAdmin;
using WebsiteOrdering.Enums;
using WebsiteOrdering.Models;

namespace WebsiteOrdering.Areas.Repository
{
    public class StatisticsRepository : IStatisticsRepository
    {
        private readonly AppDbContext _context;

        public StatisticsRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<decimal> GetTotalRevenueAsync()
        {
            return await _context.dhang
                .Where(d =>
                    (d.Iddatban == null && d.Trangthai == TrangThai.Completed) ||
                    (d.Iddatban != null && d.Trangthai == TrangThai.Completed))
                .SumAsync(d => d.Tongtien);
        }
        public async Task<decimal> GetRevenueByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            return await _context.dhang
                .Where(d => d.Trangthai == TrangThai.Completed &&
                            d.Ngaydat >= fromDate && d.Ngaydat <= toDate)
                .SumAsync(d => d.Tongtien);
        }
        public async Task<List<RevenueByDateDto>> GetRevenueByDateRangeChartAsync(DateTime fromDate, DateTime toDate, string? branchId, string? type)
        {
            var query = _context.dhang
                .Where(d => d.Ngaydat >= fromDate && d.Ngaydat <= toDate);

            if (!string.IsNullOrEmpty(branchId))
            {
                query = query.Where(d => d.Idchinhanh == branchId);
            }

            // Áp dụng lọc theo loại đơn
            if (type == "online")
            {
                query = query.Where(d => d.Iddatban == null && d.Trangthai == TrangThai.Completed);
            }
            else if (type == "restaurant")
            {
                query = query.Where(d => d.Iddatban != null && d.Trangthai == TrangThai.Completed);
            }
            else
            {
                query = query.Where(d =>
                    (d.Iddatban == null && d.Trangthai == TrangThai.Completed) ||
                    (d.Iddatban != null && d.Trangthai == TrangThai.Completed));
            }

            return await query
                .GroupBy(d => d.Ngaydat.Date)
                .OrderBy(g => g.Key)
                .Select(g => new RevenueByDateDto
                {
                    DateLabel = g.Key.ToString("dd/MM"),
                    Revenue = g.Sum(x => x.Tongtien)
                })
                .ToListAsync();
        }
        public async Task<List<RevenueByDateDto>> GetDailyRevenueInMonthAsync(int month, int year, string? branchId, string? type)
        {
            var query = _context.dhang
                .Where(d => d.Ngaydat.Month == month && d.Ngaydat.Year == year);

            // Lọc theo chi nhánh nếu có
            if (!string.IsNullOrEmpty(branchId))
            {
                query = query.Where(d => d.Idchinhanh == branchId);
            }

            // Lọc theo loại đơn hàng
            if (type == "online")
            {
                query = query.Where(d => d.Iddatban == null && d.Trangthai == TrangThai.Completed);
            }
            else if (type == "restaurant")
            {
                query = query.Where(d => d.Iddatban != null && d.Trangthai == TrangThai.Completed);
            }
            else
            {
                query = query.Where(d =>
                    (d.Iddatban == null && d.Trangthai == TrangThai.Completed) ||
                    (d.Iddatban != null && d.Trangthai == TrangThai.Completed));
            }
            return await query
                .GroupBy(d => d.Ngaydat.Date)
                .OrderBy(g => g.Key)
                .Select(g => new RevenueByDateDto
                {
                    DateLabel = g.Key.ToString("dd/MM"),
                    Revenue = g.Sum(x => x.Tongtien)
                })
                .ToListAsync();
        }
        public async Task<List<RevenueByDateDto>> GetMonthlyRevenueInYearAsync(int year, string? branchId, string? type)
        {
            var query = _context.dhang.Where(d => d.Ngaydat.Year == year);

            if (!string.IsNullOrEmpty(branchId))
            {
                query = query.Where(d => d.Idchinhanh == branchId);
            }

            if (!string.IsNullOrEmpty(branchId))
            {
                query = query.Where(d => d.Idchinhanh == branchId);
            }

            // Lọc theo loại đơn hàng
            if (type == "online")
            {
                query = query.Where(d => d.Iddatban == null && d.Trangthai == TrangThai.Completed);
            }
            else if (type == "restaurant")
            {
                query = query.Where(d => d.Iddatban != null && d.Trangthai == TrangThai.Completed);
            }
            else
            {
                query = query.Where(d =>
                    (d.Iddatban == null && d.Trangthai == TrangThai.Completed) ||
                    (d.Iddatban != null && d.Trangthai == TrangThai.Completed));
            }

            return await query
                .GroupBy(d => d.Ngaydat.Month)
                .OrderBy(g => g.Key)
                .Select(g => new RevenueByDateDto
                {
                    DateLabel = "Tháng " + g.Key,
                    Revenue = g.Sum(x => x.Tongtien)
                })
                .ToListAsync();
        }
        public async Task<List<MonAnThongKeViewModel>> GetThongKeMonAnAsync()
        {
            var monans = await _context.SanPhams
                .Select(m => new MonAnThongKeViewModel
                {
                    IdMon = m.Idmonan,
                    TenMon = m.Tenmonan,
                    SoLuongBan = m.SoLuongBan,
                    SoLanDuocGhep = _context.MonAnGhepStats
                                            .Where(s => s.Idmonan == m.Idmonan)
                                            .Sum(s => (int?)s.SoLanDuocGhep) ?? 0
                })
                .ToListAsync();

            return monans;
        }
    }
}
