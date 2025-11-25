using Microsoft.EntityFrameworkCore;
using TelecomBilling.Api.Data;
using TelecomBilling.Api.DTOs;
using TelecomBilling.Api.Models;
using TelecomBilling.Api.Services;

namespace TelecomBilling.Api.Tests
{
    public class ConsumptionServiceIntegrationTests
    {
        private readonly ConsumptionService _consumptionService;
        private readonly BundleLimitService _bundleLimitService;
        private readonly CostCalculationService _costCalculationService;
        private readonly TelecomBillingDbContext _context;

        public ConsumptionServiceIntegrationTests()
        {
            var options = new DbContextOptionsBuilder<TelecomBillingDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new TelecomBillingDbContext(options);
            _costCalculationService = new CostCalculationService(_context);
            _consumptionService = new ConsumptionService(_context, _costCalculationService);
            _bundleLimitService = new BundleLimitService(_context);
        }

        [Fact]
        public async Task GetUsageRecordsAsync_WithPeakOffPeakUsage_ShouldReturnCorrectRecords()
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

            // Add peak time usage
            var peakUsage = new UsageRecord
            {
                UserId = user.Id,
                Timestamp = new DateTime(2024, 1, 15, 10, 0, 0), // Peak time
                CallMinutes = 100,
                DataMB = 500,
                SMSCount = 25,
                IsPeakTime = true,
                IsRoaming = false,
                CreatedAt = DateTime.UtcNow
            };

            // Add off-peak time usage
            var offPeakUsage = new UsageRecord
            {
                UserId = user.Id,
                Timestamp = new DateTime(2024, 1, 15, 20, 0, 0), // Off-peak time
                CallMinutes = 50,
                DataMB = 300,
                SMSCount = 15,
                IsPeakTime = false,
                IsRoaming = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.UsageRecords.AddRange(peakUsage, offPeakUsage);
            await _context.SaveChangesAsync();

            // Act
            var result = await _consumptionService.GetUsageRecordsAsync(user.Id, 1, 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalCount);
            Assert.Equal(2, result.UsageRecords.Count);
            Assert.Equal(50, result.UsageRecords[0].CallMinutes); // Most recent first (20:00 is after 10:00)
            Assert.Equal(100, result.UsageRecords[1].CallMinutes);
            Assert.False(result.UsageRecords[0].IsPeakTime);
            Assert.True(result.UsageRecords[1].IsPeakTime);
        }

        [Fact]
        public async Task GetUsageRecordsAsync_WithRoamingUsage_ShouldReturnCorrectRecords()
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

            // Add domestic usage
            var domesticUsage = new UsageRecord
            {
                UserId = user.Id,
                Timestamp = new DateTime(2024, 1, 15, 10, 0, 0),
                CallMinutes = 100,
                DataMB = 500,
                SMSCount = 25,
                IsPeakTime = true,
                IsRoaming = false,
                CreatedAt = DateTime.UtcNow
            };

            // Add roaming usage
            var roamingUsage = new UsageRecord
            {
                UserId = user.Id,
                Timestamp = new DateTime(2024, 1, 15, 14, 0, 0),
                CallMinutes = 30,
                DataMB = 200,
                SMSCount = 10,
                IsPeakTime = true,
                IsRoaming = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.UsageRecords.AddRange(domesticUsage, roamingUsage);
            await _context.SaveChangesAsync();

            // Act
            var result = await _consumptionService.GetUsageRecordsAsync(user.Id, 1, 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalCount);
            Assert.Equal(2, result.UsageRecords.Count);
            Assert.Equal(30, result.UsageRecords[0].CallMinutes); // Most recent first
            Assert.Equal(100, result.UsageRecords[1].CallMinutes);
            Assert.True(result.UsageRecords[0].IsRoaming);
            Assert.False(result.UsageRecords[1].IsRoaming);
        }

