using AurumFinance.Exceptions;
using AurumFinance.Models;
using AurumFinance.Models.Api;
using AurumFinance.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AurumFinance.Controllers
{
    /// <summary>
    /// Every action here calls Aurum.Api's /api/auth/* endpoints through
    /// IAurumApiClient instead of touching a local database — this app owns
    /// no user credentials of its own. On success, a local cookie is issued
    /// (see AuthPrincipalFactory) carrying the backend's access + refresh
    /// tokens so later requests can call other protected backend endpoints
    /// on the user's behalf; CookieAuthEvents.ValidateAsync keeps that
    /// access token fresh automatically (see Program.cs).
    /// </summary>
    public class AuthController : Controller
    {
        private readonly IAurumApiClient _apiClient;

        public AuthController(IAurumApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        // GET: /Auth/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginModel());
        }

        // POST: /Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var result = await _apiClient.LoginAsync(model.Email, model.Password);
                await SignInAsync(result);
                return RedirectToAction("Welcome", "Home");
            }
            catch (AurumApiException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        // GET: /Auth/Register
        [HttpGet]
        public IActionResult Register()
        {
            var model = new RegisterModel();
            return View(model);
        }

        // POST: /Auth/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var result = await _apiClient.RegisterAsync(model.Email, model.Password, model.FullName);
                await SignInAsync(result);
                TempData["SuccessMessage"] = "Registration successful! We've sent a verification link to your email.";
                return RedirectToAction("Welcome", "Home");
            }
            catch (AurumApiException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        // GET: /Auth/Logout — a plain link (see Views/Shared/_Sidebar.cshtml), so this is GET, not POST.
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = await HttpContext.GetTokenAsync(CookieAuthenticationDefaults.AuthenticationScheme, "refresh_token");
            if (!string.IsNullOrEmpty(refreshToken))
            {
                try
                {
                    await _apiClient.LogoutAsync(refreshToken);
                }
                catch (AurumApiException)
                {
                    // Token was already invalid/expired server-side — the
                    // local cookie still needs clearing regardless.
                }
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // GET: /Auth/ForgotPassword
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordModel());
        }

        // POST: /Auth/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Always the same message whether or not the email is
            // registered — the backend itself never reveals account
            // existence either (see Aurum.Api's AuthService.ForgotPasswordAsync).
            await _apiClient.ForgotPasswordAsync(model.Email);
            TempData["SuccessMessage"] = "If that email is registered, a password reset link has been sent.";
            return RedirectToAction("Login");
        }

        // GET: /Auth/ResetPassword?token=...
        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            return View(new ResetPasswordModel { Token = token ?? string.Empty });
        }

        // POST: /Auth/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _apiClient.ResetPasswordAsync(model.Token, model.NewPassword);
                TempData["SuccessMessage"] = "Your password has been reset. Please sign in with your new password.";
                return RedirectToAction("Login");
            }
            catch (AurumApiException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        // GET: /Auth/VerifyEmail?token=...
        [HttpGet]
        public async Task<IActionResult> VerifyEmail(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                ViewBag.Success = false;
                ViewBag.Message = "This verification link is invalid.";
                return View();
            }

            try
            {
                await _apiClient.VerifyEmailAsync(token);
                ViewBag.Success = true;
                ViewBag.Message = "Your email has been verified.";
            }
            catch (AurumApiException ex)
            {
                ViewBag.Success = false;
                ViewBag.Message = ex.Message;
            }

            return View();
        }

        // GET: /Auth/ResendVerification
        [HttpGet]
        public IActionResult ResendVerification()
        {
            return View(new ResendVerificationModel());
        }

        // POST: /Auth/ResendVerification
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendVerification(ResendVerificationModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Same non-enumeration reasoning as ForgotPassword.
            await _apiClient.ResendVerificationAsync(model.Email);
            TempData["SuccessMessage"] = "If that email is registered and not yet verified, a new verification link has been sent.";
            return RedirectToAction("Login");
        }

        private async Task SignInAsync(AuthApiResponse result)
        {
            var principal = AuthPrincipalFactory.Create(result.User);

            var properties = new AuthenticationProperties { IsPersistent = true };
            properties.StoreTokens(new[]
            {
                new AuthenticationToken { Name = "access_token", Value = result.AccessToken },
                new AuthenticationToken { Name = "refresh_token", Value = result.RefreshToken },
                new AuthenticationToken { Name = "expires_at_utc", Value = result.ExpiresAtUtc.ToString("o") },
            });

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, properties);
        }
    }
}
