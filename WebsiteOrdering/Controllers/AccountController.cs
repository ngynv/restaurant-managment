using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebsiteOrdering.Models;
using WebsiteOrdering.Repositories;
using WebsiteOrdering.Services;
using WebsiteOrdering.ViewModels;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;


namespace WebsiteOrdering.Controllers
{
    [Route("Account")]
    public class AccountController : Controller
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ISmsService _smsService;
        private readonly IOtpService _otpService;


        public AccountController(IAccountRepository accountRepository, ISmsService smsService,
            IOtpService otpService)
        {
            _accountRepository = accountRepository;
            _smsService = smsService;
            _otpService = otpService;
        }
        [Route("Register")]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var confirmEmailUrl = Url.Action("ConfirmEmail", "Account", null, protocol: Request.Scheme);

            var result = await _accountRepository.RegisterAsync(model, confirmEmailUrl);
            if (result.Succeeded)
            {
                ViewBag.EmailConfirmationMessage = "Đăng ký thành công! Vui lòng kiểm tra email để xác nhận tài khoản.";
                return View();
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }
        [Route("Login")]
        public IActionResult Login(string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _accountRepository.LoginAsync(model);
            if (result.Succeeded)
            {
                // Login successful - redirect to return URL or default location
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return Redirect("/");
            }

            ModelState.AddModelError("", "Invalid login attempt.");
            return View(model);
        }
        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {
            await _accountRepository.LogoutAsync();
            return Redirect("/");
        }

        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var result = await _accountRepository.ConfirmEmailAsync(userId, token);

            ViewBag.ConfirmEmailMessage = result
                ? "✅ Xác nhận email thành công. Bạn có thể quay lại trang đăng nhập."
                : "❌ Xác nhận thất bại hoặc liên kết không hợp lệ.";

            return View();
        }
        [HttpGet("RegisterPhoneNumber")]
        public ActionResult RegisterPhoneNumber()
        {
            return View();
        }
        [HttpPost("SendOtp")]
        public async Task<IActionResult> SendOtp(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                ModelState.AddModelError("PhoneNumber", "Số điện thoại không được để trống.");
                return View("RegisterPhoneNumber");
            }
            var user = await _accountRepository.GetUserByPhoneNumberAsync(phone);
            if (user != null && user.PhoneNumberConfirmed)
            {
                await _accountRepository.SignInUserAsync(user);
                return RedirectToAction("Index", "Home");
            }

            var (success, createdUser, errors) = await _accountRepository.CreateUserWithPhone(phone);
            if (!success || createdUser == null)
            {
                foreach (var error in errors!) ModelState.AddModelError("", error);
                return View("RegisterPhoneNumber", phone);
            }

            var otp = _otpService.GenerateOtp(phone);
            var message = $"Ma OTP cua ban la: {otp}";
            var sendSuccess = await _smsService.SendSmsAsync(phone, message);

            if (!sendSuccess)
            {
                ModelState.AddModelError("", "Không gửi được SMS.");
                return View("RegisterPhoneNumber", phone);
            }

            return View("VerifyOtp", new VerifyOtpViewModel
            {
                PhoneNumber = phone
            });
        }

        [HttpPost("VerifyOtp")]
        public async Task<IActionResult> VerifyOtp(VerifyOtpViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (!_otpService.VerifyOtp(model.PhoneNumber, model.OtpInput))
            {
                ModelState.AddModelError("", "Mã OTP không đúng hoặc đã hết hạn.");
                return View(model);
            }

            var user = await _accountRepository.GetUserByPhoneNumberAsync(model.PhoneNumber);
            if (user == null)
            {
                ModelState.AddModelError("", "Không tìm thấy người dùng.");
                return View(model);
            }

            user.PhoneNumberConfirmed = true;
            await _accountRepository.UpdateUserAsync(user);
            await _accountRepository.SignInUserAsync(user);

            return RedirectToAction("Index", "Home");
        }
        // Đăng nhập bằng Google
        [Route("login-google")]
        public IActionResult LoginWithGoogle(string returnUrl = null)
        {
            // Store returnUrl in session to retrieve after Google callback
            if (!string.IsNullOrEmpty(returnUrl))
            {
                HttpContext.Session.SetString("GoogleLoginReturnUrl", returnUrl);
            }
            var redirectUrl = Url.Action("GoogleResponse", "Account");
            var properties = _accountRepository.GooglelLoginAsync(GoogleDefaults.AuthenticationScheme, redirectUrl);
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }
        [Route("google-response")]
        public async Task<IActionResult> GoogleResponse()
        {
            var result = await _accountRepository.GoogleLoginCallbackAsync();
            if (result.Succeeded)
            {
                // Retrieve returnUrl from session
                var returnUrl = HttpContext.Session.GetString("GoogleLoginReturnUrl");
                HttpContext.Session.Remove("GoogleLoginReturnUrl"); // Clean up

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index", "Home");
            }

            TempData["Error"] = "Đăng nhập bằng Google thất bại.";
            return RedirectToAction("Login");
        }
        [Authorize]
        [HttpGet]
        [Route("Profile")]
        public async Task<IActionResult> Profile()
        {
            var user = await _accountRepository.GetCurrentUserAsync(User);
            if (user == null) return NotFound();

            var model = new UpdateProfileViewModel
            {
                FullName = user.FullName,
                BirthDate = user.BirthDate,
                Gender = user.Gender,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email
            };

            return View(model);
        }

        [HttpPost]
        [Route("UpdateInfo")]
        public async Task<IActionResult> Profile(UpdateProfileViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var user = await _accountRepository.GetCurrentUserAsync(User);
                if (user == null) return NotFound();

                user.FullName = model.FullName;
                user.BirthDate = model.BirthDate;
                user.Gender = model.Gender;
                user.PhoneNumber = model.PhoneNumber;

                var result = await _accountRepository.UpdateUserAsync(user);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Cập nhật thông tin thành công.";
                    return RedirectToAction("Personal", "Personal"); // PRG pattern
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Đã xảy ra lỗi khi cập nhật thông tin.");
            }

            return View(model);
        }
        [HttpGet]
        [Route("ForgotPassword")]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [Route("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData["Message"] = "Vui lòng nhập email.";
                return View("Login");
            }
            var templateUrl = $"{Request.Scheme}://{Request.Host}/Account/ResetPassword?userId={{0}}&token={{1}}";
            var result = await _accountRepository.SendForgotPasswordEmailAsync(email, templateUrl);

            TempData["Message"] = result.Message;
            return View("Login");
        }
        [HttpGet]
        [Route("ResetPassword")]
        public IActionResult ResetPassword(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return BadRequest("Liên kết không hợp lệ");

            var model = new ResetPasswordViewModel { UserId = userId, Token = token };
            return View(model);
        }

        [HttpPost]
        [Route("ResetPassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _accountRepository.ResetPasswordAsync(model);
            if (result.Succeeded)
                return RedirectToAction("ResetPasswordConfirmation");

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        [HttpGet]
        [Route("ResetPasswordConfirmation")]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }
    }
}
