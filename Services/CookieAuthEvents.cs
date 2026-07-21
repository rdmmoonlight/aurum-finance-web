using System.Globalization;
using AurumFinance.Exceptions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace AurumFinance.Services
{
    /// <summary>
    /// Wired into CookieAuthenticationOptions.Events.OnValidatePrincipal in
    /// Program.cs. Runs on every authenticated request: if the backend
    /// access token stored alongside the cookie is close to (or past) its
    /// ExpiresAtUtc, this calls the backend's /api/auth/refresh with the
    /// stored refresh token and re-issues the cookie with fresh tokens —
    /// so a user's session survives past the access token's short lifetime
    /// without ever needing to log in again, until the refresh token itself
    /// is rejected (revoked, or its own longer expiry passed).
    /// </summary>
    public static class CookieAuthEvents
    {
        private static readonly TimeSpan RefreshBuffer = TimeSpan.FromMinutes(2);

        public static async Task ValidateAsync(CookieValidatePrincipalContext context)
        {
            var expiresAtRaw = context.Properties.GetTokenValue("expires_at_utc");
            var refreshToken = context.Properties.GetTokenValue("refresh_token");

            if (string.IsNullOrEmpty(expiresAtRaw) || string.IsNullOrEmpty(refreshToken))
            {
                // Cookie predates this token-storing logic, or was tampered
                // with — treat it as unauthenticated rather than guessing.
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return;
            }

            var expiresAtUtc = DateTime.Parse(expiresAtRaw, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
            if (DateTime.UtcNow < expiresAtUtc - RefreshBuffer)
            {
                return; // still comfortably valid — nothing to do this request.
            }

            var apiClient = context.HttpContext.RequestServices.GetRequiredService<IAurumApiClient>();

            try
            {
                var result = await apiClient.RefreshAsync(refreshToken, context.HttpContext.RequestAborted);

                context.Properties.StoreTokens(new[]
                {
                    new AuthenticationToken { Name = "access_token", Value = result.AccessToken },
                    new AuthenticationToken { Name = "refresh_token", Value = result.RefreshToken },
                    new AuthenticationToken { Name = "expires_at_utc", Value = result.ExpiresAtUtc.ToString("o", CultureInfo.InvariantCulture) },
                });

                context.ShouldRenew = true;
                context.ReplacePrincipal(AuthPrincipalFactory.Create(result.User));
            }
            catch (AurumApiException)
            {
                // Refresh token itself is invalid, expired, or was revoked
                // (e.g. after a password reset elsewhere) — force re-login
                // rather than keep serving a principal we can no longer back.
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
        }
    }
}
