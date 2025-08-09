using MediatR;
using Microsoft.EntityFrameworkCore;
using WebsiteOrdering.Models;

namespace WebsiteOrdering.Product.GetAllProducts
{
    public class GetAllProductsHandler :IRequestHandler<GetAllProductQuery,List<Monan>>
    {
        private readonly AppDbContext _appDbContext;
        public GetAllProductsHandler(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<List<Monan>> Handle(GetAllProductQuery query, CancellationToken cancellationToken)
        {
            
            return await _appDbContext.SanPhams
                .Where(p => p.Trangthaiman == "Còn" )
                .Select(p=>new Monan
                {
                    Idmonan = p.Idmonan,
                    Idloaimonan = p.Idloaimonan,
                    Tenmonan = p.Tenmonan,
                    Mota = p.Mota,
                    Giamonan = p.Giamonan,
                    Anhmonan = p.Anhmonan
                })
                .ToListAsync(cancellationToken);

        }
    }
}
