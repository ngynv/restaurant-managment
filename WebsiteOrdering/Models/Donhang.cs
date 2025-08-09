using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebsiteOrdering.Enums;
using WebsiteOrdering.ViewModels;

namespace WebsiteOrdering.Models
{
    public partial class Donhang
    {
        public string Iddonhang { get; set; } = null!;

        public string Diachidh { get; set; } = null!;

        public DateTime Ngaydat { get; set; }

        public TrangThai Trangthai { get; set; }

        public decimal Tongtien { get; set; }

        public string Ptttoan { get; set; } = null!;

        public int? Songuoi { get; set; }
        public string? Magiaodich { get; set; }

        public string? Tenkh { get; set; }
        public string? Sdtkh { get; set; }

        public decimal? Tienship { get; set; }
        public string? DeliveryMethod { get; set; }
        public double? Khoangcachship { get; set; }
        public string? Idchinhanh { get; set; }

        public string? Iddatban { get; set; }

        public string? Idngdung { get; set; }

        public virtual ICollection<Chitietdonhang> Chitietdonhangs { get; set; } = new List<Chitietdonhang>();

        public virtual Chinhanh? IdchinhanhNavigation { get; set; }

        public virtual Datban? IddatbanNavigation { get; set; }

        public virtual ApplicationUser? IdngdungNavigation { get; set; }
    }
}
