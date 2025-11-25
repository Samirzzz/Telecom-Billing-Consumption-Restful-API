using Microsoft.EntityFrameworkCore;
using TelecomBilling.Api.Data;
using TelecomBilling.Api.Models;

namespace TelecomBilling.Api.Tests
{
    public static class TestHelper
    {
        public static TelecomBillingDbContext CreateInMemoryDbContext(string? databaseName = null)
        {
            var options = new DbContextOptionsBuilder<TelecomBillingDbContext>()
                .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
                .Options;

            return new TelecomBillingDbContext(options);
        }

        public static User CreateTestUser(int id = 1, string planType = "Basic")
        {
            return new User
            {
                Id = id,
                Username = $"user{id}",
                Email = $"user{id}@example.com",
                PasswordHash = "hashedpassword",
                Role = UserRole.User,
                Name = $"Test User {id}",
                PhoneNumber = $"123456789{id}",
                PlanType = planType,
                Country = "US",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
        }

        public static UsageRecord CreateTestUsageRecord(
            int userId,
            DateTime timestamp,
            int callMinutes = 100,
            int dataMB = 500,
            int smsCount = 25,
            bool isPeakTime = true,
            bool isRoaming = false)
        {
            return new UsageRecord
            {
                UserId = userId,
                Timestamp = timestamp,
                CallMinutes = callMinutes,
                DataMB = dataMB,
                SMSCount = smsCount,
                IsPeakTime = isPeakTime,
                IsRoaming = isRoaming,
                CreatedAt = DateTime.UtcNow
            };
        }

        public static BundleLimit CreateTestBundleLimit(string planType = "Basic")
        {
            return new BundleLimit
            {
                PlanType = planType,
                VoiceMinutesLimit = 1000,
                DataMBLimit = 5000,
                SMSLimit = 1000,
                PeakTimeMinutesLimit = 500,
                OffPeakTimeMinutesLimit = 500,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
        }

    }
}

