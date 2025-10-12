using System.ComponentModel.DataAnnotations;

namespace TelecomBilling.Api.DTOs
{
    public class UserRequest
    {
        // Authentication fields
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;
        
        [Required]
        public string Role { get; set; } = string.Empty;
        
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
        public bool IsActive { get; set; } = true;
    }

    public class UserResponse
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string PlanType { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public bool IsRoaming { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUpdated { get; set; }
    }

    public class UserInfo
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string PlanType { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public bool IsRoaming { get; set; }
        public bool IsActive { get; set; }
    }

    public class UserListResponse
    {
        public List<UserResponse> Users { get; set; } = new List<UserResponse>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
