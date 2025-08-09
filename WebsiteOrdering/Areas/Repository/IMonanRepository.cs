using WebsiteOrdering.Areas.ViewModelAdmin;
using WebsiteOrdering.Models;

namespace WebsiteOrdering.Areas.Repository
{
    public interface IMonanRepository
    {
        Task<List<Monan>> GetAllAsync();
        Task<Monan?> GetByIdAsync(string id);
        Task<bool> ExistsAsync(string id);
        Task<string> CreateAsync(MonanFormViewModel viewModel, IFormFile? anhMoi);
        Task<bool> UpdateAsync(string id, MonanFormViewModel viewModel, IFormFile? anhMoi);
        Task<List<ProductSearchResult>> SearchProductsAsync(string searchTerm, int maxResults = 10);
    }
}
