using TelecomBilling.Api.DTOs;
using TelecomBilling.Api.Models;

namespace TelecomBilling.Api.Services
{
    public interface IConsumptionService
    {
        Task<UsageRecordResponse> CreateUsageRecordAsync(UsageRecordRequest request);
        Task<bool> DeleteUsageRecordAsync(int id);
        Task<UsageRecordListResponse> GetUsageRecordsAsync(int userId, int pageNumber = 1, int pageSize = 10);
        Task<object> GetUsageRecordsWithFormatAsync(int userId, ResponseFormat format, int pageNumber = 1, int pageSize = 10);
    }
}
