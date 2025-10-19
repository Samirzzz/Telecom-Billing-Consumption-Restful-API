using Microsoft.EntityFrameworkCore;
using TelecomBilling.Api.Data;
using TelecomBilling.Api.DTOs;
using TelecomBilling.Api.Models;
using TelecomBilling.Api.Services;

namespace TelecomBilling.Api.Tests
{
    public class BundleLimitValidationTests
    {
        private readonly BundleLimitService _bundleLimitService;
        private readonly TelecomBillingDbContext _context;

        public BundleLimitValidationTests()
        {
            var options = new DbContextOptionsBuilder<TelecomBillingDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new TelecomBillingDbContext(options);
            _bundleLimitService = new BundleLimitService(_context);
        }

        [Fact]
        public async Task ValidateUsageAgainstLimitsAsync_NoUser_ShouldReturnInvalid()
        {
            // Arrange
            var userId = 999; // Non-existent user
            var month = "2024-01";

            // Act
            var result = await _bundleLimitService.ValidateUsageAgainstLimitsAsync(userId, month);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("User not found", result.Violations);
        }

        [Fact]
        public async Task ValidateUsageAgainstLimitsAsync_NoBundleLimit_ShouldReturnValid()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hash",
                Role = UserRole.User,
                Name = "Test User",
                PhoneNumber = "1234567890",
                PlanType = "Basic",
                Country = "US"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var month = "2024-01";

            // Act
            var result = await _bundleLimitService.ValidateUsageAgainstLimitsAsync(user.Id, month);

            // Assert
            Assert.True(result.IsValid);
            Assert.Null(result.BundleLimit);
        }

