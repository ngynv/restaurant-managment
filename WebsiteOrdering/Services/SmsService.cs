
using System.Web;
using Microsoft.Extensions.Options;
using WebsiteOrdering.ViewModels;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
namespace WebsiteOrdering.Services
{
    public class SmsService : ISmsService
    {
        private readonly SmsSettings _settings;
        private readonly HttpClient _httpClient;
        public SmsService(IOptions<SmsSettings> settings)
        {
            _settings = settings.Value;
            _httpClient = new HttpClient();
        }
        public async Task<bool> SendSmsAsync(string phoneNumber, string message)
        {
            try
            {
                if (phoneNumber.StartsWith("0"))
                    phoneNumber = "84" + phoneNumber.Substring(1); // Chuyển về định dạng quốc tế

                var payload = new
                {
                    to = new[] { phoneNumber },
                    content = message,
                    type = 2, // 2 = SMS OTP
                    sender = _settings.Sender
                };

                var jsonPayload = JsonConvert.SerializeObject(payload);
                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.speedsms.vn/index.php/sms/send");

                request.Headers.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_settings.AccessToken}:x")));

                request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                return response.IsSuccessStatusCode && content.Contains("\"status\":\"success\"");
            }
            catch
            {
                return false;
            }
        }
    }
}
