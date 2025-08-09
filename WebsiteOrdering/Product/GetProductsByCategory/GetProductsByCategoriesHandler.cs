using MediatR;
using Microsoft.EntityFrameworkCore;
using WebsiteOrdering.Models;

namespace WebsiteOrdering.Product.GetAllCategoryById
{
    public class GetProductsByCategoriesHandler :IRequestHandler<GetProductsByCategoiesQuery,List<Monan>>
    {
        private readonly AppDbContext _appDbContext;

        public GetProductsByCategoriesHandler(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<List<Monan>> Handle(GetProductsByCategoiesQuery request, CancellationToken cancellationToken)
        {
            var products = await _appDbContext.SanPhams
                .Where(p => request.CategoryId.Contains(p.Idloaimonan) && p.Trangthaiman == "Còn")
               // .Where(p => p.Idloaimonan == request.CategoryId && p.Trangthaiman == "Còn" )
                .Select(p => new Monan
                {
                    Idmonan = p.Idmonan,
                  
                    Tenmonan = p.Tenmonan,
                    Mota = p.Mota,
                    Giamonan = p.Giamonan,
                    Trangthaiman = p.Trangthaiman,
                    Anhmonan = p.Anhmonan,
                    Idloaimonan = p.Idloaimonan
                })
                .ToListAsync(cancellationToken);
            
            return products;
        }
    }
}
