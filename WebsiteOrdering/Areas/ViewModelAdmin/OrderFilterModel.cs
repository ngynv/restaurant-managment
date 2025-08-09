using WebsiteOrdering.Enums;

namespace WebsiteOrdering.Areas.ViewModelAdmin
{
    public class OrderFilterModel
    {
        public TrangThai? Status { get; set; }
        public string Keyword { get; set; }
        public string DateFilter { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 14;
        public string? ChiNhanhId { get; set; }
    }
}
