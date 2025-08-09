using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using WebsiteOrdering.ViewModels;

namespace WebsiteOrdering.Models
{
   // [Table("MONAN")]
    //[PrimaryKey(nameof(IDMONAN))]

    public partial class Monan
    {

        //public string IDMONAN { get; set; }

        //public string? TENMONAN { get; set; }

        //public string? MOTA { get; set; }

        //public int GIAMONAN { get; set; }

        //public string? ANHMONAN { get; set; }

        //public string? TRANGTHAIMAN { get; set; }


        //public string IDLoaiMonAn { get; set; }
        //[NotMapped]
        //public List<SanPhamViewModel> PizzaGhep { get; set; } = new List<SanPhamViewModel>();

        //[ForeignKey(nameof(IDLoaiMonAn))]
        //public virtual Loaimonan Category { get; set; }

        //public List<ListGiaSizeViewModel> ListGiaSizes { get; set; }
        //// public List<DeBanhViewModel> DeBanh { get; set; }
        //[NotMapped]
        //public List<DeBanhViewModel> DeBanh { get; set; } = new();
        //public List<ToppingViewModel> Toppings { get; set; }

        ////[NotMapped]
        ////public int TongTien { get; set; } =0;
        //[NotMapped]
        //public int SoLuong { get; set; } = 1;

        public string Idmonan { get; set; } = null!;

        public string Tenmonan { get; set; } = null!;

        public int Giamonan { get; set; }
        public int SoLuongBan { get; set; }

        public string Anhmonan { get; set; } = null!;

        public string Mota { get; set; } = null!;

        public string Trangthaiman { get; set; } = null!;

        public string Idloaimonan { get; set; } = null!;

        public virtual ICollection<Chitietdonhang> Chitietdonhangs { get; set; } = new List<Chitietdonhang>();

        public virtual Loaimonan IdloaimonanNavigation { get; set; } = null!;
        public List<Listgiasize> ListGiaSizes { get; set; }
        [NotMapped]
        public List<SanPhamViewModel> PizzaGhep { get; set; } = new List<SanPhamViewModel>();

        [NotMapped]
        public List<Debanh> DeBanh { get; set; } = new();
        public List<Topping> Toppings { get; set; }
        [NotMapped]
        public int SoLuong { get; set; } = 1;
    }

    public class SanPhamViewModel
    {
        public string IDMONAN2 { get; set; }
        public string TENMONAN2 { get; set; }
        public int GIACOBAN2 { get; set; }
        public string ANHMONAN2 { get; set; }
    }
}
