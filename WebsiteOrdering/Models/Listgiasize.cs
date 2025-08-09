using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace WebsiteOrdering.Models
{
    public partial class Listgiasize
    {
        public int Giasize { get; set; }

        public string Idloaimonan { get; set; } = null!;

        public string Idsize { get; set; } = null!;

        public virtual Loaimonan IdloaimonanNavigation { get; set; } = null!;

        public virtual Size IdsizeNavigation { get; set; } = null!;
    }
}
