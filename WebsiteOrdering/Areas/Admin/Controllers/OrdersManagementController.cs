using Microsoft.AspNetCore.Mvc;
using WebsiteOrdering.Areas.Services;
using WebsiteOrdering.Areas.ViewModelAdmin;
using WebsiteOrdering.Enums;
using WebsiteOrdering.Repositories;

namespace WebsiteOrdering.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/OrdersManagement")]
    public class OrdersManagementController : Controller
    {

        private readonly IOrderRepository _orderRepository;
        private readonly IOrderService _orderService;

        public OrdersManagementController(IOrderRepository orderRepository, IOrderService orderService)
        {
            _orderRepository = orderRepository;
            _orderService = orderService;
        }
        [HttpGet("")]
        public async Task<IActionResult> Index(
    TrangThai? status,
    string? branchId,
    string keyword,
    string dateFilter,
    DateTime? fromDate,
    DateTime? toDate,
    int page = 1,
    int pageSize = 14)
        {
            var filter = new OrderFilterModel
            {
                Status = status,
                Keyword = keyword,
                DateFilter = dateFilter,
                FromDate = fromDate,
                ToDate = toDate,
                Page = page,
                ChiNhanhId = branchId,
                PageSize = pageSize,
            };

            var (orders, totalCount) = await _orderService.GetPagedFilteredOrdersAsync(filter);

            var statusMap = orders.ToDictionary(
                o => o.Iddonhang,
                o => _orderService.GetAvailableStatuses(o.Trangthai).Select(s => s.ToString()).ToList()
            );

            SetViewBagData(filter, statusMap, totalCount);
            return View(orders);
        }
        private void SetViewBagData(OrderFilterModel filter, Dictionary<string, List<string>> statusMap, int totalCount)
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
            ViewBag.BranchId = filter.ChiNhanhId;
        }
        [HttpGet("Details")]
        public async Task<IActionResult> Details(string id)
        {
            var order = await _orderRepository.GetOrderWithDetailsAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            ViewBag.AvailableStatuses = _orderService.GetAvailableStatuses(order.Trangthai);
            return View(order);
        }
        [HttpPost("UpdateStatus")]
        public async Task<IActionResult> UpdateStatus(string id, TrangThai newStatus)
        {
            var success = await _orderRepository.UpdateOrderStatusAsync(id, newStatus);

            if (!success)
            {
                TempData["Error"] = "Không thể cập nhật trạng thái đơn hàng. Đơn có thể đã bị huỷ hoặc đang trong quá trình giao.";
            }
            else
            {
                TempData["Success"] = "Cập nhật trạng thái thành công.";
            }

            return RedirectToAction(nameof(Index));
        }
      
    }
}
