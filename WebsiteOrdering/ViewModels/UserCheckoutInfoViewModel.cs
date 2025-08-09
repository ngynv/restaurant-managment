using System.ComponentModel.DataAnnotations;

namespace WebsiteOrdering.ViewModels
{
    public class UserCheckoutInfoViewModel
    {
        // Thông tin người nhận hàng
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Số điện thoại phải gồm đúng 10 chữ số")]
        public string PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? BranchId { get; set; }
        public string? BranchName { get; set; }
        public double? DistanceKm { get; set; }
        public int? EstimatedMinutes { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
        public string? PaymentInfo { get; set; }
        public string? DeliveryMethod { get; set; }
    }
}
