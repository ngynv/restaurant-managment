using MediatR;
using WebsiteOrdering.Models;

namespace WebsiteOrdering.Product.GetAllProducts
{
    public class GetAllProductQuery :IRequest<List<Monan>>
    {
    }
  
}
