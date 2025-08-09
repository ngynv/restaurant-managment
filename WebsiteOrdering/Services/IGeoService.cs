namespace WebsiteOrdering.Services
{
    public interface IGeoService
    {
        Task<string> ReverseGeocodeAsync(double lat, double lng);

    }
}
