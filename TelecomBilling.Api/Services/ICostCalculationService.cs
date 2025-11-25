using TelecomBilling.Api.Models;

namespace TelecomBilling.Api.Services
{
    public interface ICostCalculationService
    {
        Task<UsageRecordCost> CalculateUsageRecordCostAsync(UsageRecord usageRecord, int cumulativeMinutes, int cumulativeDataMB);
        Task<decimal> CalculateMonthlyInvoiceAsync(int userId, string month);
    }

    public class UsageRecordCost
    {
        public decimal CallCost { get; set; }
        public decimal DataCost { get; set; }
        public decimal SMSCost { get; set; }
        public decimal TotalCost { get; set; }
        public bool IsBundleExceeded { get; set; }
        public int BundleExceededMinutes { get; set; }
        public int BundleExceededDataMB { get; set; }
    }
}
