using Xunit;
using VehicleRental.Infrastructure.Services;

namespace VehicleRental.UnitTests
{
    public class PricingCalculatorTests
    {
        private readonly PricingCalculator _calculator;

        public PricingCalculatorTests()
        {
            _calculator = new PricingCalculator();
        }

        [Fact]
        public void CalculateRentalPrice_WithPositiveBatteryDelta_ShouldNotIncludeBatteryCost()
        {
            // Arrange
            decimal totalKilometers = 100;
            DateTime startDate = DateTime.UtcNow;
            DateTime endDate = startDate.AddDays(2);
            decimal pricePerKm = 0.30m;
            decimal pricePerDay = 45.00m;
            decimal? batteryDelta = 10;

            // Act
            var result = _calculator.CalculateRentalPrice(
                totalKilometers,
                startDate,
                endDate,
                pricePerKm,
                pricePerDay,
                batteryDelta);

            // Assert
            var expected = (totalKilometers * pricePerKm) + (2 * pricePerDay);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void CalculateRentalPrice_WithNegativeBatteryDelta_ShouldIncludeBatteryCost()
        {
            // Arrange
            decimal totalKilometers = 100;
            DateTime startDate = DateTime.UtcNow;
            DateTime endDate = startDate.AddDays(2);
            decimal pricePerKm = 0.30m;
            decimal pricePerDay = 45.00m;
            decimal? batteryDelta = -20;

            // Act
            var result = _calculator.CalculateRentalPrice(
                totalKilometers,
                startDate,
                endDate,
                pricePerKm,
                pricePerDay,
                batteryDelta);

            // Assert
            var expected = (totalKilometers * pricePerKm) +
                            (2 * pricePerDay) +
                          (Math.Abs(batteryDelta.Value) * 0.20m);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void CalculateRentalPrice_WithZeroBatteryDelta_ShouldNotIncludeBatteryCost()
        {
            // Arrange
            decimal totalKilometers = 100;
            DateTime startDate = DateTime.UtcNow;
            DateTime endDate = startDate.AddDays(2);
            decimal pricePerKm = 0.30m;
            decimal pricePerDay = 45.00m;
            decimal? batteryDelta = 0;

            // Act
            var result = _calculator.CalculateRentalPrice(
                totalKilometers,
                startDate,
                endDate,
                pricePerKm,
                pricePerDay,
                batteryDelta);

            // Assert
            var expected = (totalKilometers * pricePerKm) + (2 * pricePerDay);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void CalculateRentalPrice_WithNullBatteryDelta_ShouldNotIncludeBatteryCost()
        {
            // Arrange
            decimal totalKilometers = 100;
            DateTime startDate = DateTime.UtcNow;
            DateTime endDate = startDate.AddDays(2);
            decimal pricePerKm = 0.30m;
            decimal pricePerDay = 45.00m;
            decimal? batteryDelta = null;

            // Act
            var result = _calculator.CalculateRentalPrice(
                totalKilometers,
                startDate,
                endDate,
                pricePerKm,
                pricePerDay,
                batteryDelta);

            // Assert
            var expected = (totalKilometers * pricePerKm) + (2 * pricePerDay);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void CalculateRentalPrice_WithZeroKilometers_ShouldCalculateCorrectly()
        {
            // Arrange
            decimal totalKilometers = 0;
            DateTime startDate = DateTime.UtcNow;
            DateTime endDate = startDate.AddDays(2);
            decimal pricePerKm = 0.30m;
            decimal pricePerDay = 45.00m;
            decimal? batteryDelta = -10;

            // Act
            var result = _calculator.CalculateRentalPrice(
                totalKilometers,
                startDate,
                endDate,
                pricePerKm,
                pricePerDay,
                batteryDelta);

            // Assert
            var expected = (2 * pricePerDay) + (Math.Abs(batteryDelta.Value) * 0.20m);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void CalculateRentalPrice_WithOneDay_ShouldCalculateCorrectly()
        {
            // Arrange
            decimal totalKilometers = 100;
            DateTime startDate = DateTime.UtcNow;
            DateTime endDate = DateTime.UtcNow.AddMilliseconds(1);
            decimal pricePerKm = 0.30m;
            decimal pricePerDay = 45.00m;
            decimal? batteryDelta = -10;

            // Act
            var result = _calculator.CalculateRentalPrice(
                totalKilometers,
                startDate,
                endDate,
                pricePerKm,
                pricePerDay,
                batteryDelta);

            // Assert
            var expected = (1 * pricePerDay) + (totalKilometers * pricePerKm) + (Math.Abs(batteryDelta.Value) * 0.20m);
            Assert.Equal(expected, result);
        }
    }
}
