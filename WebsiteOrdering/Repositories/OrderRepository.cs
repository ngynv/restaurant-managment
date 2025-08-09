using Microsoft.EntityFrameworkCore;
using WebsiteOrdering.Enums;
using WebsiteOrdering.Models;

namespace WebsiteOrdering.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;
        private readonly IProductRepository _productRepository;
        public OrderRepository(AppDbContext context, IProductRepository productRepository)
        {
            _context = context;
            _productRepository = productRepository;
        }

        public async Task<string> CreateOrderAsync(Donhang order, List<Chitietdonhang> details, List<Chitiettopping> toppings)
        {
            _context.dhang.Add(order);
            _context.ctdh.AddRange(details);
            _context.cttopping.AddRange(toppings);
            await _context.SaveChangesAsync();
            return order.Iddonhang;
        }

        //public async Task<Donhang?> GetOrderWithDetailsAsync(string orderId)
        //{
        //    return await _context.dhang.Include(o => o.Chitietdonhangs)
        //            .ThenInclude(od => od.IdmonanNavigation)
        //            .Include(o => o.Chitietdonhangs)
        //                .ThenInclude(od => od.Idmonan2Navigation)
        //            .Include(o => o.Chitietdonhangs)
        //                .ThenInclude(od => od.IdsizeNavigation)
        //            .Include(o => o.Chitietdonhangs)
        //                .ThenInclude(od => od.IddebanhNavigation)
        //            .Include(o => o.Chitietdonhangs)
        //                .ThenInclude(od => od.Chitiettoppings)
        //                    .ThenInclude(ct => ct.IdtoppingNavigation)
        //                    .FirstOrDefaultAsync(o => o.Iddonhang == orderId);
        //}

        public async Task<Donhang?> GetOrderWithDetailsAsync(string orderId)
        {
            return await _context.dhang.Include(o => o.IdchinhanhNavigation)
                .Include(o => o.Chitietdonhangs)
                    .ThenInclude(od => od.IdmonanNavigation)
                    .Include(o => o.Chitietdonhangs)
                        .ThenInclude(od => od.Idmonan2Navigation)
                    .Include(o => o.Chitietdonhangs)
                        .ThenInclude(od => od.IdsizeNavigation)
                    .Include(o => o.Chitietdonhangs)
                        .ThenInclude(od => od.IddebanhNavigation)
                    .Include(o => o.Chitietdonhangs)
                        .ThenInclude(od => od.Chitiettoppings)
                            .ThenInclude(ct => ct.IdtoppingNavigation)
                            .FirstOrDefaultAsync(o => o.Iddonhang == orderId);
        }
        public async Task<List<Donhang>> GetAllOrdersAsync()
        {
            return await _context.dhang
                .OrderByDescending(d => d.Ngaydat)
                .ToListAsync();
        }
        public async Task<Donhang?> FindOrderAsync(string orderId)
        {
            return await _context.dhang.FindAsync(orderId);
        }
        public async Task<Chitietdonhang?> FindDetailAsync(string detailsId)
        {
            return await _context.ctdh.FindAsync(detailsId);
        }
        public async Task<string?> FindDeBanhAsync(string tendebanh)
        {
            var Iddebanh = await _context.debanh
            .Where(d => d.Tendebanh == tendebanh)
            .Select(d => d.Iddebanh)
            .FirstOrDefaultAsync();
            if (Iddebanh == null)
            {
                return null;
            }
            return Iddebanh;
        }
        public async Task<string?> FindIdSizeAsync(string tenSize)
        {
            var idSize = await _context.Sizes
            .Where(d => d.Tensize == tenSize)
            .Select(d => d.Idsize)
            .FirstOrDefaultAsync();
            if (idSize == null)
            {
                return null;
            }
            return idSize;
        }
        public async Task UpdateOrderAsync(Donhang order)
        {
            try
            {
                _context.dhang.Update(order);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log error nếu cần
                throw new Exception($"Không thể cập nhật đơn hàng {order.Iddonhang}: {ex.Message}");
            }
        }
        public async Task<List<Donhang>> GetOrdersByUserIdAsync(string userId)
        {
            return await _context.dhang
                .Where(o => o.Idngdung == userId)
                .Include(o => o.Chitietdonhangs)
                .OrderByDescending(o => o.Ngaydat)
                .ToListAsync();
        }
        public async Task<List<Donhang>> GetOrdersByStatusAsync(TrangThai? status)
        {
            return await _context.dhang
                .Where(d => d.Trangthai == status)
                .OrderByDescending(d => d.Ngaydat)
                .ToListAsync();
        }
        public async Task<bool> UpdateOrderStatusAsync(string id, TrangThai newStatus)
        {
            var order = await _context.dhang.FindAsync(id);
            if (order == null) return false;

            if (order.Trangthai == TrangThai.Cancelled)
                return false;

            if ((order.Trangthai == TrangThai.Delivering || order.Trangthai == TrangThai.Completed)
                && newStatus == TrangThai.Cancelled)
                return false;
            if (order.Trangthai != TrangThai.Completed && newStatus == TrangThai.Completed)
            {
                await _productRepository.CapNhatSoLuongBanVaGhepAsync(order.Iddonhang);
            }
            order.Trangthai = newStatus;
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> CancelOrderAsync(string orderId)
        {
            var order = await _context.dhang.FindAsync(orderId);

            if (order == null ||
                (order.Trangthai != TrangThai.Pending && order.Trangthai != TrangThai.Paid))
                return false;

            order.Trangthai = TrangThai.Cancelled;
            _context.dhang.Update(order);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
