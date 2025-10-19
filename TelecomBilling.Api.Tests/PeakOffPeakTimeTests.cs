using Microsoft.EntityFrameworkCore;
using TelecomBilling.Api.Data;
using TelecomBilling.Api.Services;

namespace TelecomBilling.Api.Tests
{
    public class PeakOffPeakTimeTests
    {
        private readonly BundleLimitService _bundleLimitService;
        private readonly TelecomBillingDbContext _context;

        public PeakOffPeakTimeTests()
        {
            var options = new DbContextOptionsBuilder<TelecomBillingDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new TelecomBillingDbContext(options);
            _bundleLimitService = new BundleLimitService(_context);
        }

        [Fact]
        public void IsPeakTime_WeekdayMorning_ShouldReturnTrue()
        {
            var timestamp = new DateTime(2024, 1, 15, 9, 0, 0);

            var result = _bundleLimitService.IsPeakTime(timestamp);

            Assert.True(result);
        }

        [Fact]
        public void IsPeakTime_WeekdayAfternoon_ShouldReturnTrue()
        {
            var timestamp = new DateTime(2024, 1, 15, 14, 30, 0);

            var result = _bundleLimitService.IsPeakTime(timestamp);

            Assert.True(result);
        }

        [Fact]
        public void IsPeakTime_WeekdayEvening_ShouldReturnFalse()
        {
            var timestamp = new DateTime(2024, 1, 15, 19, 0, 0);

            var result = _bundleLimitService.IsPeakTime(timestamp);

            Assert.False(result);
        }

        [Fact]
        public void IsPeakTime_WeekdayEarlyMorning_ShouldReturnFalse()
        {
            var timestamp = new DateTime(2024, 1, 15, 7, 0, 0);

            var result = _bundleLimitService.IsPeakTime(timestamp);

            Assert.False(result);
        }

        [Fact]
        public void IsPeakTime_Weekend_ShouldReturnFalse()
        {
            var timestamp = new DateTime(2024, 1, 13, 10, 0, 0);

            var result = _bundleLimitService.IsPeakTime(timestamp);

            Assert.False(result);
        }

        [Fact]
        public void IsPeakTime_WeekendEvening_ShouldReturnFalse()
        {
            var timestamp = new DateTime(2024, 1, 14, 15, 0, 0);

            var result = _bundleLimitService.IsPeakTime(timestamp);

            Assert.False(result);
        }

        [Theory]
        [InlineData(8, 0, true)]
        [InlineData(9, 30, true)]
        [InlineData(12, 0, true)]
        [InlineData(17, 59, true)]
        [InlineData(18, 0, false)]
        [InlineData(19, 0, false)]
        [InlineData(7, 59, false)]
        public void IsPeakTime_VariousHours_ShouldReturnExpectedResult(int hour, int minute, bool expected)
        {
            var timestamp = new DateTime(2024, 1, 15, hour, minute, 0);

            var result = _bundleLimitService.IsPeakTime(timestamp);

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(DayOfWeek.Monday, true)]
        [InlineData(DayOfWeek.Tuesday, true)]
        [InlineData(DayOfWeek.Wednesday, true)]
        [InlineData(DayOfWeek.Thursday, true)]
        [InlineData(DayOfWeek.Friday, true)]
        [InlineData(DayOfWeek.Saturday, false)]
        [InlineData(DayOfWeek.Sunday, false)]
        public void IsPeakTime_DifferentDaysOfWeek_ShouldReturnExpectedResult(DayOfWeek dayOfWeek, bool expected)
        {
            var timestamp = new DateTime(2024, 1, 15, 10, 0, 0);
            while (timestamp.DayOfWeek != dayOfWeek)
            {
                timestamp = timestamp.AddDays(1);
            }

            var result = _bundleLimitService.IsPeakTime(timestamp);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void IsPeakTime_EdgeCase_ExactPeakStart_ShouldReturnTrue()
        {
            var timestamp = new DateTime(2024, 1, 15, 8, 0, 0);

            var result = _bundleLimitService.IsPeakTime(timestamp);

            Assert.True(result);
        }

        [Fact]
        public void IsPeakTime_EdgeCase_ExactPeakEnd_ShouldReturnFalse()
        {
            var timestamp = new DateTime(2024, 1, 15, 18, 0, 0);

            var result = _bundleLimitService.IsPeakTime(timestamp);

            Assert.False(result);
        }
    }
}
