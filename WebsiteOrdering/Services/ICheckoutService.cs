using WebsiteOrdering.Enums;
using WebsiteOrdering.ViewModels;

namespace WebsiteOrdering.Services
{
    public interface ICheckoutService
    {
        List<CartItem> GetSelectedItems(List<CartItem> cart, List<string> selectedIds);
        decimal CalculateTotalAmount(List<CartItem> selectedItems);
        decimal CalculateShipFee(decimal km);
        Task<string> CreateOrderAsync(List<CartItem> selectedItems, UserCheckoutInfoViewModel userInfo, string? userId);
        Task UpdateOrderPaymentStatusAsync(string orderId, TrangThai status, string transactionId);

    }
}
