using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
namespace WebsiteOrdering.Models
{
    
    public partial class Ban
    {
        public string Idban { get; set; } = null!;

        public string Tenban { get; set; } = null!;

        public int Songuoi { get; set; }

        public string Trangthaiban { get; set; } = null!;

        public string Khuvuc { get; set; } = null!;
        public string X { get; set; }
        public string Y { get; set; }

        public string Idchinhanh { get; set; } = null!;

        public virtual ICollection<Chitietdatban> Chitietdatbans { get; set; } = new List<Chitietdatban>();
        public virtual ICollection<Banlock> Banlock { get; set; } = new List<Banlock>();
        public virtual Chinhanh IdchinhanhNavigation { get; set; } = null!;
    }
}
