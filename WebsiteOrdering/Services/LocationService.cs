using System.Net.Http;
using System.Text;
using System.Text.Json;
using WebsiteOrdering.Models;
using WebsiteOrdering.Repositories;
using WebsiteOrdering.ViewModels;

namespace WebsiteOrdering.Services
{
    public class LocationService : ILocationService
    {
        private readonly ILocationRepository _locationRepository;

        public LocationService(ILocationRepository locationRepository)
        {
            _locationRepository = locationRepository;
        }

        public async Task<IEnumerable<Chinhanh>> GetAllLocationsAsync()
        {
            var locations = await _locationRepository.GetAllAsync();
            return locations.Select(MapToViewModel).ToList();
        }

        public async Task<Chinhanh?> GetLocationByIdAsync(string id)
        {
            var location = await _locationRepository.GetByIdAsync(id);
            return location != null ? MapToViewModel(location) : null;
        }

        public async Task<Chinhanh> CreateLocationAsync(Chinhanh createDto)
        {
            var location = new Chinhanh
            {
                Tencnhanh = createDto.Tencnhanh,
                Diachicn = createDto.Diachicn,
                Latitude = createDto.Latitude,
                Longitude = createDto.Longitude
            };

            var createdLocation = await _locationRepository.CreateAsync(location);
            return MapToViewModel(createdLocation);
        }

        public async Task<Chinhanh> UpdateLocationAsync(string id, Chinhanh updateDto)
        {
            var existingLocation = await _locationRepository.GetByIdAsync(id);
            if (existingLocation == null)
                throw new ArgumentException($"Location with ID {id} not found.");

            existingLocation.Tencnhanh = updateDto.Tencnhanh;
            existingLocation.Diachicn = updateDto.Diachicn;
            existingLocation.Latitude = updateDto.Latitude;
            existingLocation.Longitude = updateDto.Longitude;

            var updatedLocation = await _locationRepository.UpdateAsync(existingLocation);
            return MapToViewModel(updatedLocation);
        }

        public async Task<bool> DeleteLocationAsync(string id)
        {
            return await _locationRepository.DeleteAsync(id);
        }

        public async Task<bool> LocationExistsAsync(string id)
        {
            return await _locationRepository.ExistsAsync(id);
        }

        public async Task<IEnumerable<Chinhanh>> GetLocationsByAreaAsync(decimal minLat, decimal maxLat, decimal minLng, decimal maxLng)
        {
            var locations = await _locationRepository.GetByAreaAsync(minLat, maxLat, minLng, maxLng);
            return locations.Select(MapToViewModel).ToList();
        }

        public async Task<IEnumerable<Chinhanh>> SearchLocationsByNameAsync(string name)
        {
            var locations = await _locationRepository.SearchByNameAsync(name);
            return locations.Select(MapToViewModel).ToList();
        }

        private static Chinhanh MapToViewModel(Chinhanh location)
        {
            return new Chinhanh
            {
                Idchinhanh = location.Idchinhanh,
                Tencnhanh = location.Tencnhanh,
                Diachicn = location.Diachicn,
                Latitude = location.Latitude,
                Longitude = location.Longitude
            };
        }
        // Enhanced distance calculation (Haversine formula)
        public double GetDistance(double lat1, double lng1, double lat2, double lng2)
        {
            const double R = 6371; // Earth's radius in kilometers
            var dLat = ToRadians(lat2 - lat1);
            var dLng = ToRadians(lng2 - lng1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLng / 2) * Math.Sin(dLng / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }
        private double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }
        public async Task<Chinhanh?> FindNearestBranchAsync(double lat, double lng)
        {
            var branches = await _locationRepository.GetAllAsync();

            Chinhanh? nearest = null;
            double minDist = double.MaxValue;

            foreach (var b in branches)
            {
                var dist = GetDistance(lat, lng, (double)b.Latitude, (double)b.Longitude);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = b;
                }
            }

            return nearest;
        }
    }
}
