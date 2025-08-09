using WebsiteOrdering.Models;

namespace WebsiteOrdering.Services
{
    public interface ILocationService
    {
        Task<IEnumerable<Chinhanh>> GetAllLocationsAsync();
        Task<Chinhanh?> GetLocationByIdAsync(string id);
        Task<Chinhanh> CreateLocationAsync(Chinhanh createDto);
        Task<Chinhanh> UpdateLocationAsync(string id, Chinhanh updateDto);
        Task<bool> DeleteLocationAsync(string id);
        Task<bool> LocationExistsAsync(string id);
        Task<IEnumerable<Chinhanh>> GetLocationsByAreaAsync(decimal minLat, decimal maxLat, decimal minLng, decimal maxLng);
        Task<IEnumerable<Chinhanh>> SearchLocationsByNameAsync(string name);
        Task<Chinhanh?> FindNearestBranchAsync(double lat, double lng);
        double GetDistance(double lat1, double lng1, double lat2, double lng2);

    }
}
