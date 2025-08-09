namespace WebsiteOrdering.ViewModels
{
    public class PendingCheckoutData
    {
        public List<CartItem> SelectedItems { get; set; }
        public UserCheckoutInfoViewModel UserInfo { get; set; }
        public string UserId { get; set; }
        public List<CartItem> CartItems { get; set; }
    }
}
