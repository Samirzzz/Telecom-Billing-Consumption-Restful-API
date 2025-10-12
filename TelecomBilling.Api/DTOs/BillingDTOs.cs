using System.ComponentModel.DataAnnotations;
using TelecomBilling.Api.Models;

namespace TelecomBilling.Api.DTOs
{
    public class BillingRequest
    {
        [Required]
        public int SubscriberId { get; set; }
        
        [Required]
        public string Month { get; set; } = string.Empty; // Format: YYYY-MM
        
        public ResponseFormat ResponseFormat { get; set; } = ResponseFormat.Json;
    }
    
    public class BillingResponse
    {
        public int Id { get; set; }
        public int SubscriberId { get; set; }
        public string Month { get; set; } = string.Empty;
        public DateTime BillingDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal VoiceAmount { get; set; }
        public decimal DataAmount { get; set; }
        public decimal SMSAmount { get; set; }
        public decimal RoamingAmount { get; set; }
        public int VoiceMinutes { get; set; }
        public int DataMB { get; set; }
        public int SMSMessages { get; set; }
        public int RoamingMinutes { get; set; }
        public int RoamingDataMB { get; set; }
        public int RoamingSMSMessages { get; set; }
        public SubscriberInfo? Subscriber { get; set; }
    }
    
    public class SubscriberInfo
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string PlanType { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public bool IsRoaming { get; set; }
        public bool Active { get; set; }
    }
    
    public class BillingListResponse
    {
        public List<BillingResponse> Billings { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