        [Fact]
        public async Task GetUsageStatisticsAsync_WithMultipleUsers_ShouldCalculateCorrectly()
        {
            // Arrange
            var users = new List<User>
            {
                new User
                {
                    Id = 1,
                    Username = "user1",
                    Email = "user1@example.com",
                    PasswordHash = "hash",
                    Role = UserRole.User,
                    Name = "User 1",
                    PhoneNumber = "1111111111",
                    PlanType = "Basic",
                    Country = "US"
                },
                new User
                {
                    Id = 2,
                    Username = "user2",
                    Email = "user2@example.com",
                    PasswordHash = "hash",
                    Role = UserRole.User,
                    Name = "User 2",
                    PhoneNumber = "2222222222",
                    PlanType = "Premium",
                    Country = "US"
                }
            };

            _context.Users.AddRange(users);

            var usageRecords = new List<UsageRecord>
            {
                new UsageRecord
                {
                    UserId = 1,
                    Timestamp = new DateTime(2024, 1, 15, 10, 0, 0),
                    CallMinutes = 100,
                    DataMB = 500,
                    SMSCount = 25,
                    IsPeakTime = true,
                    IsRoaming = false,
                    CreatedAt = DateTime.UtcNow
                },
                new UsageRecord
                {
                    UserId = 2,
                    Timestamp = new DateTime(2024, 1, 15, 14, 0, 0),
                    CallMinutes = 150,
                    DataMB = 800,
                    SMSCount = 40,
                    IsPeakTime = true,
                    IsRoaming = false,
                    CreatedAt = DateTime.UtcNow
                },
                new UsageRecord
                {
                    UserId = 1,
                    Timestamp = new DateTime(2024, 1, 15, 20, 0, 0),
                    CallMinutes = 50,
                    DataMB = 300,
                    SMSCount = 15,
                    IsPeakTime = false,
                    IsRoaming = false,
                    CreatedAt = DateTime.UtcNow
                }
            };

            _context.UsageRecords.AddRange(usageRecords);
            await _context.SaveChangesAsync();

            // Act
            var result = await _consumptionService.GetUsageStatisticsAsync("2024-01");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("2024-01", result.Month);
            Assert.Equal(300, result.TotalCallMinutes); // 100 + 150 + 50
            Assert.Equal(1600, result.TotalDataMB); // 500 + 800 + 300
            Assert.Equal(80, result.TotalSMSCount); // 25 + 40 + 15
            Assert.Equal(250, result.PeakTimeMinutes); // 100 + 150
            Assert.Equal(50, result.OffPeakTimeMinutes);
            Assert.Equal(2, result.TotalSubscribers);
        }

        [Fact]
        public async Task GetTopConsumersAsync_ByVoice_ShouldReturnCorrectOrder()
        {
            // Arrange
            var users = new List<User>
            {
                new User
                {
                    Id = 1,
                    Username = "user1",
                    Email = "user1@example.com",
                    PasswordHash = "hash",
                    Role = UserRole.User,
                    Name = "User 1",
                    PhoneNumber = "1111111111",
                    PlanType = "Basic",
                    Country = "US"
                },
                new User
                {
                    Id = 2,
                    Username = "user2",
                    Email = "user2@example.com",
                    PasswordHash = "hash",
                    Role = UserRole.User,
                    Name = "User 2",
                    PhoneNumber = "2222222222",
                    PlanType = "Premium",
                    Country = "US"
                },
                new User
                {
                    Id = 3,
                    Username = "user3",
                    Email = "user3@example.com",
                    PasswordHash = "hash",
                    Role = UserRole.User,
                    Name = "User 3",
                    PhoneNumber = "3333333333",
                    PlanType = "Enterprise",
                    Country = "US"
                }
            };

            _context.Users.AddRange(users);

            var usageRecords = new List<UsageRecord>
            {
                new UsageRecord
                {
                    UserId = 1,
                    Timestamp = new DateTime(2024, 1, 15, 10, 0, 0),
                    CallMinutes = 100,
                    DataMB = 500,
                    SMSCount = 25,
                    IsPeakTime = true,
                    IsRoaming = false,
                    CreatedAt = DateTime.UtcNow
                },
                new UsageRecord
                {
                    UserId = 2,
                    Timestamp = new DateTime(2024, 1, 15, 14, 0, 0),
                    CallMinutes = 200, // Highest
                    DataMB = 800,
                    SMSCount = 40,
                    IsPeakTime = true,
                    IsRoaming = false,
                    CreatedAt = DateTime.UtcNow
                },
                new UsageRecord
                {
                    UserId = 3,
                    Timestamp = new DateTime(2024, 1, 15, 20, 0, 0),
                    CallMinutes = 150, // Middle
                    DataMB = 300,
                    SMSCount = 15,
                    IsPeakTime = false,
                    IsRoaming = false,
                    CreatedAt = DateTime.UtcNow
                }
            };

            _context.UsageRecords.AddRange(usageRecords);
            await _context.SaveChangesAsync();

            // Act
            var result = await _consumptionService.GetTopConsumersAsync("2024-01", 3, "voice");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("2024-01", result.Month);
            Assert.Equal("voice", result.SortBy);
            Assert.Equal(3, result.TopConsumers.Count);
            Assert.Equal(1, result.TopConsumers[0].Rank);
            Assert.Equal(2, result.TopConsumers[0].UserId);
            Assert.Equal(200, result.TopConsumers[0].TotalCallMinutes);
            Assert.Equal(2, result.TopConsumers[1].Rank);
            Assert.Equal(3, result.TopConsumers[1].UserId);
            Assert.Equal(150, result.TopConsumers[1].TotalCallMinutes);
            Assert.Equal(3, result.TopConsumers[2].Rank);
            Assert.Equal(1, result.TopConsumers[2].UserId);
            Assert.Equal(100, result.TopConsumers[2].TotalCallMinutes);
        }

