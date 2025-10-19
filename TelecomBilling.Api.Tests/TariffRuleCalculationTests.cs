using Microsoft.EntityFrameworkCore;
using TelecomBilling.Api.Data;
using TelecomBilling.Api.DTOs;
using TelecomBilling.Api.Models;
using TelecomBilling.Api.Services;

namespace TelecomBilling.Api.Tests
{
    public class TariffRuleCalculationTests
    {
        private readonly TariffRuleService _tariffRuleService;
        private readonly TelecomBillingDbContext _context;

        public TariffRuleCalculationTests()
        {
            var options = new DbContextOptionsBuilder<TelecomBillingDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new TelecomBillingDbContext(options);
            _tariffRuleService = new TariffRuleService(_context);
        }

        [Fact]
        public async Task CreateTariffRuleAsync_ValidRequest_ShouldCreateSuccessfully()
        {
            // Arrange
            var request = new TariffRuleRequest
            {
                Name = "Basic Plan Tariff",
                PlanType = "Basic",
                VoicePeakRate = 0.10m,
                VoiceOffPeakRate = 0.05m,
                DataRate = 0.02m,
                SMSRate = 0.15m,
                RoamingVoiceRate = 0.25m,
                RoamingDataRate = 0.08m,
                RoamingSMSRate = 0.30m,
                IsActive = true
            };

            // Act
            var result = await _tariffRuleService.CreateTariffRuleAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(request.Name, result.Name);
            Assert.Equal(request.PlanType, result.PlanType);
            Assert.Equal(request.VoicePeakRate, result.VoicePeakRate);
            Assert.Equal(request.VoiceOffPeakRate, result.VoiceOffPeakRate);
            Assert.Equal(request.DataRate, result.DataRate);
            Assert.Equal(request.SMSRate, result.SMSRate);
            Assert.Equal(request.RoamingVoiceRate, result.RoamingVoiceRate);
            Assert.Equal(request.RoamingDataRate, result.RoamingDataRate);
            Assert.Equal(request.RoamingSMSRate, result.RoamingSMSRate);
            Assert.True(result.IsActive);
        }

