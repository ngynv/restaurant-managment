using Microsoft.EntityFrameworkCore;

namespace WebsiteOrdering.Models
{
    public class Location
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        [Precision(9, 6)]
        public decimal Latitude { get; set; }

        [Precision(9, 6)]
        public decimal Longitude { get; set; }
    }
}