        [Fact]
        public async Task GetTopConsumersAsync_ByData_ShouldReturnCorrectOrder()
        {
            // Arrange
            var users = new List<User>
            {
                new User
                {
                    Id = 1,
                    Username = "user1",
                    Email = "user1@example.com",
                    PasswordHash = "hash",
                    Role = UserRole.User,
                    Name = "User 1",
                    PhoneNumber = "1111111111",
                    PlanType = "Basic",
                    Country = "US"
                },
                new User
                {
                    Id = 2,
                    Username = "user2",
                    Email = "user2@example.com",
                    PasswordHash = "hash",
                    Role = UserRole.User,
                    Name = "User 2",
                    PhoneNumber = "2222222222",
                    PlanType = "Premium",
                    Country = "US"
                }
            };

            _context.Users.AddRange(users);

            var usageRecords = new List<UsageRecord>
            {
                new UsageRecord
                {
                    UserId = 1,
                    Timestamp = new DateTime(2024, 1, 15, 10, 0, 0),
                    CallMinutes = 100,
                    DataMB = 1000, // Highest
                    SMSCount = 25,
                    IsPeakTime = true,
                    IsRoaming = false,
                    CreatedAt = DateTime.UtcNow
                },
                new UsageRecord
                {
                    UserId = 2,
                    Timestamp = new DateTime(2024, 1, 15, 14, 0, 0),
                    CallMinutes = 200,
                    DataMB = 500, // Lower
                    SMSCount = 40,
                    IsPeakTime = true,
                    IsRoaming = false,
                    CreatedAt = DateTime.UtcNow
                }
            };

            _context.UsageRecords.AddRange(usageRecords);
            await _context.SaveChangesAsync();

            // Act
            var result = await _consumptionService.GetTopConsumersAsync("2024-01", 2, "data");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("data", result.SortBy);
            Assert.Equal(2, result.TopConsumers.Count);
            Assert.Equal(1, result.TopConsumers[0].Rank);
            Assert.Equal(1, result.TopConsumers[0].UserId);
            Assert.Equal(1000, result.TopConsumers[0].TotalDataMB);
            Assert.Equal(2, result.TopConsumers[1].Rank);
            Assert.Equal(2, result.TopConsumers[1].UserId);
            Assert.Equal(500, result.TopConsumers[1].TotalDataMB);
        }

