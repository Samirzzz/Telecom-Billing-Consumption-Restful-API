using Microsoft.EntityFrameworkCore;
using TelecomBilling.Api.Data;
using TelecomBilling.Api.Models;

namespace TelecomBilling.Api.Services
{
    public class CostCalculationService : ICostCalculationService
    {
        private readonly TelecomBillingDbContext _context;
        
        private const decimal PEAK_CALL_RATE = 0.15m;
        private const decimal OFF_PEAK_CALL_RATE = 0.05m;
        private const decimal DOMESTIC_DATA_RATE = 0.05m;
        private const decimal ROAMING_DATA_RATE = 0.20m;
        private const decimal DOMESTIC_SMS_RATE = 0.02m;
        private const decimal ROAMING_SMS_RATE = 0.10m;
        
        private const int BUNDLE_MINUTES_LIMIT = 1000;
        private const int BUNDLE_DATA_LIMIT_MB = 10000;
        private const decimal BUNDLE_EXCEEDED_MULTIPLIER = 2.0m;

        public CostCalculationService(TelecomBillingDbContext context)
        {
            _context = context;
        }

        public async Task<UsageRecordCost> CalculateUsageRecordCostAsync(UsageRecord usageRecord, int cumulativeMinutes, int cumulativeDataMB)
        {
            var cost = new UsageRecordCost();
            
            var previousMinutes = cumulativeMinutes - usageRecord.CallMinutes;
            var previousDataMB = cumulativeDataMB - usageRecord.DataMB;
            
            var bundleExceededMinutes = Math.Max(0, cumulativeMinutes - BUNDLE_MINUTES_LIMIT);
            var bundleExceededDataMB = Math.Max(0, cumulativeDataMB - BUNDLE_DATA_LIMIT_MB);
            
            cost.IsBundleExceeded = bundleExceededMinutes > 0 || bundleExceededDataMB > 0;
            cost.BundleExceededMinutes = bundleExceededMinutes;
            cost.BundleExceededDataMB = bundleExceededDataMB;
            
            var callRate = usageRecord.IsPeakTime ? PEAK_CALL_RATE : OFF_PEAK_CALL_RATE;
            
            if (previousMinutes < BUNDLE_MINUTES_LIMIT)
            {
                var minutesBeforeLimit = BUNDLE_MINUTES_LIMIT - previousMinutes;
                var withinBundleMinutes = Math.Min(usageRecord.CallMinutes, minutesBeforeLimit);
                var exceededMinutes = Math.Max(0, usageRecord.CallMinutes - withinBundleMinutes);
                
                cost.CallCost = (withinBundleMinutes * callRate) + (exceededMinutes * callRate * BUNDLE_EXCEEDED_MULTIPLIER);
            }
            else
            {
                cost.CallCost = usageRecord.CallMinutes * callRate * BUNDLE_EXCEEDED_MULTIPLIER;
            }
            
            var dataRate = usageRecord.IsRoaming ? ROAMING_DATA_RATE : DOMESTIC_DATA_RATE;
            
            if (previousDataMB < BUNDLE_DATA_LIMIT_MB)
            {
                var dataMBBeforeLimit = BUNDLE_DATA_LIMIT_MB - previousDataMB;
                var withinBundleDataMB = Math.Min(usageRecord.DataMB, dataMBBeforeLimit);
                var exceededDataMB = Math.Max(0, usageRecord.DataMB - withinBundleDataMB);
                
                cost.DataCost = (withinBundleDataMB * dataRate) + (exceededDataMB * dataRate * BUNDLE_EXCEEDED_MULTIPLIER);
            }
            else
            {
                cost.DataCost = usageRecord.DataMB * dataRate * BUNDLE_EXCEEDED_MULTIPLIER;
            }
            
            var smsRate = usageRecord.IsRoaming ? ROAMING_SMS_RATE : DOMESTIC_SMS_RATE;
            cost.SMSCost = usageRecord.SMSCount * smsRate;
            
            cost.TotalCost = cost.CallCost + cost.DataCost + cost.SMSCost;
            
            return await Task.FromResult(cost);
        }

        public async Task<decimal> CalculateMonthlyInvoiceAsync(int userId, string month)
        {
            var startDate = DateTime.ParseExact($"{month}-01", "yyyy-MM-dd", null);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var usageRecords = await _context.UsageRecords
                .Where(ur => ur.UserId == userId && ur.Timestamp >= startDate && ur.Timestamp <= endDate)
                .OrderBy(ur => ur.Timestamp)
                .ToListAsync();

            var totalCost = 0m;
            var cumulativeMinutes = 0;
            var cumulativeDataMB = 0;

            foreach (var record in usageRecords)
            {
                cumulativeMinutes += record.CallMinutes;
                cumulativeDataMB += record.DataMB;
                
                var cost = await CalculateUsageRecordCostAsync(record, cumulativeMinutes, cumulativeDataMB);
                totalCost += cost.TotalCost;
            }

            return totalCost;
        }
    }
}
