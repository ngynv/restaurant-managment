using Microsoft.EntityFrameworkCore;
using WebsiteOrdering.Models;

namespace WebsiteOrdering.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;

        public ProductRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Monan>> GetProductsByExactNameAsync(string productName)
        {
            // Case-insensitive exact match
            var exactMatch = await _context.SanPhams
                .Where(p => p.Tenmonan.ToLower() == productName.ToLower())
                .ToListAsync();
            if (exactMatch.Any())
            {
                return exactMatch;
            }
            var trimmedMatch = await _context.SanPhams
                .Where(p => p.Tenmonan.Trim().ToLower() == productName.Trim().ToLower())
                .ToListAsync();
            return trimmedMatch;
        }
        public async Task CapNhatSoLuongBanVaGhepAsync(string idDonhang)
        {
            var chiTietDonHangs = await _context.ctdh
                .Where(x => x.Iddonhang == idDonhang)
                .ToListAsync();
            foreach (var item in chiTietDonHangs)
            {
                // Tăng số lượng bán cho món chính
                var mon = await _context.SanPhams.FindAsync(item.Idmonan);
                if (mon != null)
                {
                    mon.SoLuongBan += item.Soluong;
                }

                // Nếu là pizza ghép, tăng số lượng bán cho nửa ghép và cập nhật thống kê
                if (!string.IsNullOrEmpty(item.Idmonan2))
                {
                    var mon2 = await _context.SanPhams.FindAsync(item.Idmonan2);
                    if (mon2 != null)
                    {
                        mon2.SoLuongBan += item.Soluong;
                    }

                    // Gọi hàm cập nhật số lần được ghép
                    await CapNhatSoLanDuocGhepAsync(item.Idmonan2, item.Soluong);
                }
            }

            await _context.SaveChangesAsync();
        }
        public async Task CapNhatSoLanDuocGhepAsync(string idMonan2, int soLuong)
        {
            var stat = await _context.MonAnGhepStats.FirstOrDefaultAsync(s => s.Idmonan == idMonan2);

            if (stat == null)
            {
                stat = new MonAnGhepStats
                {
                    Idmonan = idMonan2,
                    SoLanDuocGhep = soLuong
                };
                _context.MonAnGhepStats.Add(stat);
            }
            else
            {
                stat.SoLanDuocGhep += soLuong;
                _context.MonAnGhepStats.Update(stat);
            }
        }
    }
}
