using TelecomBilling.Api.DTOs;

namespace TelecomBilling.Api.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);
        Task<bool> ValidateTokenAsync(string token);
        Task<UserInfo?> GetUserFromTokenAsync(string token);
    }
}
