using WebsiteOrdering.Areas.ViewModelAdmin;
using WebsiteOrdering.Enums;
using WebsiteOrdering.Models;

namespace WebsiteOrdering.Areas.Services
{
    public interface IOrderService
    {
        List<TrangThai> GetAvailableStatuses(TrangThai currentStatus);
        Task<(List<Donhang> Orders, int TotalCount)> GetPagedFilteredOrdersAsync(OrderFilterModel filter);
        (DateTime startDate, DateTime endDate) GetDateRange(string dateFilter, DateTime? fromDate, DateTime? toDate);

    }
}
