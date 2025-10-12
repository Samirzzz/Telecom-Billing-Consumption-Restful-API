using Microsoft.EntityFrameworkCore;
using TelecomBilling.Api.Data;
using TelecomBilling.Api.DTOs;
using TelecomBilling.Api.Models;

namespace TelecomBilling.Api.Services
{
    public class ConsumptionService : IConsumptionService
    {
        private readonly TelecomBillingDbContext _context;

        public ConsumptionService(TelecomBillingDbContext context)
        {
            _context = context;
        }

        public async Task<UsageRecordResponse> CreateUsageRecordAsync(UsageRecordRequest request)
        {
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            var usageRecord = new UsageRecord
            {
                UserId = request.UserId,
                Timestamp = request.Timestamp,
                CallMinutes = request.CallMinutes,
                DataMB = request.DataMB,
                SMSCount = request.SMSCount,
                IsPeakTime = request.IsPeakTime,
                IsRoaming = request.IsRoaming,
                CreatedAt = DateTime.UtcNow
            };

            _context.UsageRecords.Add(usageRecord);
            await _context.SaveChangesAsync();

            return MapToUsageRecordResponse(usageRecord);
        }

        public async Task<bool> DeleteUsageRecordAsync(int id)
        {
            var usageRecord = await _context.UsageRecords.FindAsync(id);
            if (usageRecord == null)
            {
                return false;
            }

            _context.UsageRecords.Remove(usageRecord);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<UsageRecordListResponse> GetUsageRecordsAsync(int userId, int pageNumber = 1, int pageSize = 10)
        {
            var query = _context.UsageRecords
                .Include(ur => ur.User)
                .Where(ur => ur.UserId == userId)
                .AsQueryable();

            var totalCount = await query.CountAsync();
            var usageRecords = await query
                .OrderByDescending(ur => ur.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new UsageRecordListResponse
            {
                UsageRecords = usageRecords.Select(MapToUsageRecordResponse).ToList(),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<object> GetUsageRecordsWithFormatAsync(int userId, ResponseFormat format, int pageNumber = 1, int pageSize = 10)
        {
            var usageRecordList = await GetUsageRecordsAsync(userId, pageNumber, pageSize);

            return format switch
            {
                ResponseFormat.Json => usageRecordList,
                ResponseFormat.Soap => usageRecordList,
                ResponseFormat.Xml => usageRecordList,
                _ => usageRecordList
            };
        }

        private static UsageRecordResponse MapToUsageRecordResponse(UsageRecord usageRecord)
        {
            return new UsageRecordResponse
            {
                Id = usageRecord.Id,
                UserId = usageRecord.UserId,
                Timestamp = usageRecord.Timestamp,
                CallMinutes = usageRecord.CallMinutes,
                DataMB = usageRecord.DataMB,
                SMSCount = usageRecord.SMSCount,
                IsPeakTime = usageRecord.IsPeakTime,
                IsRoaming = usageRecord.IsRoaming,
                CreatedAt = usageRecord.CreatedAt,
                User = usageRecord.User != null ? new UserInfo
                {
                    Id = usageRecord.User.Id,
                    Name = usageRecord.User.Name,
                    PhoneNumber = usageRecord.User.PhoneNumber,
                    PlanType = usageRecord.User.PlanType,
                    Country = usageRecord.User.Country,
                    IsRoaming = usageRecord.User.IsRoaming,
                    IsActive = usageRecord.User.IsActive
                } : null
            };
        }
    }
}