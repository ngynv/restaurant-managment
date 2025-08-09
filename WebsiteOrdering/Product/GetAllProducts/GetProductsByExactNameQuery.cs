using MediatR;
using WebsiteOrdering.Models;

namespace WebsiteOrdering.Product.GetAllProducts
{
    public class GetProductsByExactNameQuery : IRequest<IEnumerable<Monan>>
    {
        public string ProductName { get; }

        public GetProductsByExactNameQuery(string productName)
        {
            ProductName = productName;
        }
    }
}
