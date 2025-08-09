using Microsoft.AspNetCore.Mvc;
using WebsiteOrdering.Areas.Services;
using WebsiteOrdering.Areas.ViewModelAdmin;
using WebsiteOrdering.Enums;

namespace WebsiteOrdering.Areas.Staff.Controllers
{
    [Area("Staff")]
    [Route("Staff/Orders")]
    public class OrdersController : Controller
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }
        [HttpGet("")]
        public async Task<IActionResult> Index(
            TrangThai? status,
            string keyword,
            string dateFilter,
            DateTime? fromDate,
            DateTime? toDate,
            int page = 1,
            int pageSize = 14)
        {
            // Lấy Id chi nhánh từ session
            var chiNhanhId = User.FindFirst("ChiNhanhId")?.Value;
            if (string.IsNullOrEmpty(chiNhanhId))
            {
                TempData["Error"] = "Không xác định được chi nhánh nhân viên.";
                return RedirectToAction("LoginStaff", "Admin", new { area = "Admin" });
            }

            var filter = new OrderFilterModel
            {
                Status = status,
                Keyword = keyword,
                DateFilter = dateFilter,
                FromDate = fromDate,
                ToDate = toDate,
                Page = page,
                PageSize = pageSize,
                ChiNhanhId = chiNhanhId
            };

            var (orders, totalCount) = await _orderService.GetPagedFilteredOrdersAsync(filter);

            var statusMap = orders.ToDictionary(
                o => o.Iddonhang,
                o => _orderService.GetAvailableStatuses(o.Trangthai)
            );

            SetViewBagData(filter, statusMap, totalCount);
            return View(orders);
        }

        private void SetViewBagData(OrderFilterModel filter, Dictionary<string, List<TrangThai>> statusMap, int totalCount)
        {
            ViewBag.StatusFilter = filter.Status;
            ViewBag.Keyword = filter.Keyword;
            ViewBag.DateFilter = filter.DateFilter;
            ViewBag.FromDate = filter.FromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = filter.ToDate?.ToString("yyyy-MM-dd");
            ViewBag.StatusMap = statusMap;
            ViewBag.Page = filter.Page;
            ViewBag.PageSize = filter.PageSize;
            ViewBag.TotalCount = totalCount;
        }
    }
}
