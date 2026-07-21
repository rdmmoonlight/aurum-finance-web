namespace AurumFinance.Models.Api
{
    /// <summary>
    /// Mirrors Aurum.Api's Features/Authentication/Dtos/AuthenticatedUserDto.
    /// Field names must match exactly (case-insensitive) for System.Text.Json's
    /// default deserialization to bind them.
    /// </summary>
    public class AuthenticatedUserApiDto
    {
        public Guid Id { get; set; }

        public string Email { get; set; } = string.Empty;

        public string? FullName { get; set; }

        public string Role { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public bool EmailConfirmed { get; set; }
    }

    /// <summary>Mirrors Aurum.Api's Features/Authentication/Dtos/AuthResponse.</summary>
    public class AuthApiResponse
    {
        public string AccessToken { get; set; } = string.Empty;

        public DateTime ExpiresAtUtc { get; set; }

        public string RefreshToken { get; set; } = string.Empty;

        public AuthenticatedUserApiDto User { get; set; } = new();
    }

    /// <summary>Mirrors Aurum.Api's Core/Shared/ErrorResponse — every error from the backend comes back in this flat shape.</summary>
    public class ApiErrorResponse
    {
        public string Error { get; set; } = string.Empty;
    }
}
