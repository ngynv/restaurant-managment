using System.Security.Claims;
using MailKit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebsiteOrdering.Models;
using WebsiteOrdering.Models.Results;
using WebsiteOrdering.Services;
using WebsiteOrdering.ViewModels;

namespace WebsiteOrdering.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailService _mailService;
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccountRepository(UserManager<ApplicationUser> userManager,
                                 SignInManager<ApplicationUser> signInManager,
                                 IEmailService mailService,
                                 IWebHostEnvironment env,
                                 IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _mailService = mailService;
            _env = env;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IdentityResult> RegisterAsync(RegisterViewModel model, string confirmEmailUrl)
        {
            if (model?.Email == null || model?.Password == null)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "InvalidInput",
                    Description = "Dữ liệu không hợp lệ"
                });
            }

            var normalizedEmail = model.Email.Trim().ToLowerInvariant();
            var normalizedName = model.FullName.Trim().ToLowerInvariant();
            if (!normalizedEmail.EndsWith("@gmail.com"))
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "InvalidEmailDomain",
                    Description = "Chỉ hỗ trợ đăng ký bằng email @gmail.com"
                });
            }
            var existingUser = await _userManager.FindByEmailAsync(normalizedEmail);
            if (existingUser != null)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "EmailAlreadyExists",
                    Description = "Email đã được sử dụng"
                });
            }

            var user = new ApplicationUser
            {
                FullName= normalizedName,
                UserName = normalizedEmail,
                Email = normalizedEmail,
                EmailConfirmed = false,
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            var addRoleResult = await _userManager.AddToRoleAsync(user, "Customer");
            if (!addRoleResult.Succeeded)
            {
                return addRoleResult;
            }
            if (result.Succeeded)
            {
                try
                {
                    await SendEmailConfirmationAsync(user, confirmEmailUrl);
                }
                catch (Exception ex)
                {

                }
            }
            return result;
        }
        private async Task SendEmailConfirmationAsync(ApplicationUser user, string confirmEmailUrl)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = System.Net.WebUtility.UrlEncode(token);
            var callbackUrl = $"{confirmEmailUrl}?userId={user.Id}&token={encodedToken}";

            var templatePath = Path.Combine(_env.ContentRootPath, "Templates", "EmailConfirmationTemplate.html");

            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException("Email template not found");
            }

            var body = await File.ReadAllTextAsync(templatePath);
            body = body.Replace("{{CONFIRM_LINK}}", callbackUrl);

            await _mailService.SendEmailAsync(user.Email, "Xác nhận Email", body);
        }
        public async Task<SignInResult> LoginAsync(LoginViewModel model)
        {
            //  var normalizedEmail = model.Email.Trim().ToLowerInvariant();

            return await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }
        public async Task<bool> ConfirmEmailAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var result = await _userManager.ConfirmEmailAsync(user, token);
            return result.Succeeded;
        }
        public async Task<ApplicationUser?> GetUserByPhoneNumberAsync(string phoneNumber)
        {
            return await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
        }
        public async Task SignInUserAsync(ApplicationUser user, bool isPersistent = false)
        {
            await _signInManager.SignInAsync(user, isPersistent);
        }

        public async Task<(bool Success, ApplicationUser? User, IEnumerable<string>? Errors)> CreateUserWithPhone(string phoneNumber)
        {
            var user = await GetUserByPhoneNumberAsync(phoneNumber);
            if (user != null)
                return (true, user, null);

            user = new ApplicationUser
            {
                UserName = phoneNumber,
                PhoneNumber = phoneNumber,
                PhoneNumberConfirmed = false
            };
            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
                return (false, null, result.Errors.Select(e => e.Description));
            var addRoleResult = await _userManager.AddToRoleAsync(user, "Customer");
            if (!addRoleResult.Succeeded)
            {
                return (false, null, addRoleResult.Errors.Select(e => e.Description));
            }
            return (true, user, null);
        }
        public async Task<IdentityResult> UpdateUserAsync(ApplicationUser user)
        {
            return await _userManager.UpdateAsync(user);
        }
        public async Task<ApplicationUser?> GetCurrentUserAsync(ClaimsPrincipal user)
        {
            return await _userManager.GetUserAsync(user);
        }
        public AuthenticationProperties GooglelLoginAsync(string provider, string redirectUrl)
        {
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return properties;
        }
        public async Task<IdentityResult> GoogleLoginCallbackAsync()
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Description = "Thông tin đăng nhập bên ngoài không có sẵn."
                });
            }

            var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: true);
            if (signInResult.Succeeded)
            {
                return IdentityResult.Success;
            }

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
            {
                return IdentityResult.Failed(new IdentityError { Description = "Không tìm thấy email từ thông tin đăng nhập." });
            }

            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                return await LinkGoogleLoginToExistingUser(existingUser, info);
            }
            else
            {
                return await CreateAndSignInGoogleUser(info, email);
            }
        }

        public async Task<IdentityResult> LinkGoogleLoginToExistingUser(ApplicationUser user, ExternalLoginInfo info)
        {
            var logins = await _userManager.GetLoginsAsync(user);
            if (!logins.Any(x => x.LoginProvider == info.LoginProvider))
            {
                var addLoginResult = await _userManager.AddLoginAsync(user, info);
                if (!addLoginResult.Succeeded)
                    return addLoginResult;
            }

            if (!user.EmailConfirmed)
            {
                user.EmailConfirmed = true;
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                    return updateResult;
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> CreateAndSignInGoogleUser(ExternalLoginInfo info, string email)
        {
            var firstName = info.Principal.FindFirstValue(ClaimTypes.GivenName) ?? "";
            var lastName = info.Principal.FindFirstValue(ClaimTypes.Surname) ?? "";
            var fullName = (firstName + " " + lastName).Trim();
            var newUser = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = fullName

            };
            var createResult = await _userManager.CreateAsync(newUser);
            if (!createResult.Succeeded)
            {
                return createResult;
            }
            var addRoleResult = await _userManager.AddToRoleAsync(newUser, "Customer");
            if (!addRoleResult.Succeeded)
            {
                return addRoleResult;
            }
            var addLoginResult = await _userManager.AddLoginAsync(newUser, info);
            if (!addLoginResult.Succeeded)
            {
                return addLoginResult;
            }

            await _signInManager.SignInAsync(newUser, isPersistent: true);
            return IdentityResult.Success;
        }

        public async Task FillUserInfoIfAuthenticated(UserCheckoutInfoViewModel model, ClaimsPrincipal userPrincipal)
        {
            if (userPrincipal.Identity != null && userPrincipal.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(userPrincipal);
                if (user != null)
                {
                    if (!string.IsNullOrWhiteSpace(user.FullName))
                    {
                        model.FullName = user.FullName;
                    }

                    if (!string.IsNullOrWhiteSpace(user.PhoneNumber))
                    {
                        model.PhoneNumber = user.PhoneNumber;
                    }
                }
            }
        }
        public async Task<ApplicationUser?> GetUserByEmailAsync(string email)
        {
            // return await _userManager.Users.FirstOrDefaultAsync(u => u.Email == email);
            return await _userManager.Users
                              .Include(u => u.IdchinhanhNavigation)
                              .FirstOrDefaultAsync(u => u.Email == email);
        }
        public async Task<ApplicationUser?> GetUserByIdAsync(string id)
        {
            return await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);
        }
        public async Task<IList<string>> GetUserRolesAsync(ApplicationUser user)
        {
            return await _userManager.GetRolesAsync(user);
        }
        public async Task<ForgotPasswordResult> SendForgotPasswordEmailAsync(string email, string resetPasswordUrlTemplate)
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();
            var user = await _userManager.FindByEmailAsync(normalizedEmail);

            if (user == null)
            {
                return new ForgotPasswordResult
                {
                    Success = false,
                    ErrorCode = "UserNotFound",
                    Message = "Không tìm thấy tài khoản với email đã nhập."
                };
            }

            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                return new ForgotPasswordResult
                {
                    Success = false,
                    ErrorCode = "EmailNotConfirmed",
                    Message = "Email chưa được xác nhận. Vui lòng xác nhận trước khi đặt lại mật khẩu."
                };
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = System.Net.WebUtility.UrlEncode(token);
            var callbackUrl = string.Format(resetPasswordUrlTemplate, user.Id, encodedToken);

            var templatePath = Path.Combine(_env.ContentRootPath, "Templates", "ResetPasswordTemplate.html");
            if (!File.Exists(templatePath))
            {
                return new ForgotPasswordResult
                {
                    Success = false,
                    ErrorCode = "TemplateNotFound",
                    Message = "Không tìm thấy mẫu email đặt lại mật khẩu."
                };
            }
            var body = await File.ReadAllTextAsync(templatePath);
            body = body.Replace("{{RESET_LINK}}", callbackUrl);

            await _mailService.SendEmailAsync(user.Email, "Đặt lại mật khẩu", body);
            return new ForgotPasswordResult
            {
                Success = true,
                Message = "Liên kết đặt lại mật khẩu đã được gửi. Vui lòng kiểm tra email."
            };
        }

        public async Task<IdentityResult> ResetPasswordAsync(ResetPasswordViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null) return IdentityResult.Failed(new IdentityError
            {
                Code = "UserNotFound",
                Description = "Người dùng không tồn tại."
            });

            return await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
        }
        public async Task<bool> SignInStaffWithClaimsAsync(ApplicationUser user)
        {
            if (string.IsNullOrEmpty(user.Idchinhanh))
                return false;

            await _signInManager.SignOutAsync(); // Đảm bảo đăng nhập mới

            var principal = await _signInManager.CreateUserPrincipalAsync(user);
            var identity = (ClaimsIdentity)principal.Identity!;

            // Thêm claim IdChiNhanh
            identity.AddClaim(new Claim("ChiNhanhId", user.Idchinhanh));

            // Thêm claim Role (lấy từ hệ thống Identity)
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, role));
            }

            await _httpContextAccessor.HttpContext!.SignInAsync(
                IdentityConstants.ApplicationScheme,
                new ClaimsPrincipal(identity));

            return true;
        }

    }
}
