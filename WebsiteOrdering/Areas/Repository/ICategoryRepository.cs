using WebsiteOrdering.Models;

namespace WebsiteOrdering.Areas.Repository
{
    public interface ICategoryRepository
    {
        Task<List<Loaimonan>> GetAllAsync();

    }
}
