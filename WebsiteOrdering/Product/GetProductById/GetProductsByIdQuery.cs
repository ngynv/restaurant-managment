using MediatR;
using WebsiteOrdering.Models;
using WebsiteOrdering.Product.GetAllProducts;

namespace WebsiteOrdering.Product.GetProductById
{
    public class GetProductsByIdQuery :IRequest<Monan>
    {
        public string Id { get;  }

   
        public GetProductsByIdQuery(string id)
        {
            Id = id;
        
        }
    }
}
