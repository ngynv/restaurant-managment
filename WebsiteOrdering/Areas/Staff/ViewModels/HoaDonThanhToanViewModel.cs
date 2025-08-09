using WebsiteOrdering.Models;

namespace WebsiteOrdering.Areas.Staff.ViewModels
{
    public class HoaDonThanhToanViewModel
    {
        public OrderSessionViewModel OrderSession { get; set; }
        public Datban Datban { get; set; }
        public Chitietdatban Chitietdatban { get; set; }
       
        public int? TienKhachDua { get; set; }
        public int? TienThua { get; set; }
        public string PhuongThucThanhToan { get; set; }
    }
}
