using Microsoft.EntityFrameworkCore;
using TelecomBilling.Api.Data;
using TelecomBilling.Api.DTOs;
using TelecomBilling.Api.Models;
using TelecomBilling.Api.Utils;

namespace TelecomBilling.Api.Services
{
    public class BundleLimitService : IBundleLimitService
    {
        private readonly TelecomBillingDbContext _context;

        public BundleLimitService(TelecomBillingDbContext context)
        {
            _context = context;
        }

        public async Task<BundleLimitListResponse> GetBundleLimitsAsync(int pageNumber = 1, int pageSize = 10)
        {
            var query = _context.BundleLimits.AsQueryable();
            var totalCount = await query.CountAsync();
            
            var bundleLimits = await query
                .OrderBy(bl => bl.PlanType)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new BundleLimitListResponse
            {
                BundleLimits = bundleLimits.Select(MapToBundleLimitResponse).ToList(),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<BundleLimitResponse?> GetBundleLimitAsync(int id)
        {
            var bundleLimit = await _context.BundleLimits.FindAsync(id);
            return bundleLimit != null ? MapToBundleLimitResponse(bundleLimit) : null;
        }

        public async Task<BundleLimitResponse?> GetBundleLimitByPlanTypeAsync(string planType)
        {
            var bundleLimit = await _context.BundleLimits
                .FirstOrDefaultAsync(bl => bl.PlanType == planType && bl.IsActive);

            return bundleLimit != null ? MapToBundleLimitResponse(bundleLimit) : null;
        }

        public async Task<BundleLimitResponse> CreateBundleLimitAsync(BundleLimitRequest request)
        {
            var bundleLimit = new BundleLimit
            {
                PlanType = request.PlanType,
                VoiceMinutesLimit = request.VoiceMinutesLimit,
                DataMBLimit = request.DataMBLimit,
                SMSLimit = request.SMSLimit,
                PeakTimeMinutesLimit = request.PeakTimeMinutesLimit,
                OffPeakTimeMinutesLimit = request.OffPeakTimeMinutesLimit,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.BundleLimits.Add(bundleLimit);
            await _context.SaveChangesAsync();

            return MapToBundleLimitResponse(bundleLimit);
        }

        public async Task<BundleLimitResponse?> UpdateBundleLimitAsync(int id, BundleLimitRequest request)
        {
            var bundleLimit = await _context.BundleLimits.FindAsync(id);
            if (bundleLimit == null)
            {
                return null;
            }

            bundleLimit.PlanType = request.PlanType;
            bundleLimit.VoiceMinutesLimit = request.VoiceMinutesLimit;
            bundleLimit.DataMBLimit = request.DataMBLimit;
            bundleLimit.SMSLimit = request.SMSLimit;
            bundleLimit.PeakTimeMinutesLimit = request.PeakTimeMinutesLimit;
            bundleLimit.OffPeakTimeMinutesLimit = request.OffPeakTimeMinutesLimit;
            bundleLimit.IsActive = request.IsActive;
            bundleLimit.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapToBundleLimitResponse(bundleLimit);
        }

        public async Task<bool> DeleteBundleLimitAsync(int id)
        {
            var bundleLimit = await _context.BundleLimits.FindAsync(id);
            if (bundleLimit == null)
            {
                return false;
            }

            _context.BundleLimits.Remove(bundleLimit);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<BundleLimitValidationResult> ValidateUsageAgainstLimitsAsync(int userId, string month)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return new BundleLimitValidationResult
                {
                    IsValid = false,
                    Violations = new List<string> { "User not found" }
                };
            }

            var bundleLimit = await GetBundleLimitByPlanTypeAsync(user.PlanType);
            if (bundleLimit == null)
            {
                return new BundleLimitValidationResult
                {
                    IsValid = true, // No limits defined, so usage is valid
                    BundleLimit = null
                };
            }

            var startDate = MonthFormatHelper.ParseMonthToStartDate(month);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var usage = await _context.UsageRecords
                .Where(ur => ur.UserId == userId && ur.Timestamp >= startDate && ur.Timestamp <= endDate)
                .GroupBy(ur => 1)
                .Select(g => new
                {
                    TotalVoiceMinutes = g.Sum(ur => ur.CallMinutes),
                    TotalDataMB = g.Sum(ur => ur.DataMB),
                    TotalSMS = g.Sum(ur => ur.SMSCount),
                    PeakTimeMinutes = g.Where(ur => ur.IsPeakTime).Sum(ur => ur.CallMinutes),
                    OffPeakTimeMinutes = g.Where(ur => !ur.IsPeakTime).Sum(ur => ur.CallMinutes)
                })
                .FirstOrDefaultAsync();

            var violations = new List<string>();

            if (usage != null)
            {
                if (usage.TotalVoiceMinutes > bundleLimit.VoiceMinutesLimit)
                {
                    violations.Add($"Voice minutes limit exceeded: {usage.TotalVoiceMinutes}/{bundleLimit.VoiceMinutesLimit}");
                }

                if (usage.TotalDataMB > bundleLimit.DataMBLimit)
                {
                    violations.Add($"Data MB limit exceeded: {usage.TotalDataMB}/{bundleLimit.DataMBLimit}");
                }

                if (usage.TotalSMS > bundleLimit.SMSLimit)
                {
                    violations.Add($"SMS limit exceeded: {usage.TotalSMS}/{bundleLimit.SMSLimit}");
                }

                if (usage.PeakTimeMinutes > bundleLimit.PeakTimeMinutesLimit)
                {
                    violations.Add($"Peak time minutes limit exceeded: {usage.PeakTimeMinutes}/{bundleLimit.PeakTimeMinutesLimit}");
                }

                if (usage.OffPeakTimeMinutes > bundleLimit.OffPeakTimeMinutesLimit)
                {
                    violations.Add($"Off-peak time minutes limit exceeded: {usage.OffPeakTimeMinutes}/{bundleLimit.OffPeakTimeMinutesLimit}");
                }
            }

            return new BundleLimitValidationResult
            {
                IsValid = violations.Count == 0,
                Violations = violations,
                BundleLimit = bundleLimit
            };
        }

        public async Task<UsageLimitCheckResponse> CheckUsageLimitsAsync(UsageLimitCheckRequest request)
        {
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
            {
                return new UsageLimitCheckResponse
                {
                    UserId = request.UserId,
                    Month = request.Month,
                    HasViolations = true,
                    Violations = new List<LimitViolation> { new LimitViolation { Message = "User not found" } }
                };
            }

            var bundleLimit = await GetBundleLimitByPlanTypeAsync(user.PlanType);
            if (bundleLimit == null)
            {
                return new UsageLimitCheckResponse
                {
                    UserId = request.UserId,
                    PlanType = user.PlanType,
                    Month = request.Month,
                    BundleLimit = null,
                    HasViolations = false
                };
            }

            var normalizedMonth = MonthFormatHelper.NormalizeMonthFormat(request.Month) ?? request.Month;
            var startDate = MonthFormatHelper.ParseMonthToStartDate(normalizedMonth);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var usage = await _context.UsageRecords
                .Where(ur => ur.UserId == request.UserId && ur.Timestamp >= startDate && ur.Timestamp <= endDate)
                .GroupBy(ur => 1)
                .Select(g => new
                {
                    TotalVoiceMinutes = g.Sum(ur => ur.CallMinutes),
                    TotalDataMB = g.Sum(ur => ur.DataMB),
                    TotalSMS = g.Sum(ur => ur.SMSCount),
                    PeakTimeMinutes = g.Where(ur => ur.IsPeakTime).Sum(ur => ur.CallMinutes),
                    OffPeakTimeMinutes = g.Where(ur => !ur.IsPeakTime).Sum(ur => ur.CallMinutes)
                })
                .FirstOrDefaultAsync();

            var currentUsage = new UsageSummary();
            var violations = new List<LimitViolation>();

            if (usage != null)
            {
                currentUsage = new UsageSummary
                {
                    TotalVoiceMinutes = usage.TotalVoiceMinutes,
                    TotalDataMB = usage.TotalDataMB,
                    TotalSMS = usage.TotalSMS,
                    PeakTimeMinutes = usage.PeakTimeMinutes,
                    OffPeakTimeMinutes = usage.OffPeakTimeMinutes
                };

                // Check voice minutes limit
                if (usage.TotalVoiceMinutes > bundleLimit.VoiceMinutesLimit)
                {
                    violations.Add(new LimitViolation
                    {
                        LimitType = "Voice Minutes",
                        CurrentUsage = usage.TotalVoiceMinutes,
                        Limit = bundleLimit.VoiceMinutesLimit,
                        Excess = usage.TotalVoiceMinutes - bundleLimit.VoiceMinutesLimit,
                        Message = $"Voice minutes limit exceeded by {usage.TotalVoiceMinutes - bundleLimit.VoiceMinutesLimit} minutes"
                    });
                }

                // Check data MB limit
                if (usage.TotalDataMB > bundleLimit.DataMBLimit)
                {
                    violations.Add(new LimitViolation
                    {
                        LimitType = "Data MB",
                        CurrentUsage = usage.TotalDataMB,
                        Limit = bundleLimit.DataMBLimit,
                        Excess = usage.TotalDataMB - bundleLimit.DataMBLimit,
                        Message = $"Data MB limit exceeded by {usage.TotalDataMB - bundleLimit.DataMBLimit} MB"
                    });
                }

                // Check SMS limit
                if (usage.TotalSMS > bundleLimit.SMSLimit)
                {
                    violations.Add(new LimitViolation
                    {
                        LimitType = "SMS",
                        CurrentUsage = usage.TotalSMS,
                        Limit = bundleLimit.SMSLimit,
                        Excess = usage.TotalSMS - bundleLimit.SMSLimit,
                        Message = $"SMS limit exceeded by {usage.TotalSMS - bundleLimit.SMSLimit} messages"
                    });
                }

                // Check peak time minutes limit
                if (usage.PeakTimeMinutes > bundleLimit.PeakTimeMinutesLimit)
                {
                    violations.Add(new LimitViolation
                    {
                        LimitType = "Peak Time Minutes",
                        CurrentUsage = usage.PeakTimeMinutes,
                        Limit = bundleLimit.PeakTimeMinutesLimit,
                        Excess = usage.PeakTimeMinutes - bundleLimit.PeakTimeMinutesLimit,
                        Message = $"Peak time minutes limit exceeded by {usage.PeakTimeMinutes - bundleLimit.PeakTimeMinutesLimit} minutes"
                    });
                }

                // Check off-peak time minutes limit
                if (usage.OffPeakTimeMinutes > bundleLimit.OffPeakTimeMinutesLimit)
                {
                    violations.Add(new LimitViolation
                    {
                        LimitType = "Off-Peak Time Minutes",
                        CurrentUsage = usage.OffPeakTimeMinutes,
                        Limit = bundleLimit.OffPeakTimeMinutesLimit,
                        Excess = usage.OffPeakTimeMinutes - bundleLimit.OffPeakTimeMinutesLimit,
                        Message = $"Off-peak time minutes limit exceeded by {usage.OffPeakTimeMinutes - bundleLimit.OffPeakTimeMinutesLimit} minutes"
                    });
                }
            }

            return new UsageLimitCheckResponse
            {
                UserId = request.UserId,
                PlanType = user.PlanType,
                Month = normalizedMonth,
                BundleLimit = bundleLimit,
                CurrentUsage = currentUsage,
                Violations = violations,
                HasViolations = violations.Count > 0
            };
        }

        public bool IsPeakTime(DateTime timestamp)
        {
            var hour = timestamp.Hour;
            var dayOfWeek = timestamp.DayOfWeek;
            
            return dayOfWeek >= DayOfWeek.Monday && 
                   dayOfWeek <= DayOfWeek.Friday && 
                   hour >= 8 && 
                   hour < 20;
        }

        public async Task<bool> IsWithinBundleLimitsAsync(int userId, string month, int additionalVoiceMinutes = 0, int additionalDataMB = 0, int additionalSMS = 0)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            var bundleLimit = await GetBundleLimitByPlanTypeAsync(user.PlanType);
            if (bundleLimit == null) return true; // No limits defined

            var startDate = MonthFormatHelper.ParseMonthToStartDate(month);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var usage = await _context.UsageRecords
                .Where(ur => ur.UserId == userId && ur.Timestamp >= startDate && ur.Timestamp <= endDate)
                .GroupBy(ur => 1)
                .Select(g => new
                {
                    TotalVoiceMinutes = g.Sum(ur => ur.CallMinutes),
                    TotalDataMB = g.Sum(ur => ur.DataMB),
                    TotalSMS = g.Sum(ur => ur.SMSCount),
                    PeakTimeMinutes = g.Where(ur => ur.IsPeakTime).Sum(ur => ur.CallMinutes),
                    OffPeakTimeMinutes = g.Where(ur => !ur.IsPeakTime).Sum(ur => ur.CallMinutes)
                })
                .FirstOrDefaultAsync();

            if (usage == null) return true;

            var projectedVoiceMinutes = usage.TotalVoiceMinutes + additionalVoiceMinutes;
            var projectedDataMB = usage.TotalDataMB + additionalDataMB;
            var projectedSMS = usage.TotalSMS + additionalSMS;

            return projectedVoiceMinutes <= bundleLimit.VoiceMinutesLimit &&
                   projectedDataMB <= bundleLimit.DataMBLimit &&
                   projectedSMS <= bundleLimit.SMSLimit &&
                   usage.PeakTimeMinutes <= bundleLimit.PeakTimeMinutesLimit &&
                   usage.OffPeakTimeMinutes <= bundleLimit.OffPeakTimeMinutesLimit;
        }

        private static BundleLimitResponse MapToBundleLimitResponse(BundleLimit bundleLimit)
        {
            return new BundleLimitResponse
            {
                Id = bundleLimit.Id,
                PlanType = bundleLimit.PlanType,
                VoiceMinutesLimit = bundleLimit.VoiceMinutesLimit,
                DataMBLimit = bundleLimit.DataMBLimit,
                SMSLimit = bundleLimit.SMSLimit,
                PeakTimeMinutesLimit = bundleLimit.PeakTimeMinutesLimit,
                OffPeakTimeMinutesLimit = bundleLimit.OffPeakTimeMinutesLimit,
                IsActive = bundleLimit.IsActive,
                CreatedAt = bundleLimit.CreatedAt,
                LastUpdated = bundleLimit.LastUpdated
            };
        }
    }
}
