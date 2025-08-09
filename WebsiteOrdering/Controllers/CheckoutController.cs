using Microsoft.AspNetCore.Mvc;
using WebsiteOrdering.ViewModels;
using WebsiteOrdering.Helper;
using WebsiteOrdering.Repositories;
using WebsiteOrdering.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;

namespace WebsiteOrdering.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ICheckoutService _checkoutService;
        private readonly IOrderRepository _orderRepository;
        private readonly IAccountRepository _accountRepository;
        public CheckoutController(IOrderRepository orderRepository, ICheckoutService checkoutService,
            IAccountRepository accountRepository)
        {
            _orderRepository = orderRepository;
            _checkoutService = checkoutService;
            _accountRepository = accountRepository;
        }
        //[HttpPost]
        //public async Task<IActionResult> Confirm(UserCheckoutInfoViewModel userInfo, [FromForm] List<string> selectedIds)
        //{

        //    var cart = HttpContext.Session.Get<List<CartItem>>("Cart") ?? new();
        //    var selectedItems = _checkoutService.GetSelectedItems(cart, selectedIds);

        //    if (!selectedItems.Any())
        //    {
        //        TempData["Error"] = "Không có món nào được chọn.";
        //        return RedirectToAction("Index", "Cart");
        //    }

        //    if (!ModelState.IsValid)
        //    {
        //        TempData["Error"] = "Vui lòng điền đầy đủ thông tin đặt hàng.";
        //        return RedirectToAction("Index", "Cart");
        //    }
        //    var userId = User.Identity?.IsAuthenticated == true ? User.FindFirst(ClaimTypes.NameIdentifier)?.Value : null;
        //    // Kiểm tra phương thức thanh toán
        //    if (userInfo.PaymentInfo == "VNPay")
        //    {
        //        // Với VNPay, lưu thông tin vào session để xử lý sau khi thanh toán thành công
        //        var checkoutData = new PendingCheckoutData
        //        {
        //            SelectedItems = selectedItems,
        //            UserInfo = userInfo,
        //            UserId = userId,
        //            CartItems = cart // Lưu toàn bộ cart để remove sau
        //        };

        //        HttpContext.Session.Set("PendingCheckout", checkoutData);

        //        // Tạo payment URL mà không tạo đơn hàng trước
        //        var tempOrderId = Guid.NewGuid().ToString(); // Tạo ID tạm thời cho VNPay
        //        var paymentModel = new PaymentInformationModel
        //        {
        //            OrderId = tempOrderId,
        //            Amount = selectedItems.Sum(x => x.TongTien),
        //            Name = userInfo.FullName,
        //            OrderDescription = $"Thanh toán đơn hàng {tempOrderId}"
        //        };

        //        return RedirectToAction("CreatePaymentUrl", "Payment", paymentModel);
        //    }
        //    else
        //    {
        //        var orderId = await _checkoutService.CreateOrderAsync(selectedItems, userInfo, userId);

        //        var updatedCart = cart.Except(selectedItems).ToList();
        //        HttpContext.Session.Set("Cart", updatedCart);

        //        return RedirectToAction("Success", new { id = orderId });
        //    }
        //}
        [HttpPost("Confirm")]
        public async Task<IActionResult> Confirm(CartPageViewModel model, [FromForm] List<string> selectedIds)
        {
            if (!User.Identity?.IsAuthenticated == true)
            {
                var returnUrl = Url.Action("Index", "Cart");
                return RedirectToAction("Login", "Account", new { returnUrl });
            }

            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index", "Cart");
            }

            return await ProcessConfirm(model.UserInfo, selectedIds);
        }


        // New method to process pending checkout after login
        public async Task<IActionResult> ProcessPendingCheckout()
        {
            if (!User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Login", "Account");
            }

            // Retrieve pending checkout data
            var pendingDataJson = HttpContext.Session.GetString("PendingCheckoutData");
            if (string.IsNullOrEmpty(pendingDataJson))
            {
                TempData["Error"] = "Không tìm thấy thông tin đặt hàng. Vui lòng thử lại.";
                return RedirectToAction("Index");
            }

            try
            {
                var pendingData = JsonSerializer.Deserialize<dynamic>(pendingDataJson);
                var userInfo = JsonSerializer.Deserialize<UserCheckoutInfoViewModel>(pendingData.GetProperty("UserInfo").GetRawText());
                var selectedIdsJson = pendingData.GetProperty("SelectedIds").GetRawText();
                var selectedIds = JsonSerializer.Deserialize<List<string>>(selectedIdsJson);

                // Clear the pending data
                HttpContext.Session.Remove("PendingCheckoutData");

                // Re-fill user info since user is now authenticated
                await _accountRepository.FillUserInfoIfAuthenticated(userInfo, User);

                return await ProcessConfirm(userInfo, selectedIds);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi xử lý đơn hàng. Vui lòng thử lại.";
                return RedirectToAction("Index");
            }
        }

        // Extract the main confirm logic to reusable method
        private async Task<IActionResult> ProcessConfirm(UserCheckoutInfoViewModel userInfo, List<string> selectedIds)
        {
            var cart = HttpContext.Session.Get<List<CartItem>>("Cart") ?? new();
            var selectedItems = _checkoutService.GetSelectedItems(cart, selectedIds);

            if (!selectedItems.Any())
            {
                TempData["Error"] = "Không có món nào được chọn.";
                return RedirectToAction("Index");
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Vui lòng điền đầy đủ thông tin đặt hàng.";
                return RedirectToAction("Index");
            }
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Kiểm tra phương thức thanh toán
            if (userInfo.PaymentInfo == "VNPay")
            {
                // Với VNPay, lưu thông tin vào session để xử lý sau khi thanh toán thành công
                var checkoutData = new PendingCheckoutData
                {
                    SelectedItems = selectedItems,
                    UserInfo = userInfo,
                    UserId = userId,
                    CartItems = cart
                };
                HttpContext.Session.Set("PendingCheckout", checkoutData);

                var tempOrderId = Guid.NewGuid().ToString();
                var paymentModel = new PaymentInformationModel
                {
                    OrderId = tempOrderId,
                    Amount = selectedItems.Sum(x => x.TongTien),
                    Name = userInfo.FullName,
                    OrderDescription = $"Thanh toán đơn hàng {tempOrderId}"
                };
                return RedirectToAction("CreatePaymentUrl", "Payment", paymentModel);
            }
            else
            {
                var orderId = await _checkoutService.CreateOrderAsync(selectedItems, userInfo, userId);
                var updatedCart = cart.Except(selectedItems).ToList();
                HttpContext.Session.Set("Cart", updatedCart);
                return RedirectToAction("Success", new { id = orderId });
            }
        }
        [HttpGet]
        public async Task<IActionResult> Success(string id)
        {
            var order = await _orderRepository.GetOrderWithDetailsAsync(id);
            if (order == null) return NotFound("Không tìm thấy đơn hàng.");

            var tongTienHang = order.Chitietdonhangs.Sum(item => item.Tongtiendh * item.Soluong);
            var thanhTien = tongTienHang + (order.Tienship ?? 0);

            var viewModel = new DonhangViewModel
            {
                Donhang = order,
                TongTienHang = tongTienHang,
                ThanhTien = (int)thanhTien
            };

            return View(viewModel);
        }
        [HttpGet]
        public async Task<IActionResult> CheckOrderStatus(string orderId)
        {
            var order = await _orderRepository.FindOrderAsync(orderId);
            if (order == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
            }

            return Json(new
            {
                success = true,
                status = order.Trangthai,
                orderDate = order.Ngaydat.ToString("dd/MM/yyyy HH:mm"),
                totalAmount = order.Tongtien
            });
        }
    }
}
