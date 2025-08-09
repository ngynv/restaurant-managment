using Microsoft.EntityFrameworkCore;
using WebsiteOrdering.Areas.ViewModelAdmin;
using WebsiteOrdering.Models;
using WebsiteOrdering.Services;

namespace WebsiteOrdering.Areas.Repository
{
    public class MonanRepository : IMonanRepository
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly LuceneProductIndexer _luceneIndexer;
        public MonanRepository(AppDbContext context, IWebHostEnvironment env, LuceneProductIndexer luceneProductIndexer)
        {
            _context = context;
            _env = env;
            _luceneIndexer = luceneProductIndexer;
        }

        public async Task<List<Monan>> GetAllAsync()
        {
            return await _context.SanPhams.Include(m => m.IdloaimonanNavigation).ToListAsync();
        }

        public async Task<Monan?> GetByIdAsync(string id)
        {
            return await _context.SanPhams.Include(m => m.IdloaimonanNavigation)
                                          .FirstOrDefaultAsync(x => x.Idmonan == id);
        }

        public async Task<bool> ExistsAsync(string id)
        {
            return await _context.SanPhams.AnyAsync(e => e.Idmonan == id);
        }
        private string GenerateId(int length = 5)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public async Task<string> CreateAsync(MonanFormViewModel viewModel, IFormFile? anhMoi)
        {
            var fileName = await SaveImageAsync(anhMoi);
            string newId;
            do
            {
                newId = GenerateId();
            } while (_context.SanPhams.Any(m => m.Idmonan == newId));
            var monan = new Monan
            {
                Idmonan = newId,
                Tenmonan = viewModel.Tenmonan,
                Idloaimonan = viewModel.Idloaimonan,
                Giamonan = viewModel.Giamonan,
                Mota = viewModel.Mota,
                Trangthaiman = viewModel.Trangthaiman,
                Anhmonan = fileName
            };

            _context.SanPhams.Add(monan);
            await _context.SaveChangesAsync();

            return monan.Idmonan;
        }

        public async Task<bool> UpdateAsync(string id, MonanFormViewModel viewModel, IFormFile? anhMoi)
        {
            var monan = await _context.SanPhams.FindAsync(id);
            if (monan == null)
                return false;

            monan.Tenmonan = viewModel.Tenmonan;
            monan.Idloaimonan = viewModel.Idloaimonan;
            monan.Giamonan = viewModel.Giamonan;
            monan.Mota = viewModel.Mota;
            monan.Trangthaiman = viewModel.Trangthaiman;

            if (anhMoi != null && anhMoi.Length > 0)
            {
                monan.Anhmonan = await SaveImageAsync(anhMoi);
            }

            _context.SanPhams.Update(monan);
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<string> SaveImageAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return "default.jpg";

            var originalName = Path.GetFileNameWithoutExtension(file.FileName);
            var ext = Path.GetExtension(file.FileName).ToLower();
            var safeName = originalName.Replace(" ", "_"); // bỏ khoảng trắng
            var fileName = $"{safeName}_{Guid.NewGuid().ToString().Substring(0, 8)}{ext}";
            var path = Path.Combine(_env.WebRootPath, "css", "img", fileName);

            using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);

            return fileName;
        }
        public async Task<List<ProductSearchResult>> SearchProductsAsync(string searchTerm, int maxResults = 10)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                return new List<ProductSearchResult>();
            }

            var searchResults = _luceneIndexer.SearchWithScore(searchTerm, maxResults);
            var resultIds = searchResults.Select(r => r.Id).ToList();

            var products = await _context.SanPhams
                .Where(p => resultIds.Contains(p.Idmonan))
                .Select(p => new ProductSearchResult
                {
                    Id = p.Idmonan,
                    Name = p.Tenmonan
                })
                .ToListAsync();

            return products;
        }
    }
}
