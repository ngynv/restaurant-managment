using Microsoft.Extensions.Caching.Memory;

namespace WebsiteOrdering.Services
{
    public class OtpService : IOtpService
    {
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _otpExpiration = TimeSpan.FromMinutes(5);

        public OtpService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public string GenerateOtp(string phoneNumber)
        {
            var otp = new Random().Next(100000, 999999).ToString();
            SaveOtp(phoneNumber, otp);
            return otp;
        }

        public void SaveOtp(string phoneNumber, string otp)
        {
            _cache.Set(phoneNumber, otp, _otpExpiration);
        }

        public bool VerifyOtp(string phoneNumber, string otp)
        {
            return _cache.TryGetValue(phoneNumber, out string? expectedOtp) && expectedOtp == otp;
        }
    }
}
