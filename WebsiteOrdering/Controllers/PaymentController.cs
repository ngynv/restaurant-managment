using Microsoft.AspNetCore.Mvc;
using WebsiteOrdering.Enums;
using WebsiteOrdering.Helper;
using WebsiteOrdering.Models;
using WebsiteOrdering.Services;
using WebsiteOrdering.ViewModels;

namespace WebsiteOrdering.Controllers
{
    public class PaymentController : Controller
    {
        private readonly VNPayService _vnPayService;
        private readonly ICheckoutService _checkoutService;

        public PaymentController(VNPayService vnPayService, ICheckoutService checkoutService)
        {
            _vnPayService = vnPayService;
            _checkoutService = checkoutService;
        }

        public IActionResult CreatePaymentUrl(PaymentInformationModel model)
        {
            var url = _vnPayService.CreatePaymentUrl(HttpContext, new VNPayRequestModel
            {
                OrderId = model.OrderId,
                FullName = model.Name,
                Description = model.OrderDescription,
                Amount = model.Amount,
                CreatedDate = DateTime.Now
            });
            return Redirect(url);
        }

        public async Task<IActionResult> PaymentCallback()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            if (response == null || !response.Success)
            {
                // Thanh toán thất bại - xóa pending checkout data
                HttpContext.Session.Remove("PendingCheckout");
                TempData["Error"] = "Thanh toán thất bại hoặc bị hủy bỏ";
                return RedirectToAction("Index", "Cart");
            }

            // Thanh toán thành công - lấy thông tin checkout từ session
            var checkoutData = HttpContext.Session.Get<PendingCheckoutData>("PendingCheckout");
            if (checkoutData == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin đơn hàng";
                return RedirectToAction("Index", "Cart");
            }

            try
            {
                // Tạo đơn hàng với phương thức thanh toán VNPay
                var userInfo = checkoutData.UserInfo;
                userInfo.PaymentInfo = "VNPay"; // Đảm bảo payment method là VNPay

                var orderId = await _checkoutService.CreateOrderAsync(
                    checkoutData.SelectedItems,
                    userInfo,
                    checkoutData.UserId
                );

                // Cập nhật trạng thái thanh toán cho đơn hàng vừa tạo
                await _checkoutService.UpdateOrderPaymentStatusAsync(orderId, TrangThai.Paid, response.TransactionId);

                // Xóa sản phẩm đã đặt khỏi giỏ hàng
                var cart = HttpContext.Session.Get<List<CartItem>>("Cart") ?? new();
                var selectedProductIds = checkoutData.SelectedItems.Select(x => x.IDMONAN).ToList();

                // Xóa theo ProductId
                var updatedCart = cart.Where(x => !selectedProductIds.Contains(x.IDMONAN)).ToList();

                HttpContext.Session.Set("Cart", updatedCart);

                // Xóa pending checkout data
                HttpContext.Session.Remove("PendingCheckout");

                TempData["Success"] = "Thanh toán thành công!";
                return RedirectToAction("Success", "Checkout", new { id = orderId });
            }
            catch (Exception ex)
            {
                // Log error
                TempData["Error"] = "Có lỗi xảy ra khi tạo đơn hàng";
                return RedirectToAction("Index", "Cart");
            }
        }
    }
    public class PaymentInformationModel
    {
        public string OrderId { get; set; }
        public double Amount { get; set; }
        public string Name { get; set; }
        public string OrderDescription { get; set; }
    }
}
