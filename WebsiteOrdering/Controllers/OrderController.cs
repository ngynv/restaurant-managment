using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteOrdering.Enums;
using WebsiteOrdering.Models;
using WebsiteOrdering.Repositories;

namespace WebsiteOrdering.Controllers
{
    public class OrderController : Controller
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IOrderRepository _orderRepository;

        public OrderController(IAccountRepository accountRepository, AppDbContext context, 
            IOrderRepository orderRepository)
        {
            _accountRepository = accountRepository;
            _orderRepository = orderRepository;
        }
        [HttpGet]
        public async Task<IActionResult> MyOrders()
        {
            // Lấy user đang đăng nhập
            var user = await _accountRepository.GetCurrentUserAsync(User);
            if (user == null)
            {
                return View(new List<Donhang>());
            }

            // Truy vấn đơn hàng theo Id người dùng
            var orders = await _orderRepository.GetOrdersByUserIdAsync(user.Id);
            return View(orders);
        }
        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            var order = await _orderRepository.GetOrderWithDetailsAsync(id);
            if (order == null)
            {
                return NotFound("Không tìm thấy đơn hàng.");
            }
            return View(order);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(string id)
        {
            var user = await _accountRepository.GetCurrentUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var order = await _orderRepository.GetOrderWithDetailsAsync(id);
            if (order == null || order.Idngdung != user.Id)
            {
                return NotFound("Không tìm thấy đơn hàng.");
            }

            if (order.Trangthai != TrangThai.Pending && order.Trangthai != TrangThai.Paid && order.Trangthai != TrangThai.Preparing)
            {
                return BadRequest("Chỉ có thể hủy đơn hàng khi đang chuẩn bị");
            }

            var result = await _orderRepository.CancelOrderAsync(id);
            if (!result)
            {
                return StatusCode(500, "Hủy đơn hàng thất bại.");
            }

            return RedirectToAction("Personal", "Personal", new { tab = "orders" });
        }

    }
}
