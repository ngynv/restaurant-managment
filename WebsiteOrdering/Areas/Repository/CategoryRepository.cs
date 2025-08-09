using Microsoft.EntityFrameworkCore;
using WebsiteOrdering.Models;

namespace WebsiteOrdering.Areas.Repository
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly AppDbContext _context;

        public CategoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Loaimonan>> GetAllAsync()
        {
            return await _context.Category.ToListAsync();
        }
    }
}
