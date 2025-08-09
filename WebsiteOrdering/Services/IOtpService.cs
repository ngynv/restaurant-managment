namespace WebsiteOrdering.Services
{
    public interface IOtpService
    {
        string GenerateOtp(string phoneNumber);
        void SaveOtp(string phoneNumber, string otp);
        bool VerifyOtp(string phoneNumber, string otp);
    }
}
