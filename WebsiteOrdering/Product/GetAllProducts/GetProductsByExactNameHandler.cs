using MediatR;
using WebsiteOrdering.Models;
using WebsiteOrdering.Repositories;

namespace WebsiteOrdering.Product.GetAllProducts
{
    public class GetProductsByExactNameHandler : IRequestHandler<GetProductsByExactNameQuery, IEnumerable<Monan>>
    {
        private readonly IProductRepository _productRepository;

        public GetProductsByExactNameHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<IEnumerable<Monan>> Handle(GetProductsByExactNameQuery request, CancellationToken cancellationToken)
        {
            return await _productRepository.GetProductsByExactNameAsync(request.ProductName);
        }
    }
}
