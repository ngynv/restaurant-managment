using MediatR;
using WebsiteOrdering.Models;

namespace WebsiteOrdering.Product.GetAllCategoryById
{
    public class GetProductsByCategoiesQuery :IRequest<List<Monan>>
    {
        public string CategoryId { get;  }
        public GetProductsByCategoiesQuery(string categoryId)
        {
            CategoryId = categoryId;
        }
    }
}
