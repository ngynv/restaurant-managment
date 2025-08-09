using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebsiteOrdering.Models
{
    public partial class Chitiettopping
    {
        public string Idtopping { get; set; } = null!;

        public string IdChitiet { get; set; } = null!;

        public virtual Chitietdonhang Chitietdonhang { get; set; } = null!;

        public virtual Topping IdtoppingNavigation { get; set; } = null!;
    }
}
