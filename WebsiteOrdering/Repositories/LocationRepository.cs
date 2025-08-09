using Microsoft.EntityFrameworkCore;
using WebsiteOrdering.Models;

namespace WebsiteOrdering.Repositories
{
    public class LocationRepository : ILocationRepository
    {
        private readonly AppDbContext _context;

        public LocationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Chinhanh>> GetAllAsync()
        {
            return await _context.chinhanh
                .OrderBy(l => l.Tencnhanh)
                .ToListAsync();
        }

        public async Task<Chinhanh?> GetByIdAsync(string id)
        {
            return await _context.chinhanh
                .FirstOrDefaultAsync(l => l.Idchinhanh == id);
        }

        public async Task<Chinhanh> CreateAsync(Chinhanh location)
        {
            _context.chinhanh.Add(location);
            await _context.SaveChangesAsync();
            return location;
        }

        public async Task<Chinhanh> UpdateAsync(Chinhanh location)
        {
            _context.chinhanh.Update(location);
            await _context.SaveChangesAsync();
            return location;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var location = await _context.chinhanh.FindAsync(id);
            if (location == null)
                return false;

            _context.chinhanh.Remove(location);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(string id)
        {
            return await _context.chinhanh.AnyAsync(l => l.Idchinhanh == id);
        }

        public async Task<IEnumerable<Chinhanh>> GetByAreaAsync(decimal minLat, decimal maxLat, decimal minLng, decimal maxLng)
        {
            return await _context.chinhanh
                .Where(l => l.Latitude >= minLat && l.Latitude <= maxLat &&
                           l.Longitude >= minLng && l.Longitude <= maxLng)
                .OrderBy(l => l.Tencnhanh)
                .ToListAsync();
        }

        public async Task<IEnumerable<Chinhanh>> SearchByNameAsync(string name)
        {
            return await _context.chinhanh
                .Where(l => l.Tencnhanh.Contains(name) || l.Diachicn.Contains(name))
                .OrderBy(l => l.Tencnhanh)
                .ToListAsync();
        }
    }
}
