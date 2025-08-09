using WebsiteOrdering.Models;
using WebsiteOrdering.ViewModels;
namespace WebsiteOrdering.Areas.Staff.ViewModels
{
  
    public class OrderSessionViewModel
    {
        public string IdDatBan { get; set; }
        public string IdNhanVien { get; set; }
        public List<CartItem> ChiTietMonAn { get; set; } = new();
        public string TrangThai { get; set; } = "Đang dùng bữa";
        public Monan? ChiTietMon { get; set; }
        public List<Loaimonan> LoaiMonList { get; set; } = new();
        public List<Monan> MonAnList { get; set; } = new();
        public string? IdLoai { get; set; }

        public decimal TongTien
        {
            get
            {
                return ChiTietMonAn.Sum(m => m.TongTien);
            }
        }

    }

}
