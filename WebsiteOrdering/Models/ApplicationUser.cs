using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebsiteOrdering.Models;


namespace WebsiteOrdering.Models
{
    [Table("NGUOIDUNG")]
    public class ApplicationUser : IdentityUser
    {
        [Column("HOTEN")]
        [StringLength(100)]
        public string? FullName { get; set; }

        [Column("NGAYSINH")]
        public DateOnly? BirthDate { get; set; }

        [Column("GIOITINH")]
        [StringLength(10)]
        public string? Gender { get; set; }
        [Column("CHINHANH")]
        public string? Idchinhanh { get; set; }
        public virtual ICollection<Datban> Datbans { get; set; } = new List<Datban>();

        public virtual ICollection<Donhang> Donhangs { get; set; } = new List<Donhang>();

        [ForeignKey("Idchinhanh")]
        public virtual Chinhanh? IdchinhanhNavigation { get; set; }

    }
}
