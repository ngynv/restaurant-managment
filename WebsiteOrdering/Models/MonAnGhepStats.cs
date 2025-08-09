using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebsiteOrdering.Models
{
    public class MonAnGhepStats
    {
        public string Idmonan { get; set; } = string.Empty;

        [Required]
        public int SoLanDuocGhep { get; set; } = 0;

        [ForeignKey("Idmonan")]
        public Monan? MonAn { get; set; }
    }
}
