using System;
using Xunit;
using VehicleRental.Core.Entities;
using VehicleRental.Core.Services;
using VehicleRental.Infrastructure.Services;
using VehicleRental.Infrastructure.Services.Validators;
using VehicleRental.Infrastructure.Interfaces;
using Moq;
using VehicleRental.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using VehicleRental.Core.DTOs;

namespace VehicleRental.UnitTests
{
    public class TelemetryTests
    {
        private readonly Mock<ILogger<TelemetryService>> _loggerMock;
        private readonly DbContextOptions<VehicleRentalDbContext> _options;
        private readonly VehicleRentalDbContext _context;

        public TelemetryTests()
        {
            _loggerMock = new Mock<ILogger<TelemetryService>>();
            _options = new DbContextOptionsBuilder<VehicleRentalDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new VehicleRentalDbContext(_options);
        }

        private Vehicle createTestVehicle() {
            return new Vehicle
            {
                VIN = "TEST123",
                Make = "Test",
                Model = "Car",
                Year = 2023,
                PricePerKmInEuro = 0.30m,
                PricePerDayInEuro = 45.00m,
                Rentals = new List<Rental>(),
                TelemetryReadings = new List<Telemetry>()
            };
        }

        private TelemetryType createOdometerTelemetryType() {
            return new TelemetryType
            {
                Name = "odometer",
                Description = "Odometer reading",
                Unit = "km",
                Readings = new List<Telemetry>()
            };
        }

        private TelemetryType createBatterySocTelemetryType() {
            return new TelemetryType
            {
                Name = "battery_soc",
                Description = "Battery State of Charge",
                Unit = "%",
                Readings = new List<Telemetry>()
            };
        }

        private List<ITelemetryValidator> createValidators() {
            return new List<ITelemetryValidator>
            {
                new OdometerValidator(),
                new BatteryValidator()
            };
        }

        [Fact]
        public async Task ProcessTelemetry_WithInvalidOdometer_ShouldMarkAsInvalid()
        {
            // Arrange
            var testVehicle = createTestVehicle();
            var odometerTelemetryType = createOdometerTelemetryType();

            await _context.Vehicles.AddAsync(testVehicle);
            await _context.TelemetryTypes.AddAsync(odometerTelemetryType);

            await _context.SaveChangesAsync();

            var validators = createValidators();
            var telemetryService = new TelemetryService(_context, _loggerMock.Object, validators);

            // Act
            var telemetryRequest = new TelemetryRequest
            {
                VehicleId = testVehicle.Id,
                TelemetryTypeId = odometerTelemetryType.Id,
                Value = -100, // Invalid negative value
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            await telemetryService.ProcessTelemetryAsync(telemetryRequest);

            // Assert
            var savedTelemetry = await _context.Telemetry
                .FirstOrDefaultAsync(t => t.VehicleId == testVehicle.Id &&
                                        t.TelemetryTypeId == odometerTelemetryType.Id);

            Assert.NotNull(savedTelemetry);
            Assert.False(savedTelemetry.IsValid);
            Assert.Contains("negative", savedTelemetry.ValidationMessage.ToLower());
        }

        [Fact]
        public async Task ProcessTelemetry_WithDecreasingOdometer_ShouldMarkAsInvalid()
        {
            // Arrange
            var testVehicle = createTestVehicle();
            var odometerTelemetryType = createOdometerTelemetryType();

            await _context.Vehicles.AddAsync(testVehicle);
            await _context.TelemetryTypes.AddAsync(odometerTelemetryType);

            // Add initial odometer reading
            var initialTelemetry = new Telemetry
            {
                VehicleId = testVehicle.Id,
                TelemetryTypeId = odometerTelemetryType.Id,
                Value = 1000,
                Timestamp = DateTime.UtcNow.AddHours(-1),
                IsValid = true,
                ValidationMessage = "Valid",
                Vehicle = testVehicle,
                TelemetryType = odometerTelemetryType
            };
            await _context.Telemetry.AddAsync(initialTelemetry);
            await _context.SaveChangesAsync();

            var validators = createValidators();
            var telemetryService = new TelemetryService(_context, _loggerMock.Object, validators);

            // Act
            var telemetryRequest = new TelemetryRequest
            {
                VehicleId = testVehicle.Id,
                TelemetryTypeId = odometerTelemetryType.Id,
                Value = 900, // Lower than previous reading
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            await telemetryService.ProcessTelemetryAsync(telemetryRequest);

            // Assert
            var savedTelemetry = await _context.Telemetry
                .FirstOrDefaultAsync(t => t.VehicleId == testVehicle.Id &&
                                        t.TelemetryTypeId == odometerTelemetryType.Id &&
                                        t.Value == 900);

            Assert.NotNull(savedTelemetry);
            Assert.False(savedTelemetry.IsValid);
            Assert.Contains("less than", savedTelemetry.ValidationMessage.ToLower());
        }

        [Fact]
        public async Task ProcessTelemetry_WithValidBatteryLevel_ShouldBeProcessed()
        {
            // Arrange
            var testVehicle = createTestVehicle();
            var batterySocTelemetryType = createBatterySocTelemetryType();

            await _context.Vehicles.AddAsync(testVehicle);
            await _context.TelemetryTypes.AddAsync(batterySocTelemetryType);

            await _context.SaveChangesAsync();

            var validators = createValidators();
            var telemetryService = new TelemetryService(_context, _loggerMock.Object, validators);

            // Act
            var telemetryRequest = new TelemetryRequest
            {
                VehicleId = testVehicle.Id,
                TelemetryTypeId = batterySocTelemetryType.Id,
                Value = 85,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            await telemetryService.ProcessTelemetryAsync(telemetryRequest);

            // Assert
            var savedTelemetry = await _context.Telemetry
                .FirstOrDefaultAsync(t => t.VehicleId == testVehicle.Id &&
                                        t.TelemetryTypeId == batterySocTelemetryType.Id &&
                                        t.Value == 85);

            Assert.NotNull(savedTelemetry);
            Assert.True(savedTelemetry.IsValid);
            Assert.Equal(85, savedTelemetry.Value);
        }
    }
}