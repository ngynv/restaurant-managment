namespace WebsiteOrdering.ViewModels
{
    public class CheckoutViewModel
    {
        public List<CartItem> CartItems { get; set; }
        public UserCheckoutInfoViewModel UserInfo { get; set; }
        public List<string> SelectedIds { get; set; }
    }
}
