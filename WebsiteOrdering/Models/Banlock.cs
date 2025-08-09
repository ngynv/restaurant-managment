using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebsiteOrdering.Models
{
    public class Banlock
    {
        [Key]
        public string IdBanLock { get; set; }
        public string IdBan { get; set; }
        public TimeOnly BatDau { get; set; }
        public TimeOnly KetThuc { get; set; }
        public DateOnly Ngay { get; set; }
        [ForeignKey("IdBan")]
        public virtual Ban Ban { get; set; } = null!;
    }
}
