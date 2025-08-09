using System.ComponentModel.DataAnnotations;

namespace WebsiteOrdering.Areas.ViewModelAdmin
{
    public class MonanFormViewModel
    {
        public string? Idmonan { get; set; }

        [Required(ErrorMessage = "Tên món ăn là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên món ăn không được vượt quá 100 ký tự")]
        public string Tenmonan { get; set; }

        [Required(ErrorMessage = "Loại món ăn là bắt buộc")]
        public string Idloaimonan { get; set; }

        public string? Anhmonan { get; set; }

        [Required(ErrorMessage = "Giá món ăn là bắt buộc")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn hoặc bằng 0")]
        public int Giamonan { get; set; }

        public string? Trangthaiman { get; set; }

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        public string? Mota { get; set; }
    }
}
