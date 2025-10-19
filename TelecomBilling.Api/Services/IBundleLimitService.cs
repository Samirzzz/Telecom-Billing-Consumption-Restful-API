using TelecomBilling.Api.DTOs;

namespace TelecomBilling.Api.Services
{
    public interface IBundleLimitService
    {
        Task<BundleLimitListResponse> GetBundleLimitsAsync(int pageNumber = 1, int pageSize = 10);
        Task<BundleLimitResponse?> GetBundleLimitAsync(int id);
        Task<BundleLimitResponse?> GetBundleLimitByPlanTypeAsync(string planType);
        Task<BundleLimitResponse> CreateBundleLimitAsync(BundleLimitRequest request);
        Task<BundleLimitResponse?> UpdateBundleLimitAsync(int id, BundleLimitRequest request);
        Task<bool> DeleteBundleLimitAsync(int id);
        Task<BundleLimitValidationResult> ValidateUsageAgainstLimitsAsync(int userId, string month);
        Task<UsageLimitCheckResponse> CheckUsageLimitsAsync(UsageLimitCheckRequest request);
        bool IsPeakTime(DateTime timestamp);
        Task<bool> IsWithinBundleLimitsAsync(int userId, string month, int additionalVoiceMinutes = 0, int additionalDataMB = 0, int additionalSMS = 0);
    }
}
