using WebsiteOrdering.Models;

namespace WebsiteOrdering.Repositories
{
    public interface ILocationRepository
    {
        Task<IEnumerable<Chinhanh>> GetAllAsync();
        Task<Chinhanh?> GetByIdAsync(string id);
        Task<Chinhanh> CreateAsync(Chinhanh location);
        Task<Chinhanh> UpdateAsync(Chinhanh location);
        Task<bool> DeleteAsync(string id);
        Task<bool> ExistsAsync(string id);
        Task<IEnumerable<Chinhanh>> GetByAreaAsync(decimal minLat, decimal maxLat, decimal minLng, decimal maxLng);
        Task<IEnumerable<Chinhanh>> SearchByNameAsync(string name);
    }
}
