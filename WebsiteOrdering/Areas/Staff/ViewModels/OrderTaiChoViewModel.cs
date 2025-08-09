using WebsiteOrdering.Models;

namespace WebsiteOrdering.Areas.Staff.ViewModels
{
    public class OrderTaiChoViewModel
    {
        public List<Loaimonan> LoaiMonList { get; set; }
        public List<Monan> MonAnList { get; set; }
        public Monan? ChiTietMon { get; set; }
        public OrderSessionViewModel OrderSession { get; set; }
        public List<Monan> PizzaGhepList { get; set; }
        public Dictionary<string, OrderSessionViewModel>? AllSessions { get; set; }

        // Dùng để hiển thị lại khi chỉnh sửa
        public string? IdSizeSelected { get; set; }
        public string? IdDeBanhSelected { get; set; }
        public string? IdPizzaGhepSelected { get; set; }
        public List<string> ToppingsSelected { get; set; } = new();
        public int SoLuongSelected { get; set; }
        public string? GhiChuSelected { get; set; }
        public bool IsUpdate { get; set; }

    }

}
