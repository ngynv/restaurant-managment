using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebsiteOrdering.ViewModels;

namespace WebsiteOrdering.Models
{
    public partial class Loaimonan
    {
        public string Idloaimonan { get; set; } = null!;

        public string Tenloaimonan { get; set; } = null!;

        public string? IdloaimanCha { get; set; }

        public virtual ICollection<Listgiasize> Listgiasizes { get; set; } = new List<Listgiasize>();

        public virtual ICollection<Monan> Monans { get; set; } = new List<Monan>();

        public virtual ICollection<Topping> Toppings { get; set; } = new List<Topping>();
    }
}
