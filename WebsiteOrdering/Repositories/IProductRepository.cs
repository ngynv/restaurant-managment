using WebsiteOrdering.Models;

namespace WebsiteOrdering.Repositories
{
    public interface IProductRepository
    {
        Task<IEnumerable<Monan>> GetProductsByExactNameAsync(string productName);
        Task CapNhatSoLuongBanVaGhepAsync(string idDonhang);
        Task CapNhatSoLanDuocGhepAsync(string idMonan2, int soLuong);
    }
}
