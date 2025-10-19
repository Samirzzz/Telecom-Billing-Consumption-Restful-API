using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TelecomBilling.Api.Models
{
    public class User
    {
        public int Id { get; set; }
        
        // Authentication fields
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        
        [Required]
        public UserRole Role { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsActive { get; set; } = true;
        
        // Subscriber fields
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;
        
        [Required]
        public string PlanType { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string Country { get; set; } = string.Empty;
        
        public bool IsRoaming { get; set; }
        
        public DateTime? LastUpdated { get; set; }
        
        // Navigation properties
        public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
        public ICollection<UsageRecord> UsageRecords { get; set; } = new List<UsageRecord>();
    }
    
    public enum UserRole
    {
        Admin,
        User
    }

    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Token { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRevoked { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }

    public class UsageRecord
    {
        public int Id { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required]
        public DateTime Timestamp { get; set; }
        
        public int CallMinutes { get; set; }
        
        public int DataMB { get; set; }
        
        public int SMSCount { get; set; }
        
        public bool IsPeakTime { get; set; }
        
        public bool IsRoaming { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        // Navigation property
        public User? User { get; set; }
    }

    public class Invoice
    {
        public int Id { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required]
        public string Month { get; set; } = string.Empty; // Format: YYYY-MM
        
        [Required]
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
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime? LastUpdated { get; set; }
        
        // Navigation property
        public User? User { get; set; }
    }

    public class TariffRule
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string PlanType { get; set; } = string.Empty;
        
        [Required]
        public decimal VoicePeakRate { get; set; } // Rate per minute during peak hours
        
        [Required]
        public decimal VoiceOffPeakRate { get; set; } // Rate per minute during off-peak hours
        
        [Required]
        public decimal DataRate { get; set; } // Rate per MB
        
        [Required]
        public decimal SMSRate { get; set; } // Rate per SMS
        
        [Required]
        public decimal RoamingVoiceRate { get; set; } // Roaming voice rate per minute
        
        [Required]
        public decimal RoamingDataRate { get; set; } // Roaming data rate per MB
        
        [Required]
        public decimal RoamingSMSRate { get; set; } // Roaming SMS rate per message
        
        public bool IsActive { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime? LastUpdated { get; set; }
    }

    public class BundleLimit
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string PlanType { get; set; } = string.Empty;
        
        [Required]
        public int VoiceMinutesLimit { get; set; } // Monthly voice minutes limit
        
        [Required]
        public int DataMBLimit { get; set; } // Monthly data MB limit
        
        [Required]
        public int SMSLimit { get; set; } // Monthly SMS limit
        
        [Required]
        public int PeakTimeMinutesLimit { get; set; } // Monthly peak time minutes limit
        
        [Required]
        public int OffPeakTimeMinutesLimit { get; set; } // Monthly off-peak time minutes limit
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? LastUpdated { get; set; }
    }

    public enum ResponseFormat
    {
        Json,
        Soap,
        Xml
    }
}
