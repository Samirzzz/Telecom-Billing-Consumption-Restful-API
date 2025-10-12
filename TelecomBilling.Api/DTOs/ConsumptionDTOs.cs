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
}
