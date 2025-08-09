namespace WebsiteOrdering.ViewModels
{
    public class UserLocationSessionViewModel
    {
        public string NearestBranchId { get; set; } = null!;
        public double DistanceKm { get; set; }
        public int EstimatedMinutes { get; set; }
        public string DeliveryMethod { get; set; } = "delivery";
    }
}
