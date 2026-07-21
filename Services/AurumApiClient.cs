using System.Net.Http.Json;
using AurumFinance.Exceptions;
using AurumFinance.Models.Api;

namespace AurumFinance.Services
{
    /// <summary>
    /// Base address is configured in Program.cs from "Api:BaseUrl"
    /// (appsettings.json) — this class only knows relative paths.
    /// </summary>
    public class AurumApiClient : IAurumApiClient
    {
        private readonly HttpClient _httpClient;

        public AurumApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public Task<AuthApiResponse> RegisterAsync(string email, string password, string? fullName, CancellationToken ct = default) =>
            PostForAuthAsync("api/auth/register", new { Email = email, Password = password, FullName = fullName }, ct);

        public Task<AuthApiResponse> LoginAsync(string email, string password, CancellationToken ct = default) =>
            PostForAuthAsync("api/auth/login", new { Email = email, Password = password }, ct);

        public Task<AuthApiResponse> RefreshAsync(string refreshToken, CancellationToken ct = default) =>
            PostForAuthAsync("api/auth/refresh", new { RefreshToken = refreshToken }, ct);

        public async Task LogoutAsync(string refreshToken, CancellationToken ct = default)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/logout", new { RefreshToken = refreshToken }, ct);
            await EnsureSuccessAsync(response, ct);
        }

        public async Task<AuthenticatedUserApiDto> GetMeAsync(string accessToken, CancellationToken ct = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "api/auth/me");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request, ct);
            await EnsureSuccessAsync(response, ct);

            return await response.Content.ReadFromJsonAsync<AuthenticatedUserApiDto>(cancellationToken: ct)
                ?? throw new AurumApiException("The server returned an empty response.");
        }

        public async Task ForgotPasswordAsync(string email, CancellationToken ct = default)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/forgot-password", new { Email = email }, ct);
            await EnsureSuccessAsync(response, ct);
        }

        public async Task ResetPasswordAsync(string token, string newPassword, CancellationToken ct = default)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/reset-password", new { Token = token, NewPassword = newPassword }, ct);
            await EnsureSuccessAsync(response, ct);
        }

        public async Task VerifyEmailAsync(string token, CancellationToken ct = default)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/verify-email", new { Token = token }, ct);
            await EnsureSuccessAsync(response, ct);
        }

        public async Task ResendVerificationAsync(string email, CancellationToken ct = default)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/resend-verification", new { Email = email }, ct);
            await EnsureSuccessAsync(response, ct);
        }

        private async Task<AuthApiResponse> PostForAuthAsync(string path, object body, CancellationToken ct)
        {
            var response = await _httpClient.PostAsJsonAsync(path, body, ct);
            await EnsureSuccessAsync(response, ct);

            return await response.Content.ReadFromJsonAsync<AuthApiResponse>(cancellationToken: ct)
                ?? throw new AurumApiException("The server returned an empty response.");
        }

        private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken ct)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            string message = $"The request to {response.RequestMessage?.RequestUri} failed with status {(int)response.StatusCode}.";
            try
            {
                var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(cancellationToken: ct);
                if (error is not null && !string.IsNullOrWhiteSpace(error.Error))
                {
                    message = error.Error;
                }
            }
            catch
            {
                // Body wasn't the expected { "error": "..." } shape — fall back to the generic message above.
            }

            throw new AurumApiException(message, (int)response.StatusCode);
        }
    }
}
