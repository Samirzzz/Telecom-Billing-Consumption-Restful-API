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
        Task<SubscriberResponse?> GetSubscriberAsync(int id);
        Task<SubscriberListResponse> GetSubscribersAsync(int pageNumber = 1, int pageSize = 10);
        Task<SubscriberResponse?> UpdateSubscriberAsync(int id, UpdateSubscriberRequest request);
        Task<SubscriberResponse?> UpdateSubscriberPlanAsync(int id, UpdatePlanRequest request);
        Task<bool> DeactivateSubscriberAsync(int id);
    }
}
