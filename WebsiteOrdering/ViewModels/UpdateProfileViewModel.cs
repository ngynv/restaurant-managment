using System.ComponentModel.DataAnnotations;

namespace WebsiteOrdering.ViewModels
{
    public class UpdateProfileViewModel
    {
        [Display(Name = "Họ tên")]
        [StringLength(100)]
        public string? FullName { get; set; }

        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateOnly? BirthDate { get; set; }

        [Display(Name = "Giới tính")]
        [StringLength(10)]
        public string? Gender { get; set; }

        [Display(Name = "Số điện thoại")]
        [Phone]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Email")]
        public string? Email { get; set; }
    }
}
