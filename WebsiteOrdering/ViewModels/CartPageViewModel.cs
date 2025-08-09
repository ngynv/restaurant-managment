using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace WebsiteOrdering.ViewModels
{
    public class CartPageViewModel
    {
        // Danh sách món trong giỏ hàng
        public List<CartItem> CartItems { get; set; } = new();
        public UserCheckoutInfoViewModel UserInfo { get; set; } = new();

    }
}
