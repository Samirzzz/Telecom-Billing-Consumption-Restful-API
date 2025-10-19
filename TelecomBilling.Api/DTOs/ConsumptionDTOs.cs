using System.ComponentModel.DataAnnotations;
using TelecomBilling.Api.Models;

namespace TelecomBilling.Api.DTOs
{
    public class UsageRecordRequest
    {
        [Required]
        public int UserId { get; set; }
        
        [Required]
        public DateTime Timestamp { get; set; }
        
        public int CallMinutes { get; set; }
        
        public int DataMB { get; set; }
        
        public int SMSCount { get; set; }
        
        public bool IsPeakTime { get; set; }
        
        public bool IsRoaming { get; set; }
        
        public ResponseFormat ResponseFormat { get; set; } = ResponseFormat.Json;
    }
    
    public class UsageRecordResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime Timestamp { get; set; }
        public int CallMinutes { get; set; }
        public int DataMB { get; set; }
        public int SMSCount { get; set; }
        public bool IsPeakTime { get; set; }
        public bool IsRoaming { get; set; }
        public DateTime CreatedAt { get; set; }
        public UserInfo? User { get; set; }
    }
    
    public class ConsumptionSummaryResponse
    {
        public int UserId { get; set; }
        public string Month { get; set; } = string.Empty;
        public int TotalCallMinutes { get; set; }
        public int TotalDataMB { get; set; }
        public int TotalSMSCount { get; set; }
        public int PeakTimeMinutes { get; set; }
        public int OffPeakTimeMinutes { get; set; }
        public int RoamingMinutes { get; set; }
        public int RoamingDataMB { get; set; }
        public int RoamingSMSCount { get; set; }
        public List<UsageRecordResponse> UsageRecords { get; set; } = new();
    }

    public class UsageRecordListResponse
    {
        public List<UsageRecordResponse> UsageRecords { get; set; } = new List<UsageRecordResponse>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class BulkUsageRecordRequest
    {
        [Required]
        public List<UsageRecordRequest> UsageRecords { get; set; } = new List<UsageRecordRequest>();
    }

    public class BulkUsageRecordResponse
    {
        public int TotalRecords { get; set; }
        public int SuccessfullyCreated { get; set; }
        public int FailedRecords { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<UsageRecordResponse> CreatedRecords { get; set; } = new List<UsageRecordResponse>();
    }

    public class TopConsumersResponse
    {
        public string Month { get; set; } = string.Empty;
        public string SortBy { get; set; } = string.Empty;
        public List<TopConsumerItem> TopConsumers { get; set; } = new List<TopConsumerItem>();
    }

    public class TopConsumerItem
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string PlanType { get; set; } = string.Empty;
        public int TotalCallMinutes { get; set; }
        public int TotalDataMB { get; set; }
        public int TotalSMSCount { get; set; }
        public decimal TotalCost { get; set; }
        public int Rank { get; set; }
    }

    public class UsageStatisticsResponse
    {
        public string Month { get; set; } = string.Empty;
        public int TotalSubscribers { get; set; }
        public int TotalCallMinutes { get; set; }
        public int TotalDataMB { get; set; }
        public int TotalSMSCount { get; set; }
        public int PeakTimeMinutes { get; set; }
        public int OffPeakTimeMinutes { get; set; }
        public int RoamingMinutes { get; set; }
        public int RoamingDataMB { get; set; }
        public int RoamingSMSCount { get; set; }
        public decimal AverageCallMinutesPerUser { get; set; }
        public decimal AverageDataMBPerUser { get; set; }
        public decimal AverageSMSCountPerUser { get; set; }
    }
}
