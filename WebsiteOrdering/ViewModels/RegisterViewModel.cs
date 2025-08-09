using System.ComponentModel.DataAnnotations;

namespace WebsiteOrdering.ViewModels
{
    public class RegisterViewModel
    {

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ")]
        [Display(Name = "Email")]
        [StringLength(256, ErrorMessage = "Email không được vượt quá 256 ký tự")]
        public required string Email { get; set; }


        [Required(ErrorMessage = "Họ tên là bắt buộc")]
       
        [Display(Name = "Họ và tên")]
        [StringLength(256, ErrorMessage = "Họ tên không được vượt quá 256 ký tự")]
        public required string FullName { get; set; }
        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất {2} ký tự và tối đa {1} ký tự")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public required string Password { get; set; }

        [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public required string ConfirmPassword { get; set; }
    }
}
