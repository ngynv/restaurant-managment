using System.ComponentModel.DataAnnotations;

namespace WebsiteOrdering.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }
    }
}
