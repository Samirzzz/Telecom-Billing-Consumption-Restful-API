using System.ComponentModel.DataAnnotations;
using TelecomBilling.Api.Models;

namespace TelecomBilling.Api.DTOs
{
    public class BundleLimitRequest
    {
        [Required]
        [StringLength(50)]
        public string PlanType { get; set; } = string.Empty;
        
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Voice minutes limit must be non-negative")]
        public int VoiceMinutesLimit { get; set; }
        
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Data MB limit must be non-negative")]
        public int DataMBLimit { get; set; }
        
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "SMS limit must be non-negative")]
        public int SMSLimit { get; set; }
        
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Peak time minutes limit must be non-negative")]
        public int PeakTimeMinutesLimit { get; set; }
        
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Off-peak time minutes limit must be non-negative")]
        public int OffPeakTimeMinutesLimit { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
    
    public class BundleLimitResponse
    {
        public int Id { get; set; }
        public string PlanType { get; set; } = string.Empty;
        public int VoiceMinutesLimit { get; set; }
        public int DataMBLimit { get; set; }
        public int SMSLimit { get; set; }
        public int PeakTimeMinutesLimit { get; set; }
        public int OffPeakTimeMinutesLimit { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUpdated { get; set; }
    }
    
    public class BundleLimitListResponse
    {
        public List<BundleLimitResponse> BundleLimits { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
    
    public class BundleLimitValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Violations { get; set; } = new();
        public BundleLimitResponse? BundleLimit { get; set; }
    }
    
    public class UsageLimitCheckRequest
    {
        [Required]
        public int UserId { get; set; }
        
        [Required]
        public string Month { get; set; } = string.Empty; // Format: YYYY-MM
        
        public ResponseFormat ResponseFormat { get; set; } = ResponseFormat.Json;
    }
    
    public class UsageLimitCheckResponse
    {
        public int UserId { get; set; }
        public string PlanType { get; set; } = string.Empty;
        public string Month { get; set; } = string.Empty;
        public BundleLimitResponse? BundleLimit { get; set; }
        public UsageSummary CurrentUsage { get; set; } = new();
        public List<LimitViolation> Violations { get; set; } = new();
        public bool HasViolations { get; set; }
    }
    
    public class UsageSummary
    {
        public int TotalVoiceMinutes { get; set; }
        public int TotalDataMB { get; set; }
        public int TotalSMS { get; set; }
        public int PeakTimeMinutes { get; set; }
        public int OffPeakTimeMinutes { get; set; }
    }
    
    public class LimitViolation
    {
        public string LimitType { get; set; } = string.Empty;
        public int CurrentUsage { get; set; }
        public int Limit { get; set; }
        public int Excess { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}

