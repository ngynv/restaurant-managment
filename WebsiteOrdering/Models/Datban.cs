using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using WebsiteOrdering.Enums;

namespace WebsiteOrdering.Models
{
    public partial class Datban
    {
        public string Iddatban { get; set; } = null!;

        public DateOnly Ngaydat { get; set; }

        public TimeOnly Giobatdau { get; set; }

        public TimeOnly Gioketthuc { get; set; }

        public int Songuoidat { get; set; }

        public string? Ghichu { get; set; }

        public TrangThai Trangthaidatban { get; set; }
        [Column("IDNGDUNG")]
        public string? Idngdung { get; set; }
        public string? Tenngdat {  get; set; }
        public string? Sđtngdat { get; set; }

        public string Idchinhanh { get; set; } = null!;
        public string? Lydo {  get; set; }
        public virtual ICollection<Chitietdatban> Chitietdatbans { get; set; } = new List<Chitietdatban>();

        public virtual ICollection<Donhang> Donhangs { get; set; } = new List<Donhang>();

        public virtual Chinhanh IdchinhanhNavigation { get; set; } = null!;
        [ForeignKey("Idngdung")]
        public virtual ApplicationUser? Nguoidung { get; set; }
    }
}
