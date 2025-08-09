
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebsiteOrdering.Models
{
    public partial class Chinhanh
    {
        public string Idchinhanh { get; set; } = null!;

        public string Tencnhanh { get; set; } = null!;

        public string Diachicn { get; set; } = null!;
        [Precision(9, 6)]
        public decimal Latitude { get; set; }

        [Precision(9, 6)]
        public decimal Longitude { get; set; }

        public virtual ICollection<Ban> Bans { get; set; } = new List<Ban>();

        public virtual ICollection<Datban> Datbans { get; set; } = new List<Datban>();

        public virtual ICollection<Donhang> Donhangs { get; set; } = new List<Donhang>();

        public virtual ICollection<ApplicationUser> Nguoidungs { get; set; } = new List<ApplicationUser>();
    }
}
