using System.Diagnostics;
using System.Text.Json;

namespace WebsiteOrdering.Services
{
    public class GeoService : IGeoService
    {
        private readonly HttpClient _http;
        public GeoService(HttpClient http)
        {
            http.DefaultRequestHeaders.UserAgent.ParseAdd("WebsiteOrdering/1.0 (6bagshospital.com)");
            _http = http;
        }
        public async Task<string> ReverseGeocodeAsync(double lat, double lng)
        {
        // Dùng OpenStreetMap Nominatim
            var url = $"https://nominatim.openstreetmap.org/reverse?format=jsonv2&lat={lat}&lon={lng}";

            var response = await _http.GetAsync(url);
            Debug.WriteLine(response);
            if (!response.IsSuccessStatusCode)
                return "Không xác định được địa chỉ";

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("display_name").GetString() ?? "Không rõ địa chỉ";
        }
    }
}
