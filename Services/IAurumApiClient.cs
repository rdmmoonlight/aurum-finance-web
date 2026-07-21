using AurumFinance.Models.Api;

namespace AurumFinance.Services
{
    /// <summary>
    /// Everything this MVC app needs from Aurum.Api's Authentication feature.
    /// Implemented by AurumApiClient — a single typed HttpClient wrapping
    /// the backend's /api/auth/* endpoints, so this app owns no user
    /// credentials or database of its own; the backend remains the single
    /// source of truth for accounts.
    /// </summary>
    public interface IAurumApiClient
    {
        Task<AuthApiResponse> RegisterAsync(string email, string password, string? fullName, CancellationToken ct = default);

        Task<AuthApiResponse> LoginAsync(string email, string password, CancellationToken ct = default);

        Task<AuthApiResponse> RefreshAsync(string refreshToken, CancellationToken ct = default);

        Task LogoutAsync(string refreshToken, CancellationToken ct = default);

        Task<AuthenticatedUserApiDto> GetMeAsync(string accessToken, CancellationToken ct = default);

        /// <summary>Always completes without throwing, whether or not the email is registered — mirrors the backend's own non-enumeration behavior.</summary>
        Task ForgotPasswordAsync(string email, CancellationToken ct = default);

        Task ResetPasswordAsync(string token, string newPassword, CancellationToken ct = default);

        Task VerifyEmailAsync(string token, CancellationToken ct = default);

        /// <summary>Always completes without throwing, whether or not the email is registered or already verified.</summary>
        Task ResendVerificationAsync(string email, CancellationToken ct = default);
    }
}
