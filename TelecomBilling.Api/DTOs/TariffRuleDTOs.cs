using System.ComponentModel.DataAnnotations;
using TelecomBilling.Api.Models;

namespace TelecomBilling.Api.DTOs
{
    public class TariffRuleRequest
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string PlanType { get; set; } = string.Empty;
        
        [Required]
        public decimal VoicePeakRate { get; set; }
        
        [Required]
        public decimal VoiceOffPeakRate { get; set; }
        
        [Required]
        public decimal DataRate { get; set; }
        
        [Required]
        public decimal SMSRate { get; set; }
        
        [Required]
        public decimal RoamingVoiceRate { get; set; }
        
        [Required]
        public decimal RoamingDataRate { get; set; }
        
        [Required]
        public decimal RoamingSMSRate { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
    
    public class TariffRuleResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PlanType { get; set; } = string.Empty;
        public decimal VoicePeakRate { get; set; }
        public decimal VoiceOffPeakRate { get; set; }
        public decimal DataRate { get; set; }
        public decimal SMSRate { get; set; }
        public decimal RoamingVoiceRate { get; set; }
        public decimal RoamingDataRate { get; set; }
        public decimal RoamingSMSRate { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUpdated { get; set; }
    }
    
    public class TariffRuleListResponse
    {
        public List<TariffRuleResponse> TariffRules { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
