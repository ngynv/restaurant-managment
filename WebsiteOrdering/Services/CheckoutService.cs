using Microsoft.EntityFrameworkCore;
using WebsiteOrdering.Enums;
using WebsiteOrdering.Models;
using WebsiteOrdering.Repositories;
using WebsiteOrdering.ViewModels;

namespace WebsiteOrdering.Services
{
    public class CheckoutService : ICheckoutService
    {
        private readonly IOrderRepository _orderRepo;
        public CheckoutService(IOrderRepository orderRepo) => _orderRepo = orderRepo;

        public List<CartItem> GetSelectedItems(List<CartItem> cart, List<string> selectedIds)
        {
            var result = new List<CartItem>();
            foreach (var id in selectedIds)
            {
                var parts = id.Split('_');
                var item = cart.FirstOrDefault(c =>
                    c.IDMONAN == parts[0] &&
                    c.IDMMONAN2 == (parts[1] == "" ? null : parts[1]) &&
                    c.Size == (parts[2] == "" ? null : parts[2]) &&
                    c.DeBanh == (parts[3] == "" ? null : parts[3]) &&
                    (c.Topping?.Select(t => t.Idtopping).OrderBy(x => x)
                        .SequenceEqual(
                            (parts.Length > 4 && !string.IsNullOrEmpty(parts[4]) ? parts[4].Split(',').OrderBy(x => x) : new List<string>())
                        ) ?? true));
                if (item != null) result.Add(item);
            }
            return result;
        }
        private static string GenerateOrderId(int length = 5)
        {
            return Guid.NewGuid().ToString("N").Substring(0, length).ToUpper();
        }
        public decimal CalculateTotalAmount(List<CartItem> selectedItems)
        {
            return selectedItems.Sum(i => i.TongTien);
        }
        public decimal CalculateShipFee(decimal km)
        {
            const decimal baseDistance = 2m;
            const decimal baseFee = 15000m;
            const decimal perKmRate = 5000m;

            if (km <= baseDistance)
            {
                return baseFee;
            }
            else
            {
                return baseFee + Math.Ceiling(km - baseDistance) * perKmRate;
            }
        }
        public async Task<string> CreateOrderAsync(List<CartItem> items, UserCheckoutInfoViewModel info, string? userId)
        {
            string orderId;
            do
            {
                orderId = GenerateOrderId(5);
            } while (await _orderRepo.FindOrderAsync(orderId) != null);
            decimal shipFee = 0;
            decimal distance = 0;

            // Nếu là giao hàng thì tính khoảng cách và phí
            if (info.DeliveryMethod == "delivery")
            {
                distance = (decimal)info.DistanceKm;
                if (distance > 0)
                {
                    shipFee = CalculateShipFee(distance);
                }
            }
            else
            {
                info.Address = info.BranchName;
            }
            var order = new Donhang
            {
                Iddonhang = orderId,
                Tenkh = info.FullName,
                Sdtkh = info.PhoneNumber,
                Diachidh = info.Address,
                Tongtien = CalculateTotalAmount(items) + shipFee,
                Ngaydat = DateTime.Now,
                Trangthai = TrangThai.Pending,
                Ptttoan = info.PaymentInfo ?? "COD",
                Idngdung = userId,
                Idchinhanh = info.BranchId,
                Khoangcachship = info.DistanceKm,
                Tienship = shipFee,
                DeliveryMethod = info.DeliveryMethod,
            };

            var details = new List<Chitietdonhang>();
            var toppings = new List<Chitiettopping>();

            foreach (var item in items)
            {
                string detailId;
                do
                {
                    detailId = GenerateOrderId(5);
                } while (await _orderRepo.FindDetailAsync(detailId) != null);
                var debanhId = await _orderRepo.FindDeBanhAsync(item.DeBanh);
                var idSize = await _orderRepo.FindIdSizeAsync(item.Size);
                var pizzaGhep = "Nguyên";
                if (item.TENSANPHAM2 != null)
                {
                    pizzaGhep = "Ghép";
                }
                details.Add(new Chitietdonhang
                {
                    IdChitiet = detailId,
                    Iddonhang = orderId,
                    Idmonan = item.IDMONAN,
                    Idmonan2 = item.IDMMONAN2,
                    Idsize = idSize,
                    Iddebanh = debanhId,
                    Kieupizza = pizzaGhep,
                    Soluong = item.SoLuong,
                    Dongia = item.GiaCoBan,
                    Tongtiendh = item.TongTien,
                    Ghichu = item.GhiChu ?? "",
                   
                });
                if (item.Topping?.Any() == true)
                {
                    toppings.AddRange(item.Topping.Select(t => new Chitiettopping
                    {
                        IdChitiet = detailId,
                        Idtopping = t.Idtopping
                    }));
                }
            }

            return await _orderRepo.CreateOrderAsync(order, details, toppings);
        }
        public async Task UpdateOrderPaymentStatusAsync(string orderId, TrangThai status, string transactionId)
        {
            var order = await _orderRepo.FindOrderAsync(orderId);
            if (order != null)
            {
                if (order != null)
                {
                    order.Trangthai = status;
                    order.Magiaodich = transactionId;
                    await _orderRepo.UpdateOrderAsync(order);
                }
            }
        }
    }
}