        [Fact]
        public async Task GetActiveTariffRuleAsync_ExistingPlanType_ShouldReturnRule()
        {
            // Arrange
            var tariffRule = new TariffRule
            {
                Name = "Premium Plan Tariff",
                PlanType = "Premium",
                VoicePeakRate = 0.08m,
                VoiceOffPeakRate = 0.04m,
                DataRate = 0.015m,
                SMSRate = 0.12m,
                RoamingVoiceRate = 0.20m,
                RoamingDataRate = 0.06m,
                RoamingSMSRate = 0.25m,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.TariffRules.Add(tariffRule);
            await _context.SaveChangesAsync();

            // Act
            var result = await _tariffRuleService.GetActiveTariffRuleAsync("Premium");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Premium", result.PlanType);
            Assert.Equal(0.08m, result.VoicePeakRate);
            Assert.Equal(0.04m, result.VoiceOffPeakRate);
        }

        [Fact]
        public async Task GetActiveTariffRuleAsync_NonExistentPlanType_ShouldReturnNull()
        {
            // Act
            var result = await _tariffRuleService.GetActiveTariffRuleAsync("NonExistent");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetActiveTariffRuleAsync_InactiveRule_ShouldReturnNull()
        {
            // Arrange
            var tariffRule = new TariffRule
            {
                Name = "Inactive Plan Tariff",
                PlanType = "Inactive",
                VoicePeakRate = 0.10m,
                VoiceOffPeakRate = 0.05m,
                DataRate = 0.02m,
                SMSRate = 0.15m,
                RoamingVoiceRate = 0.25m,
                RoamingDataRate = 0.08m,
                RoamingSMSRate = 0.30m,
                IsActive = false, // Inactive
                CreatedAt = DateTime.UtcNow
            };

            _context.TariffRules.Add(tariffRule);
            await _context.SaveChangesAsync();

            // Act
            var result = await _tariffRuleService.GetActiveTariffRuleAsync("Inactive");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateTariffRuleAsync_ExistingRule_ShouldUpdateSuccessfully()
        {
            // Arrange
            var tariffRule = new TariffRule
            {
                Name = "Original Tariff",
                PlanType = "Basic",
                VoicePeakRate = 0.10m,
                VoiceOffPeakRate = 0.05m,
                DataRate = 0.02m,
                SMSRate = 0.15m,
                RoamingVoiceRate = 0.25m,
                RoamingDataRate = 0.08m,
                RoamingSMSRate = 0.30m,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.TariffRules.Add(tariffRule);
            await _context.SaveChangesAsync();

            var updateRequest = new TariffRuleRequest
            {
                Name = "Updated Tariff",
                PlanType = "Basic",
                VoicePeakRate = 0.12m, // Increased
                VoiceOffPeakRate = 0.06m, // Increased
                DataRate = 0.025m, // Increased
                SMSRate = 0.18m, // Increased
                RoamingVoiceRate = 0.30m, // Increased
                RoamingDataRate = 0.10m, // Increased
                RoamingSMSRate = 0.35m, // Increased
                IsActive = true
            };

            // Act
            var result = await _tariffRuleService.UpdateTariffRuleAsync(tariffRule.Id, updateRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Tariff", result.Name);
            Assert.Equal(0.12m, result.VoicePeakRate);
            Assert.Equal(0.06m, result.VoiceOffPeakRate);
            Assert.Equal(0.025m, result.DataRate);
            Assert.Equal(0.18m, result.SMSRate);
            Assert.Equal(0.30m, result.RoamingVoiceRate);
            Assert.Equal(0.10m, result.RoamingDataRate);
            Assert.Equal(0.35m, result.RoamingSMSRate);
            Assert.NotNull(result.LastUpdated);
        }

        [Fact]
        public async Task UpdateTariffRuleAsync_NonExistentRule_ShouldReturnNull()
        {
            // Arrange
            var updateRequest = new TariffRuleRequest
            {
                Name = "Non-existent Tariff",
                PlanType = "Basic",
                VoicePeakRate = 0.10m,
                VoiceOffPeakRate = 0.05m,
                DataRate = 0.02m,
                SMSRate = 0.15m,
                RoamingVoiceRate = 0.25m,
                RoamingDataRate = 0.08m,
                RoamingSMSRate = 0.30m,
                IsActive = true
            };

            // Act
            var result = await _tariffRuleService.UpdateTariffRuleAsync(999, updateRequest);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteTariffRuleAsync_ExistingRule_ShouldDeleteSuccessfully()
        {
            // Arrange
            var tariffRule = new TariffRule
            {
                Name = "To Delete",
                PlanType = "Basic",
                VoicePeakRate = 0.10m,
                VoiceOffPeakRate = 0.05m,
                DataRate = 0.02m,
                SMSRate = 0.15m,
                RoamingVoiceRate = 0.25m,
                RoamingDataRate = 0.08m,
                RoamingSMSRate = 0.30m,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.TariffRules.Add(tariffRule);
            await _context.SaveChangesAsync();

            // Act
            var result = await _tariffRuleService.DeleteTariffRuleAsync(tariffRule.Id);

            // Assert
            Assert.True(result);
            var deletedRule = await _context.TariffRules.FindAsync(tariffRule.Id);
            Assert.Null(deletedRule);
        }

        [Fact]
        public async Task DeleteTariffRuleAsync_NonExistentRule_ShouldReturnFalse()
        {
            // Act
            var result = await _tariffRuleService.DeleteTariffRuleAsync(999);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetTariffRulesAsync_WithPagination_ShouldReturnCorrectResults()
        {
            // Arrange
            var tariffRules = new List<TariffRule>
            {
                new TariffRule
                {
                    Name = "Basic Plan",
                    PlanType = "Basic",
                    VoicePeakRate = 0.10m,
                    VoiceOffPeakRate = 0.05m,
                    DataRate = 0.02m,
                    SMSRate = 0.15m,
                    RoamingVoiceRate = 0.25m,
                    RoamingDataRate = 0.08m,
                    RoamingSMSRate = 0.30m,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new TariffRule
                {
                    Name = "Premium Plan",
                    PlanType = "Premium",
                    VoicePeakRate = 0.08m,
                    VoiceOffPeakRate = 0.04m,
                    DataRate = 0.015m,
                    SMSRate = 0.12m,
                    RoamingVoiceRate = 0.20m,
                    RoamingDataRate = 0.06m,
                    RoamingSMSRate = 0.25m,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new TariffRule
                {
                    Name = "Enterprise Plan",
                    PlanType = "Enterprise",
                    VoicePeakRate = 0.06m,
                    VoiceOffPeakRate = 0.03m,
                    DataRate = 0.01m,
                    SMSRate = 0.10m,
                    RoamingVoiceRate = 0.15m,
                    RoamingDataRate = 0.04m,
                    RoamingSMSRate = 0.20m,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            _context.TariffRules.AddRange(tariffRules);
            await _context.SaveChangesAsync();

            // Act
            var result = await _tariffRuleService.GetTariffRulesAsync(1, 2);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.TotalCount);
            Assert.Equal(1, result.PageNumber);
            Assert.Equal(2, result.PageSize);
            Assert.Equal(2, result.TariffRules.Count);
            Assert.Equal("Basic", result.TariffRules[0].PlanType);
            Assert.Equal("Enterprise", result.TariffRules[1].PlanType);
        }

        [Theory]
        [InlineData("Basic", 0.10, 0.05, 0.02, 0.15)]
        [InlineData("Premium", 0.08, 0.04, 0.015, 0.12)]
        [InlineData("Enterprise", 0.06, 0.03, 0.01, 0.10)]
        public async Task CreateTariffRuleAsync_DifferentPlanTypes_ShouldCreateWithCorrectRates(
            string planType, decimal voicePeak, decimal voiceOffPeak, decimal dataRate, decimal smsRate)
        {
            // Arrange
            var request = new TariffRuleRequest
            {
                Name = $"{planType} Plan Tariff",
                PlanType = planType,
                VoicePeakRate = voicePeak,
                VoiceOffPeakRate = voiceOffPeak,
                DataRate = dataRate,
                SMSRate = smsRate,
                RoamingVoiceRate = voicePeak * 2.5m,
                RoamingDataRate = dataRate * 4m,
                RoamingSMSRate = smsRate * 2m,
                IsActive = true
            };

            // Act
            var result = await _tariffRuleService.CreateTariffRuleAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(planType, result.PlanType);
            Assert.Equal(voicePeak, result.VoicePeakRate);
            Assert.Equal(voiceOffPeak, result.VoiceOffPeakRate);
            Assert.Equal(dataRate, result.DataRate);
            Assert.Equal(smsRate, result.SMSRate);
        }
    }
}

