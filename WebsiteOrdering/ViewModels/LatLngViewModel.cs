namespace WebsiteOrdering.ViewModels
{
    public class LatLngViewModel
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Address { get; set; } = string.Empty;
        public string DeliveryMethod { get; set; } = "delivery";
    }
}
