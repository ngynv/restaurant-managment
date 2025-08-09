
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebsiteOrdering.ViewModels;

namespace WebsiteOrdering.Models
{
    public partial class Debanh
    {
        public string Iddebanh { get; set; } = null!;

        public string Tendebanh { get; set; } = null!;

        public int Giadebanh { get; set; }

        public virtual ICollection<Chitietdonhang> Chitietdonhangs { get; set; } = new List<Chitietdonhang>();
    }
}