        [Fact]
        public async Task GetTopConsumersAsync_BySMS_ShouldReturnCorrectOrder()
        {
            // Arrange
            var users = new List<User>
            {
                new User
                {
                    Id = 1,
                    Username = "user1",
                    Email = "user1@example.com",
                    PasswordHash = "hash",
                    Role = UserRole.User,
                    Name = "User 1",
                    PhoneNumber = "1111111111",
                    PlanType = "Basic",
                    Country = "US"
                },
                new User
                {
                    Id = 2,
                    Username = "user2",
                    Email = "user2@example.com",
                    PasswordHash = "hash",
                    Role = UserRole.User,
                    Name = "User 2",
                    PhoneNumber = "2222222222",
                    PlanType = "Premium",
                    Country = "US"
                }
            };

            _context.Users.AddRange(users);

            var usageRecords = new List<UsageRecord>
            {
                new UsageRecord
                {
                    UserId = 1,
                    Timestamp = new DateTime(2024, 1, 15, 10, 0, 0),
                    CallMinutes = 100,
                    DataMB = 500,
                    SMSCount = 50, // Higher
                    IsPeakTime = true,
                    IsRoaming = false,
                    CreatedAt = DateTime.UtcNow
                },
                new UsageRecord
                {
                    UserId = 2,
                    Timestamp = new DateTime(2024, 1, 15, 14, 0, 0),
                    CallMinutes = 200,
                    DataMB = 800,
                    SMSCount = 30, // Lower
                    IsPeakTime = true,
                    IsRoaming = false,
                    CreatedAt = DateTime.UtcNow
                }
            };

            _context.UsageRecords.AddRange(usageRecords);
            await _context.SaveChangesAsync();

            // Act
            var result = await _consumptionService.GetTopConsumersAsync("2024-01", 2, "sms");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("sms", result.SortBy);
            Assert.Equal(2, result.TopConsumers.Count);
            Assert.Equal(1, result.TopConsumers[0].Rank);
            Assert.Equal(1, result.TopConsumers[0].UserId);
            Assert.Equal(50, result.TopConsumers[0].TotalSMSCount);
            Assert.Equal(2, result.TopConsumers[1].Rank);
            Assert.Equal(2, result.TopConsumers[1].UserId);
            Assert.Equal(30, result.TopConsumers[1].TotalSMSCount);
        }

        [Fact]
        public async Task GetTopConsumersAsync_DefaultSort_ShouldReturnByCombinedUsage()
        {
            // Arrange
            var users = new List<User>
            {
                new User
                {
                    Id = 1,
                    Username = "user1",
                    Email = "user1@example.com",
                    PasswordHash = "hash",
                    Role = UserRole.User,
                    Name = "User 1",
                    PhoneNumber = "1111111111",
                    PlanType = "Basic",
                    Country = "US"
                },
                new User
                {
                    Id = 2,
                    Username = "user2",
                    Email = "user2@example.com",
                    PasswordHash = "hash",
                    Role = UserRole.User,
                    Name = "User 2",
                    PhoneNumber = "2222222222",
                    PlanType = "Premium",
                    Country = "US"
                }
            };

            _context.Users.AddRange(users);

            var usageRecords = new List<UsageRecord>
            {
                new UsageRecord
                {
                    UserId = 1,
                    Timestamp = new DateTime(2024, 1, 15, 10, 0, 0),
                    CallMinutes = 100,
                    DataMB = 500,
                    SMSCount = 25,
                    IsPeakTime = true,
                    IsRoaming = false,
                    CreatedAt = DateTime.UtcNow
                },
                new UsageRecord
                {
                    UserId = 2,
                    Timestamp = new DateTime(2024, 1, 15, 14, 0, 0),
                    CallMinutes = 200,
                    DataMB = 800,
                    SMSCount = 40,
                    IsPeakTime = true,
                    IsRoaming = false,
                    CreatedAt = DateTime.UtcNow
                }
            };

            _context.UsageRecords.AddRange(usageRecords);
            await _context.SaveChangesAsync();

            // Act
            var result = await _consumptionService.GetTopConsumersAsync("2024-01", 2, "total");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("total", result.SortBy);
            Assert.Equal(2, result.TopConsumers.Count);
            Assert.Equal(1, result.TopConsumers[0].Rank);
            Assert.Equal(2, result.TopConsumers[0].UserId);
            // User 2 has higher combined usage: 200 + 800 + 40 = 1040 vs 100 + 500 + 25 = 625
            Assert.Equal(2, result.TopConsumers[1].Rank);
            Assert.Equal(1, result.TopConsumers[1].UserId);
        }
    }
}
