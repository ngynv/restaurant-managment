using System.Collections.Generic;
using WebsiteOrdering.Models;

namespace WebsiteOrdering.ViewModels
{
    public class MenuWithCartViewModel
    {
        public List<Monan> Products { get; set; }
        public List<CartItem> CartItems { get; set; }
    }
}
