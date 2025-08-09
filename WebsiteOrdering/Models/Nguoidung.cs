using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
namespace WebsiteOrdering.Models
{
    [Table("NGUOIDUNG")]
    public class Nguoidung
    {
        [Key]
        [Column("IDNGDUNG")]
        [StringLength(5)]
        public string Idnguoidung { get; set; } = null!;

        [Column("TENND")]
        [StringLength(100)]
        public string Ten { get; set; } = null!;

        [Column("SĐTND")]
        public int? Sodt { get; set; }

        [Column("MATKHAU")]
        [StringLength(50)]
        public string Matkhau { get; set; } = null!;

        [Column("CHUCVUND")]
        [StringLength(50)]
        public string Chucvu { get; set; } = null!;

        [Column("EMAILND")]
        [StringLength(100)]
        public string Email { get; set; } = null!;

        [Column("GIOITINHND")]
        [StringLength(50)]
        public string? Gioitinh { get; set; }

        [Column("NGAYSINH")]
        public DateTime? Ngaysinh { get; set; }

        [Column("IDCHINHANH")]
        [StringLength(5)]
        public string? Idchinhanh { get; set; }

        public Chinhanh? Chinhanh { get; set; }
        public ICollection<Datban> Datbans { get; set; } = new List<Datban>();
        public ICollection<Donhang> Donhangs { get; set; } = new List<Donhang>();
    }
}
