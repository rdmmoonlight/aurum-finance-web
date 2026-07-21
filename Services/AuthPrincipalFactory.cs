using System.Security.Claims;
using AurumFinance.Models.Api;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace AurumFinance.Services
{
    /// <summary>
    /// Maps the backend's AuthenticatedUserApiDto onto the claims this MVC
    /// app authenticates with locally. The Role claim uses the standard
    /// ClaimTypes.Role, so [Authorize(Roles = "Admin")] works here exactly
    /// like it does on the backend.
    /// </summary>
    public static class AuthPrincipalFactory
    {
        public static ClaimsPrincipal Create(AuthenticatedUserApiDto user)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Name, string.IsNullOrWhiteSpace(user.FullName) ? user.Email : user.FullName),
                new(ClaimTypes.Role, user.Role),
                new("email_confirmed", user.EmailConfirmed ? "true" : "false"),
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            return new ClaimsPrincipal(identity);
        }
    }
}
