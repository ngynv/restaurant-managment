namespace WebsiteOrdering.ViewModels
{
    public class SelectedStoreViewModel
    {
        public string BranchId { get; set; } = null!;
        public string DeliveryMethod { get; set; } = "pickup"; // mặc định pickup
        public double DistanceKm { get; set; }
        public int EstimatedMinutes { get; set; }
    }
}
