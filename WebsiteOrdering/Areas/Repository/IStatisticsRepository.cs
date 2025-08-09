using WebsiteOrdering.Areas.Staff.ViewModels;
using WebsiteOrdering.Areas.ViewModelAdmin;

namespace WebsiteOrdering.Areas.Repository
{
    public interface IStatisticsRepository
    {
        Task<decimal> GetTotalRevenueAsync();
        Task<decimal> GetRevenueByDateRangeAsync(DateTime fromDate, DateTime toDate);
        Task<List<RevenueByDateDto>> GetDailyRevenueInMonthAsync(int month, int year, string? branchId, string? type);
        Task<List<RevenueByDateDto>> GetRevenueByDateRangeChartAsync(DateTime fromDate, DateTime toDate, string? branchId, string? type);
        Task<List<RevenueByDateDto>> GetMonthlyRevenueInYearAsync(int year, string? branchId, string? type);
        Task<List<MonAnThongKeViewModel>> GetThongKeMonAnAsync();
    }
}
