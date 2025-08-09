using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebsiteOrdering.Models
{
    public partial class Chitietdatban
    {
        public TimeOnly Giovao { get; set; }

        public TimeOnly Giora { get; set; }

        public string Iddatban { get; set; } = null!;

        public string Idban { get; set; } = null!;

        public virtual Ban IdbanNavigation { get; set; } = null!;

        public virtual Datban IddatbanNavigation { get; set; } = null!;
    }
}
