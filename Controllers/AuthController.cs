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
    [AllowAnonymous] // Mengizinkan tamu/user anonim mengakses controller ini
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
            return View(new LoginViewModel());
        }

        // POST: /Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var result = await _apiClient.LoginAsync(model.Email, model.Password);
                await SignInAsync(result);
                return RedirectToAction("Index", "Home"); // Atau "Welcome" / "Dashboard"
            }
            catch (AurumApiException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Gagal terhubung ke server: {ex.Message}");
                return View(model);
            }
        }

        // =========================================================================
        // GET: /Auth/Register (SEBELUMNYA HILANG - PENYEBAB HALAMAN KOSONG)
        // =========================================================================
        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        // =========================================================================
        // POST: /Auth/Register (SEBELUMNYA HILANG - PENYEBAB SUBMIT GAGAL)
        // =========================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // 1. Panggil API Backend
                var result = await _apiClient.RegisterAsync(model.Email, model.Password, model.FullName);
                
                // 2. Set cookie autentikasi
                await SignInAsync(result);
                
                // 3. Tampilkan pesan sukses
                TempData["SuccessMessage"] = "Registrasi berhasil! Link verifikasi telah dikirim ke email Anda.";
                
                // 4. Redirect ke Halaman Utama / Dashboard
                return RedirectToAction("Index", "Home");
            }
            catch (AurumApiException ex)
            {
                // Error dari API (misal: Email sudah terdaftar)
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
            catch (Exception ex)
            {
                // Error umum (misal: API offline / koneksi gagal)
                ModelState.AddModelError(string.Empty, $"Gagal terhubung ke server backend: {ex.Message}");
                return View(model);
            }
        }

        // GET: /Auth/Logout
        [HttpGet]
        [Authorize] // Khusus Logout, wajib login dulu
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
                    // Token expired/invalid di server, tetap hapus cookie lokal
                }
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Auth");
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

            await _apiClient.ForgotPasswordAsync(model.Email);
            TempData["SuccessMessage"] = "Jika email terdaftar, tautan reset password telah dikirimkan.";
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
                TempData["SuccessMessage"] = "Password berhasil diubah. Silakan masuk menggunakan password baru.";
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
                ViewBag.Message = "Tautan verifikasi tidak valid.";
                return View();
            }

            try
            {
                await _apiClient.VerifyEmailAsync(token);
                ViewBag.Success = true;
                ViewBag.Message = "Email Anda telah berhasil diverifikasi!";
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

            await _apiClient.ResendVerificationAsync(model.Email);
            TempData["SuccessMessage"] = "Jika email terdaftar dan belum diverifikasi, link baru telah dikirimkan.";
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