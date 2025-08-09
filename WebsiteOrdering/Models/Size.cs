using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebsiteOrdering.ViewModels;

namespace WebsiteOrdering.Models
{
    public partial class Size
    {
        public string Idsize { get; set; } = null!;

        public string Tensize { get; set; } = null!;

        public virtual ICollection<Chitietdonhang> Chitietdonhangs { get; set; } = new List<Chitietdonhang>();

        public virtual ICollection<Listgiasize> Listgiasizes { get; set; } = new List<Listgiasize>();
    }
}