        [Fact]
        public async Task ValidateUsageAgainstLimitsAsync_WithinLimits_ShouldReturnValid()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hash",
                Role = UserRole.User,
                Name = "Test User",
                PhoneNumber = "1234567890",
                PlanType = "Basic",
                Country = "US"
            };

            var bundleLimit = new BundleLimit
            {
                PlanType = "Basic",
                VoiceMinutesLimit = 1000,
                DataMBLimit = 5000,
                SMSLimit = 1000,
                PeakTimeMinutesLimit = 500,
                OffPeakTimeMinutesLimit = 500,
                IsActive = true
            };

            _context.Users.Add(user);
            _context.BundleLimits.Add(bundleLimit);
            await _context.SaveChangesAsync();

            // Add usage within limits
            var usageRecord = new UsageRecord
            {
                UserId = user.Id,
                Timestamp = new DateTime(2024, 1, 15, 10, 0, 0),
                CallMinutes = 100,
                DataMB = 500,
                SMSCount = 50,
                IsPeakTime = true,
                IsRoaming = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.UsageRecords.Add(usageRecord);
            await _context.SaveChangesAsync();

            var month = "2024-01";

            // Act
            var result = await _bundleLimitService.ValidateUsageAgainstLimitsAsync(user.Id, month);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Violations);
        }

        [Fact]
        public async Task ValidateUsageAgainstLimitsAsync_VoiceMinutesExceeded_ShouldReturnInvalid()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hash",
                Role = UserRole.User,
                Name = "Test User",
                PhoneNumber = "1234567890",
                PlanType = "Basic",
                Country = "US"
            };

            var bundleLimit = new BundleLimit
            {
                PlanType = "Basic",
                VoiceMinutesLimit = 100,
                DataMBLimit = 5000,
                SMSLimit = 1000,
                PeakTimeMinutesLimit = 500,
                OffPeakTimeMinutesLimit = 500,
                IsActive = true
            };

            _context.Users.Add(user);
            _context.BundleLimits.Add(bundleLimit);
            await _context.SaveChangesAsync();

            // Add usage exceeding voice minutes limit
            var usageRecord = new UsageRecord
            {
                UserId = user.Id,
                Timestamp = new DateTime(2024, 1, 15, 10, 0, 0),
                CallMinutes = 150, // Exceeds 100 limit
                DataMB = 500,
                SMSCount = 50,
                IsPeakTime = true,
                IsRoaming = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.UsageRecords.Add(usageRecord);
            await _context.SaveChangesAsync();

            var month = "2024-01";

            // Act
            var result = await _bundleLimitService.ValidateUsageAgainstLimitsAsync(user.Id, month);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("Voice minutes limit exceeded: 150/100", result.Violations);
        }

        [Fact]
        public async Task ValidateUsageAgainstLimitsAsync_DataMBExceeded_ShouldReturnInvalid()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hash",
                Role = UserRole.User,
                Name = "Test User",
                PhoneNumber = "1234567890",
                PlanType = "Basic",
                Country = "US"
            };

            var bundleLimit = new BundleLimit
            {
                PlanType = "Basic",
                VoiceMinutesLimit = 1000,
                DataMBLimit = 100, // Low limit
                SMSLimit = 1000,
                PeakTimeMinutesLimit = 500,
                OffPeakTimeMinutesLimit = 500,
                IsActive = true
            };

            _context.Users.Add(user);
            _context.BundleLimits.Add(bundleLimit);
            await _context.SaveChangesAsync();

            // Add usage exceeding data MB limit
            var usageRecord = new UsageRecord
            {
                UserId = user.Id,
                Timestamp = new DateTime(2024, 1, 15, 10, 0, 0),
                CallMinutes = 100,
                DataMB = 200, // Exceeds 100 limit
                SMSCount = 50,
                IsPeakTime = true,
                IsRoaming = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.UsageRecords.Add(usageRecord);
            await _context.SaveChangesAsync();

            var month = "2024-01";

            // Act
            var result = await _bundleLimitService.ValidateUsageAgainstLimitsAsync(user.Id, month);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("Data MB limit exceeded: 200/100", result.Violations);
        }

        [Fact]
        public async Task ValidateUsageAgainstLimitsAsync_SMSExceeded_ShouldReturnInvalid()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hash",
                Role = UserRole.User,
                Name = "Test User",
                PhoneNumber = "1234567890",
                PlanType = "Basic",
                Country = "US"
            };

            var bundleLimit = new BundleLimit
            {
                PlanType = "Basic",
                VoiceMinutesLimit = 1000,
                DataMBLimit = 5000,
                SMSLimit = 50, // Low limit
                PeakTimeMinutesLimit = 500,
                OffPeakTimeMinutesLimit = 500,
                IsActive = true
            };

            _context.Users.Add(user);
            _context.BundleLimits.Add(bundleLimit);
            await _context.SaveChangesAsync();

            // Add usage exceeding SMS limit
            var usageRecord = new UsageRecord
            {
                UserId = user.Id,
                Timestamp = new DateTime(2024, 1, 15, 10, 0, 0),
                CallMinutes = 100,
                DataMB = 500,
                SMSCount = 75, // Exceeds 50 limit
                IsPeakTime = true,
                IsRoaming = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.UsageRecords.Add(usageRecord);
            await _context.SaveChangesAsync();

            var month = "2024-01";

            // Act
            var result = await _bundleLimitService.ValidateUsageAgainstLimitsAsync(user.Id, month);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("SMS limit exceeded: 75/50", result.Violations);
        }

        [Fact]
        public async Task ValidateUsageAgainstLimitsAsync_PeakTimeMinutesExceeded_ShouldReturnInvalid()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hash",
                Role = UserRole.User,
                Name = "Test User",
                PhoneNumber = "1234567890",
                PlanType = "Basic",
                Country = "US"
            };

            var bundleLimit = new BundleLimit
            {
                PlanType = "Basic",
                VoiceMinutesLimit = 1000,
                DataMBLimit = 5000,
                SMSLimit = 1000,
                PeakTimeMinutesLimit = 50, // Low limit
                OffPeakTimeMinutesLimit = 500,
                IsActive = true
            };

            _context.Users.Add(user);
            _context.BundleLimits.Add(bundleLimit);
            await _context.SaveChangesAsync();

            // Add usage exceeding peak time minutes limit
            var usageRecord = new UsageRecord
            {
                UserId = user.Id,
                Timestamp = new DateTime(2024, 1, 15, 10, 0, 0), // Peak time
                CallMinutes = 75, // Exceeds 50 peak time limit
                DataMB = 500,
                SMSCount = 50,
                IsPeakTime = true,
                IsRoaming = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.UsageRecords.Add(usageRecord);
            await _context.SaveChangesAsync();

            var month = "2024-01";

            // Act
            var result = await _bundleLimitService.ValidateUsageAgainstLimitsAsync(user.Id, month);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("Peak time minutes limit exceeded: 75/50", result.Violations);
        }

        [Fact]
        public async Task ValidateUsageAgainstLimitsAsync_OffPeakTimeMinutesExceeded_ShouldReturnInvalid()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hash",
                Role = UserRole.User,
                Name = "Test User",
                PhoneNumber = "1234567890",
                PlanType = "Basic",
                Country = "US"
            };

            var bundleLimit = new BundleLimit
            {
                PlanType = "Basic",
                VoiceMinutesLimit = 1000,
                DataMBLimit = 5000,
                SMSLimit = 1000,
                PeakTimeMinutesLimit = 500,
                OffPeakTimeMinutesLimit = 50, // Low limit
                IsActive = true
            };

            _context.Users.Add(user);
            _context.BundleLimits.Add(bundleLimit);
            await _context.SaveChangesAsync();

            // Add usage exceeding off-peak time minutes limit
            var usageRecord = new UsageRecord
            {
                UserId = user.Id,
                Timestamp = new DateTime(2024, 1, 15, 20, 0, 0), // Off-peak time
                CallMinutes = 75, // Exceeds 50 off-peak time limit
                DataMB = 500,
                SMSCount = 50,
                IsPeakTime = false,
                IsRoaming = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.UsageRecords.Add(usageRecord);
            await _context.SaveChangesAsync();

            var month = "2024-01";

            // Act
            var result = await _bundleLimitService.ValidateUsageAgainstLimitsAsync(user.Id, month);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("Off-peak time minutes limit exceeded: 75/50", result.Violations);
        }

        [Fact]
        public async Task ValidateUsageAgainstLimitsAsync_MultipleViolations_ShouldReturnAllViolations()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hash",
                Role = UserRole.User,
                Name = "Test User",
                PhoneNumber = "1234567890",
                PlanType = "Basic",
                Country = "US"
            };

            var bundleLimit = new BundleLimit
            {
                PlanType = "Basic",
                VoiceMinutesLimit = 100,
                DataMBLimit = 100,
                SMSLimit = 50,
                PeakTimeMinutesLimit = 50,
                OffPeakTimeMinutesLimit = 50,
                IsActive = true
            };

            _context.Users.Add(user);
            _context.BundleLimits.Add(bundleLimit);
            await _context.SaveChangesAsync();

            // Add usage exceeding multiple limits
            var usageRecord = new UsageRecord
            {
                UserId = user.Id,
                Timestamp = new DateTime(2024, 1, 15, 10, 0, 0),
                CallMinutes = 150, // Exceeds voice limit
                DataMB = 200, // Exceeds data limit
                SMSCount = 75, // Exceeds SMS limit
                IsPeakTime = true,
                IsRoaming = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.UsageRecords.Add(usageRecord);
            await _context.SaveChangesAsync();

            var month = "2024-01";

            // Act
            var result = await _bundleLimitService.ValidateUsageAgainstLimitsAsync(user.Id, month);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal(4, result.Violations.Count); // Voice, Data, SMS, Peak time
            Assert.Contains("Voice minutes limit exceeded: 150/100", result.Violations);
            Assert.Contains("Data MB limit exceeded: 200/100", result.Violations);
            Assert.Contains("SMS limit exceeded: 75/50", result.Violations);
            Assert.Contains("Peak time minutes limit exceeded: 150/50", result.Violations);
        }

        [Fact]
        public async Task IsWithinBundleLimitsAsync_WithinLimits_ShouldReturnTrue()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hash",
                Role = UserRole.User,
                Name = "Test User",
                PhoneNumber = "1234567890",
                PlanType = "Basic",
                Country = "US"
            };

            var bundleLimit = new BundleLimit
            {
                PlanType = "Basic",
                VoiceMinutesLimit = 1000,
                DataMBLimit = 5000,
                SMSLimit = 1000,
                PeakTimeMinutesLimit = 500,
                OffPeakTimeMinutesLimit = 500,
                IsActive = true
            };

            _context.Users.Add(user);
            _context.BundleLimits.Add(bundleLimit);
            await _context.SaveChangesAsync();

            var month = "2024-01";

            // Act
            var result = await _bundleLimitService.IsWithinBundleLimitsAsync(user.Id, month, 100, 500, 50);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsWithinBundleLimitsAsync_ExceedsLimits_ShouldReturnFalse()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hash",
                Role = UserRole.User,
                Name = "Test User",
                PhoneNumber = "1234567890",
                PlanType = "Basic",
                Country = "US"
            };

            var bundleLimit = new BundleLimit
            {
                PlanType = "Basic",
                VoiceMinutesLimit = 100,
                DataMBLimit = 100,
                SMSLimit = 50,
                PeakTimeMinutesLimit = 50,
                OffPeakTimeMinutesLimit = 50,
                IsActive = true
            };

            _context.Users.Add(user);
            _context.BundleLimits.Add(bundleLimit);
            await _context.SaveChangesAsync();

            // Add existing usage that already exceeds limits
            var existingUsage = new UsageRecord
            {
                UserId = user.Id,
                Timestamp = new DateTime(2024, 1, 15, 10, 0, 0),
                CallMinutes = 60, // Already exceeds 100 limit when combined with additional 150
                DataMB = 80, // Already exceeds 100 limit when combined with additional 200
                SMSCount = 30, // Already exceeds 50 limit when combined with additional 75
                IsPeakTime = true,
                IsRoaming = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.UsageRecords.Add(existingUsage);
            await _context.SaveChangesAsync();

            var month = "2024-01";

            // Act
            var result = await _bundleLimitService.IsWithinBundleLimitsAsync(user.Id, month, 150, 200, 75);

            // Assert
            Assert.False(result);
        }
    }
}
