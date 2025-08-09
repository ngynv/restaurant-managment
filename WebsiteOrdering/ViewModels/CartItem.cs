using WebsiteOrdering.Models;

namespace WebsiteOrdering.ViewModels
{
    public class CartItem
    {
        public string IDMONAN { get; set; }

        public string TENSANPHAM { get; set; }
        public string ANHSANPHAM { get; set; }
        public string? IDMMONAN2 { get; set; }
        public string? TENSANPHAM2 { get; set; }

        public string? ANHSANPHAM2 { get; set; }
        public string? Size { get; set; }
        public string? DeBanh { get; set; }
        public string? GhiChu { get; set; }
        public int SoLuong { get; set; }
        public int GiaCoBan { get; set; }
        public int? GiaSize { get; set; }
        public int? GiaDeBanh { get; set; }
        public string? IdDeBanh { get; set; }
        public string? TenSize { get; set; }
        public string? TenDeBanh { get; set; }
        public List<Topping>? Topping { get; set; } = new List<Topping>();

        public int TongTien
        {
            get
            {
                int tongTopping = Topping?.Sum(t => t.Giatopping) ?? 0;
                int size = GiaSize ?? 0;
                int de = GiaDeBanh ?? 0;
                return (GiaCoBan + size + de + tongTopping) * SoLuong;
            }
        }
        public decimal DonGia
        {
            get
            {
                decimal tongTopping = Topping?.Sum(t => t.Giatopping) ?? 0;
                return GiaCoBan + (GiaSize ?? 0) + (GiaDeBanh ?? 0) + tongTopping;
            }
        }
        public List<SanPhamViewModel> SanPham { get; set; }

        public CartItem()
        {

        }
        public CartItem(Monan sanpham, string? idSize, List<Listgiasize> listGiaSizes, List<Topping>? topping, Debanh? debanh, string ghiChu = "", Monan? sanphamGhep = null)
        {
            IDMONAN = sanpham.Idmonan;
            TENSANPHAM = sanpham.Tenmonan;
            ANHSANPHAM = sanpham.Anhmonan;
            SoLuong = 1;
            GhiChu = ghiChu;

            // Nếu có pizza ghép
            if (sanphamGhep != null)
            {
                IDMMONAN2 = sanphamGhep.Idmonan;
                TENSANPHAM2 = sanphamGhep.Tenmonan;
                ANHSANPHAM2 = sanphamGhep.Anhmonan ?? "default.jpg";
                GiaCoBan = (sanpham.Giamonan + sanphamGhep.Giamonan) / 2;
            }
            else
            {
                GiaCoBan = sanpham.Giamonan;
                ANHSANPHAM2 = "";
            }

            // Gán size và giá size từ listGiaSizes
            if (!string.IsNullOrEmpty(idSize))
            {
                var giaSize = listGiaSizes.FirstOrDefault(x => x.Idsize == idSize);
                if (giaSize != null)
                {
                  // TenSize = giaSize.IdsizeNavigation?.Tensize;
                    Size = giaSize.IdsizeNavigation?.Tensize;
                    GiaSize = giaSize.Giasize;
                }
            }
         

            // Gán đế bánh
            if (debanh != null)
            {
                IdDeBanh = debanh.Iddebanh;
                DeBanh = debanh.Tendebanh;
                GiaDeBanh = debanh.Giadebanh;
            }

            // Gán topping
            if (topping != null)
            {
                Topping = topping;
            }

        }


    }

}
