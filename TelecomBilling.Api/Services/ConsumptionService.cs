using Microsoft.EntityFrameworkCore;
using TelecomBilling.Api.Data;
using TelecomBilling.Api.DTOs;
using TelecomBilling.Api.Models;
using TelecomBilling.Api.Utils;

namespace TelecomBilling.Api.Services
{
    public class ConsumptionService : IConsumptionService
    {
        private readonly TelecomBillingDbContext _context;
        private readonly ICostCalculationService _costCalculationService;

        public ConsumptionService(TelecomBillingDbContext context, ICostCalculationService costCalculationService)
        {
            _context = context;
            _costCalculationService = costCalculationService;
        }

        public async Task<UsageRecordResponse> CreateUsageRecordAsync(UsageRecordRequest request)
        {
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            var month = request.Timestamp.ToString("yyyy-MM");
            var startDate = DateTime.ParseExact($"{month}-01", "yyyy-MM-dd", null);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var existingUsage = await _context.UsageRecords
                .Where(ur => ur.UserId == request.UserId && ur.Timestamp >= startDate && ur.Timestamp <= endDate)
                .OrderBy(ur => ur.Timestamp)
                .ToListAsync();

            var cumulativeMinutes = existingUsage.Sum(ur => ur.CallMinutes);
            var cumulativeDataMB = existingUsage.Sum(ur => ur.DataMB);

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

            var cost = await _costCalculationService.CalculateUsageRecordCostAsync(
                usageRecord, 
                cumulativeMinutes + request.CallMinutes, 
                cumulativeDataMB + request.DataMB);

            usageRecord.CallCost = cost.CallCost;
            usageRecord.DataCost = cost.DataCost;
            usageRecord.SMSCost = cost.SMSCost;
            usageRecord.TotalCost = cost.TotalCost;
            usageRecord.IsBundleExceeded = cost.IsBundleExceeded;
            usageRecord.BundleExceededMinutes = cost.BundleExceededMinutes;
            usageRecord.BundleExceededDataMB = cost.BundleExceededDataMB;

            _context.UsageRecords.Add(usageRecord);
            await _context.SaveChangesAsync();

            await _context.Entry(usageRecord)
                .Reference(ur => ur.User)
                .LoadAsync();

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

        public async Task<BulkUsageRecordResponse> CreateBulkUsageRecordsAsync(BulkUsageRecordRequest request)
        {
            var response = new BulkUsageRecordResponse
            {
                TotalRecords = request.UsageRecords.Count
            };

            var createdRecords = new List<UsageRecordResponse>();

            foreach (var usageRequest in request.UsageRecords)
            {
                try
                {
                    var usageRecord = await CreateUsageRecordAsync(usageRequest);
                    createdRecords.Add(usageRecord);
                    response.SuccessfullyCreated++;
                }
                catch (Exception ex)
                {
                    response.FailedRecords++;
                    response.Errors.Add($"Failed to create usage record for user {usageRequest.UserId}: {ex.Message}");
                }
            }

            response.CreatedRecords = createdRecords;
            return response;
        }

        public async Task<TopConsumersResponse> GetTopConsumersAsync(string? month, int limit, string sortBy)
        {
            var targetMonth = string.IsNullOrEmpty(month) ? DateTime.UtcNow.ToString("yyyy-MM") : MonthFormatHelper.NormalizeMonthFormat(month) ?? month;
            
            var startDate = MonthFormatHelper.ParseMonthToStartDate(targetMonth);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var query = _context.UsageRecords
                .Include(ur => ur.User)
                .Where(ur => ur.Timestamp >= startDate && ur.Timestamp <= endDate)
                .GroupBy(ur => new { ur.UserId, ur.User!.Name, ur.User.PhoneNumber, ur.User.PlanType })
                .Select(g => new TopConsumerItem
                {
                    UserId = g.Key.UserId,
                    UserName = g.Key.Name,
                    PhoneNumber = g.Key.PhoneNumber,
                    PlanType = g.Key.PlanType,
                    TotalCallMinutes = g.Sum(ur => ur.CallMinutes),
                    TotalDataMB = g.Sum(ur => ur.DataMB),
                    TotalSMSCount = g.Sum(ur => ur.SMSCount),
                    TotalCost = 0
                });

            var topConsumers = sortBy.ToLower() switch
            {
                "voice" => await query.OrderByDescending(x => x.TotalCallMinutes).Take(limit).ToListAsync(),
                "data" => await query.OrderByDescending(x => x.TotalDataMB).Take(limit).ToListAsync(),
                "sms" => await query.OrderByDescending(x => x.TotalSMSCount).Take(limit).ToListAsync(),
                _ => await query.OrderByDescending(x => x.TotalCallMinutes + x.TotalDataMB + x.TotalSMSCount).Take(limit).ToListAsync()
            };

            // Add rank
            for (int i = 0; i < topConsumers.Count; i++)
            {
                topConsumers[i].Rank = i + 1;
            }

            return new TopConsumersResponse
            {
                Month = targetMonth,
                SortBy = sortBy,
                TopConsumers = topConsumers
            };
        }

        public async Task<UsageStatisticsResponse> GetUsageStatisticsAsync(string? month)
        {
            var targetMonth = string.IsNullOrEmpty(month) ? DateTime.UtcNow.ToString("yyyy-MM") : MonthFormatHelper.NormalizeMonthFormat(month) ?? month;
            
            var startDate = MonthFormatHelper.ParseMonthToStartDate(targetMonth);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var usageStats = await _context.UsageRecords
                .Where(ur => ur.Timestamp >= startDate && ur.Timestamp <= endDate)
                .GroupBy(ur => 1)
                .Select(g => new
                {
                    TotalCallMinutes = g.Sum(ur => ur.CallMinutes),
                    TotalDataMB = g.Sum(ur => ur.DataMB),
                    TotalSMSCount = g.Sum(ur => ur.SMSCount),
                    PeakTimeMinutes = g.Where(ur => ur.IsPeakTime).Sum(ur => ur.CallMinutes),
                    OffPeakTimeMinutes = g.Where(ur => !ur.IsPeakTime).Sum(ur => ur.CallMinutes),
                    RoamingMinutes = g.Where(ur => ur.IsRoaming).Sum(ur => ur.CallMinutes),
                    RoamingDataMB = g.Where(ur => ur.IsRoaming).Sum(ur => ur.DataMB),
                    RoamingSMSCount = g.Where(ur => ur.IsRoaming).Sum(ur => ur.SMSCount)
                })
                .FirstOrDefaultAsync();

            var totalSubscribers = await _context.Users.CountAsync(u => u.IsActive);

            return new UsageStatisticsResponse
            {
                Month = targetMonth,
                TotalSubscribers = totalSubscribers,
                TotalCallMinutes = usageStats?.TotalCallMinutes ?? 0,
                TotalDataMB = usageStats?.TotalDataMB ?? 0,
                TotalSMSCount = usageStats?.TotalSMSCount ?? 0,
                PeakTimeMinutes = usageStats?.PeakTimeMinutes ?? 0,
                OffPeakTimeMinutes = usageStats?.OffPeakTimeMinutes ?? 0,
                RoamingMinutes = usageStats?.RoamingMinutes ?? 0,
                RoamingDataMB = usageStats?.RoamingDataMB ?? 0,
                RoamingSMSCount = usageStats?.RoamingSMSCount ?? 0,
                AverageCallMinutesPerUser = totalSubscribers > 0 ? (usageStats?.TotalCallMinutes ?? 0) / (decimal)totalSubscribers : 0,
                AverageDataMBPerUser = totalSubscribers > 0 ? (usageStats?.TotalDataMB ?? 0) / (decimal)totalSubscribers : 0,
                AverageSMSCountPerUser = totalSubscribers > 0 ? (usageStats?.TotalSMSCount ?? 0) / (decimal)totalSubscribers : 0
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
                CallCost = usageRecord.CallCost,
                DataCost = usageRecord.DataCost,
                SMSCost = usageRecord.SMSCost,
                TotalCost = usageRecord.TotalCost,
                IsBundleExceeded = usageRecord.IsBundleExceeded,
                BundleExceededMinutes = usageRecord.BundleExceededMinutes,
                BundleExceededDataMB = usageRecord.BundleExceededDataMB,
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