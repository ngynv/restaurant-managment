using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using WebsiteOrdering.Models;
using WebsiteOrdering.Models.Results;
using WebsiteOrdering.ViewModels;

namespace WebsiteOrdering.Repositories
{
    public interface IAccountRepository
    {
        Task<IdentityResult> RegisterAsync(RegisterViewModel model, string confirmEmailUrl);
        Task<SignInResult> LoginAsync(LoginViewModel model);
        Task LogoutAsync();
        Task<bool> ConfirmEmailAsync(string userId, string token);
        Task<ApplicationUser?> GetUserByPhoneNumberAsync(string phoneNumber);
        Task<(bool Success, ApplicationUser? User, IEnumerable<string>? Errors)> CreateUserWithPhone(string phoneNumber);
        Task SignInUserAsync(ApplicationUser user, bool isPersistent = false);
        Task<IdentityResult> UpdateUserAsync(ApplicationUser user);
        Task<ApplicationUser?> GetCurrentUserAsync(ClaimsPrincipal user);

        Task<IdentityResult> GoogleLoginCallbackAsync();
        Task<IdentityResult> LinkGoogleLoginToExistingUser(ApplicationUser user, ExternalLoginInfo info);
        Task<IdentityResult> CreateAndSignInGoogleUser(ExternalLoginInfo info, string email);
        AuthenticationProperties GooglelLoginAsync(string provider, string redirectUrl);
        Task FillUserInfoIfAuthenticated(UserCheckoutInfoViewModel model, ClaimsPrincipal userPrincipal);
        Task<ApplicationUser?> GetUserByEmailAsync(string email);
        Task<ApplicationUser?> GetUserByIdAsync(string id);
        Task<IList<string>> GetUserRolesAsync(ApplicationUser user);
        Task<ForgotPasswordResult> SendForgotPasswordEmailAsync(string email, string resetPasswordUrlTemplate);
        Task<IdentityResult> ResetPasswordAsync(ResetPasswordViewModel model);
        Task<bool> SignInStaffWithClaimsAsync(ApplicationUser user);
    }
}
