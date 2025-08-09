namespace WebsiteOrdering.Models.Results
{
    public class ForgotPasswordResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";      
        public string? ErrorCode { get; set; }           
    }
}
