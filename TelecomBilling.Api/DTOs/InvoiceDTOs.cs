using System.ComponentModel.DataAnnotations;
using TelecomBilling.Api.Models;

namespace TelecomBilling.Api.DTOs
{
    public class InvoiceRequest
    {
        [Required]
        public int UserId { get; set; }
        
        [Required]
        public string Month { get; set; } = string.Empty; // Format: YYYY-MM
        
        public ResponseFormat ResponseFormat { get; set; } = ResponseFormat.Json;
    }
    
    public class InvoiceResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
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
        public UserInfo? User { get; set; }
    }
    
    public class InvoiceListResponse
    {
        public List<InvoiceResponse> Invoices { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
