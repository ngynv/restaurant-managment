using MediatR;
using Microsoft.EntityFrameworkCore;
using WebsiteOrdering.Models;

namespace WebsiteOrdering.Product.GetAllCategory
{
    public class GetAllCategoriesHandler :IRequestHandler<GetAllCategoriesQuery, List<Loaimonan>>
    {
        private readonly AppDbContext _context;
        public GetAllCategoriesHandler(AppDbContext context)
        {
            _context = context;
        }
        public async Task<List<Loaimonan>> Handle(GetAllCategoriesQuery query,CancellationToken cancellationToken)
        {
            return await _context.Category.ToListAsync(cancellationToken);
        }
    }
}
