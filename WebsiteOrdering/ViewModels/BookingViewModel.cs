using WebsiteOrdering.Models;

namespace WebsiteOrdering.ViewModels
{
    public class BookingViewModel
    {
        public string? Iddatban { get; set; }
        public DateOnly Ngaydat { get; set; }
        public TimeOnly Giobatdau { get; set; }
        public int Songuoidat { get; set; }
        public string? Ghichu { get; set; }
        public string? Idchinhanh { get; set; }
        public string? SelectedIdban { get; set; }

        // Người dùng
        public string? UserEmail { get; set; }
        public string? UserPhoneNumber { get; set; }
        public string? UserFullName { get; set; }

        // Tên & sđt người đặt
        public string? Tenngdat { get; set; }
        public string? Sdtngdat { get; set; }
    }
}
