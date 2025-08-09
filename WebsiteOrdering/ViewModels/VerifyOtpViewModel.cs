using System.ComponentModel.DataAnnotations;

namespace WebsiteOrdering.ViewModels
{
    public class VerifyOtpViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [MaxLength(10),MinLength(10)]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã OTP.")]
        [StringLength(6, ErrorMessage = "Mã OTP phải 6 ký tự.")]
        public string OtpInput { get; set; }
    }
}
