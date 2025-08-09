namespace WebsiteOrdering.ViewModels
{
    public class DatBanViewModel
    {
        public string? Iddatban { get; set; }
        public DateOnly Ngaydat { get; set; }
        public TimeOnly Giobatdau { get; set; }
        public int Songuoidat { get; set; }
        public string? Ghichu { get; set; }
        public string? Tenngdat { get; set; }
        public string? Sđtngdat { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? FullName { get; set; }
        public string Idchinhanh { get; set; } = null!;
        public string SelectedIdban { get; set; } = null!;
    }
}
