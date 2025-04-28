using System;
using Xunit;
using VehicleRental.Core.Entities;
using VehicleRental.Core.Entities.Enums;
using VehicleRental.Application.Services;
using VehicleRental.Infrastructure.Services;
using Moq;
using VehicleRental.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using VehicleRental.Application.DTOs;
using VehicleRental.Infrastructure.Services.Validators;

namespace VehicleRental.UnitTests
{
    public class RentalTests
    {
        private readonly Mock<ILogger<RentalService>> _loggerMock;
        private readonly DbContextOptions<VehicleRentalDbContext> _options;
        private readonly VehicleRentalDbContext _context;
        private readonly RentalValidator _rentalValidator;

        public RentalTests()
        {
            _loggerMock = new Mock<ILogger<RentalService>>();
            _options = new DbContextOptionsBuilder<VehicleRentalDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new VehicleRentalDbContext(_options);
            _rentalValidator = new RentalValidator();
        }

        private async Task<(Vehicle vehicle, Customer customer)> SetupTestData()
        {
            var vehicle = new Vehicle
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
            await _context.Vehicles.AddAsync(vehicle);

            var customer = new Customer
            {
                Name = "Test Customer",
                Rentals = new List<Rental>()
            };
            await _context.Customers.AddAsync(customer);

            await _context.SaveChangesAsync();
            return (vehicle, customer);
        }

        [Fact]
        public async Task CreateRental_WithOverlappingDates_ShouldThrowException()
        {
            // Arrange
            var (vehicle, customer) = await SetupTestData();

            var existingRental = new Rental
            {
                VehicleId = vehicle.Id,
                CustomerId = customer.Id,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(2),
                Status = RentalStatus.Ordered,
                Vehicle = vehicle,
                Customer = customer
            };
            await _context.Rentals.AddAsync(existingRental);
            await _context.SaveChangesAsync();

            var rentalService = new RentalService(_context, _loggerMock.Object, new PricingCalculator(), _rentalValidator);

            // Act & Assert
            var newRentalRequest = new CreateRentalRequest
            {
                CustomerId = customer.Id,
                VehicleId = vehicle.Id,
                StartDate = DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds(),
                EndDate = DateTimeOffset.UtcNow.AddDays(3).ToUnixTimeSeconds()
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => rentalService.CreateRentalAsync(newRentalRequest));
        }

        [Fact]
        public async Task CreateRental_WithNonexistentVehicle_ShouldThrowException()
        {
            // Arrange
            var (_, customer) = await SetupTestData();
            var rentalService = new RentalService(_context, _loggerMock.Object, new PricingCalculator(), _rentalValidator);

            // Act & Assert
            var newRentalRequest = new CreateRentalRequest
            {
                CustomerId = customer.Id,
                VehicleId = 999, // Nonexistent vehicle ID
                StartDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                EndDate = DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds()
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => rentalService.CreateRentalAsync(newRentalRequest));
        }

        [Fact]
        public async Task CreateRental_WithNonexistentCustomer_ShouldThrowException()
        {
            // Arrange
            var (vehicle, _) = await SetupTestData();
            var rentalService = new RentalService(_context, _loggerMock.Object, new PricingCalculator(), _rentalValidator);

            // Act & Assert
            var newRentalRequest = new CreateRentalRequest
            {
                CustomerId = 999, // Nonexistent customer ID
                VehicleId = vehicle.Id,
                StartDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                EndDate = DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds()
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => rentalService.CreateRentalAsync(newRentalRequest));
        }

        [Fact]
        public async Task CreateRental_WithExactOverlap_ShouldThrowException()
        {
            // Arrange
            var (vehicle, customer) = await SetupTestData();

            var existingRental = new Rental
            {
                VehicleId = vehicle.Id,
                CustomerId = customer.Id,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(2),
                Status = RentalStatus.Ordered,
                Vehicle = vehicle,
                Customer = customer
            };
            await _context.Rentals.AddAsync(existingRental);
            await _context.SaveChangesAsync();

            var rentalService = new RentalService(_context, _loggerMock.Object, new PricingCalculator(), _rentalValidator);

            // Act & Assert
            var newRentalRequest = new CreateRentalRequest
            {
                CustomerId = customer.Id,
                VehicleId = vehicle.Id,
                StartDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                EndDate = DateTimeOffset.UtcNow.AddDays(2).ToUnixTimeSeconds()
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => rentalService.CreateRentalAsync(newRentalRequest));
        }

        [Fact]
        public async Task CreateRental_WithContainedOverlap_ShouldThrowException()
        {
            // Arrange
            var (vehicle, customer) = await SetupTestData();

            var existingRental = new Rental
            {
                VehicleId = vehicle.Id,
                CustomerId = customer.Id,
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(3),
                Status = RentalStatus.Ordered,
                Vehicle = vehicle,
                Customer = customer
            };
            await _context.Rentals.AddAsync(existingRental);
            await _context.SaveChangesAsync();

            var rentalService = new RentalService(_context, _loggerMock.Object, new PricingCalculator(), _rentalValidator);

            // Act & Assert
            var newRentalRequest = new CreateRentalRequest
            {
                CustomerId = customer.Id,
                VehicleId = vehicle.Id,
                StartDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                EndDate = DateTimeOffset.UtcNow.AddDays(4).ToUnixTimeSeconds()
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => rentalService.CreateRentalAsync(newRentalRequest));
        }

        [Fact]
        public async Task CreateRental_WithAdjacentDates_ShouldSucceed()
        {
            // Arrange
            var (vehicle, customer) = await SetupTestData();

            var existingRental = new Rental
            {
                VehicleId = vehicle.Id,
                CustomerId = customer.Id,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(2),
                Status = RentalStatus.Ordered,
                Vehicle = vehicle,
                Customer = customer
            };
            await _context.Rentals.AddAsync(existingRental);
            await _context.SaveChangesAsync();

            var rentalService = new RentalService(_context, _loggerMock.Object, new PricingCalculator(), _rentalValidator);

            // Act
            var newRentalRequest = new CreateRentalRequest
            {
                CustomerId = customer.Id,
                VehicleId = vehicle.Id,
                StartDate = DateTimeOffset.UtcNow.AddDays(2).AddMinutes(5).ToUnixTimeSeconds(),
                EndDate = DateTimeOffset.UtcNow.AddDays(4).ToUnixTimeSeconds()
            };

            var result = await rentalService.CreateRentalAsync(newRentalRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(customer.Id, result.CustomerId);
            Assert.Equal(vehicle.Id, result.VehicleId);
            Assert.Equal(RentalStatus.Ordered.ToString(), result.Status);
        }

        [Fact]
        public async Task FinishRental_ShouldUpdateAllParameters()
        {
            // Arrange
            var (vehicle, customer) = await SetupTestData();

            // Add telemetry data
            var odometerType = new TelemetryType
            {
                Name = "odometer",
                Description = "Odometer reading",
                Unit = "km",
                Readings = new List<Telemetry>()
            };
            await _context.TelemetryTypes.AddAsync(odometerType);

            var batteryType = new TelemetryType
            {
                Name = "battery_soc",
                Description = "Battery State of Charge",
                Unit = "%",
                Readings = new List<Telemetry>()
            };
            await _context.TelemetryTypes.AddAsync(batteryType);
            await _context.SaveChangesAsync();

            // Add telemetry readings
            var startTime = DateTime.UtcNow;
            var endTime = startTime.AddDays(2);

            var startOdometer = new Telemetry
            {
                VehicleId = vehicle.Id,
                TelemetryTypeId = odometerType.Id,
                Value = 1000,
                Timestamp = startTime,
                IsValid = true,
                ValidationMessage = "Valid",
                Vehicle = vehicle,
                TelemetryType = odometerType
            };

            var endOdometer = new Telemetry
            {
                VehicleId = vehicle.Id,
                TelemetryTypeId = odometerType.Id,
                Value = 1200,
                Timestamp = endTime,
                IsValid = true,
                ValidationMessage = "Valid",
                Vehicle = vehicle,
                TelemetryType = odometerType
            };

            var startBattery = new Telemetry
            {
                VehicleId = vehicle.Id,
                TelemetryTypeId = batteryType.Id,
                Value = 100,
                Timestamp = startTime,
                IsValid = true,
                ValidationMessage = "Valid",
                Vehicle = vehicle,
                TelemetryType = batteryType
            };

            var endBattery = new Telemetry
            {
                VehicleId = vehicle.Id,
                TelemetryTypeId = batteryType.Id,
                Value = 80,
                Timestamp = endTime,
                IsValid = true,
                ValidationMessage = "Valid",
                Vehicle = vehicle,
                TelemetryType = batteryType
            };

            await _context.Telemetry.AddRangeAsync(new[] { startOdometer, endOdometer, startBattery, endBattery });
            await _context.SaveChangesAsync();

            // Create rental
            var rental = new Rental
            {
                VehicleId = vehicle.Id,
                CustomerId = customer.Id,
                StartDate = startTime,
                EndDate = endTime,
                Status = RentalStatus.Ordered,
                Vehicle = vehicle,
                Customer = customer
            };
            await _context.Rentals.AddAsync(rental);
            await _context.SaveChangesAsync();

            var rentalService = new RentalService(_context, _loggerMock.Object, new PricingCalculator(), _rentalValidator);

            // Act
            var result = await rentalService.FinishRentalAsync(rental.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(RentalStatus.Completed.ToString(), result.Status);
            Assert.Equal(1000, result.InitialOdometerReading);
            Assert.Equal(1200, result.FinalOdometerReading);
            Assert.Equal(100, result.InitialBatteryLevel);
            Assert.Equal(80, result.FinalBatteryLevel);
            Assert.Equal(200, result.TotalDistance);
            Assert.True(result.TotalCost > 0);
        }

        [Fact]
        public async Task UpdateRental_WithOverlappingDates_ShouldThrowException()
        {
            // Arrange
            var (vehicle, customer) = await SetupTestData();

            // Create first rental
            var firstRental = new Rental
            {
                VehicleId = vehicle.Id,
                CustomerId = customer.Id,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(2),
                Status = RentalStatus.Ordered,
                Vehicle = vehicle,
                Customer = customer
            };
            await _context.Rentals.AddAsync(firstRental);

            // Create second rental
            var secondRental = new Rental
            {
                VehicleId = vehicle.Id,
                CustomerId = customer.Id,
                StartDate = DateTime.UtcNow.AddDays(3),
                EndDate = DateTime.UtcNow.AddDays(5),
                Status = RentalStatus.Ordered,
                Vehicle = vehicle,
                Customer = customer
            };
            await _context.Rentals.AddAsync(secondRental);
            await _context.SaveChangesAsync();

            var rentalService = new RentalService(_context, _loggerMock.Object, new PricingCalculator(), _rentalValidator);

            // Act & Assert
            var updateRequest = new UpdateRentalDatesRequest
            {
                StartDate = DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds(),
                EndDate = DateTimeOffset.UtcNow.AddDays(4).ToUnixTimeSeconds()
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => rentalService.UpdateRentalAsync(secondRental.Id, updateRequest));
        }

        [Fact]
        public async Task CancelRental_WhenAlreadyCancelled_ShouldThrowException()
        {
            // Arrange
            var (vehicle, customer) = await SetupTestData();

            var rental = new Rental
            {
                VehicleId = vehicle.Id,
                CustomerId = customer.Id,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(2),
                Status = RentalStatus.Cancelled,
                Vehicle = vehicle,
                Customer = customer
            };
            await _context.Rentals.AddAsync(rental);
            await _context.SaveChangesAsync();

            var rentalService = new RentalService(_context, _loggerMock.Object, new PricingCalculator(), _rentalValidator);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => rentalService.CancelRentalAsync(rental.Id));
        }

        [Fact]
        public async Task FinishRental_WhenNotInOrderedStatus_ShouldThrowException()
        {
            // Arrange
            var (vehicle, customer) = await SetupTestData();

            var rental = new Rental
            {
                VehicleId = vehicle.Id,
                CustomerId = customer.Id,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(2),
                Status = RentalStatus.Cancelled,
                Vehicle = vehicle,
                Customer = customer
            };
            await _context.Rentals.AddAsync(rental);
            await _context.SaveChangesAsync();

            var rentalService = new RentalService(_context, _loggerMock.Object, new PricingCalculator(), _rentalValidator);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => rentalService.FinishRentalAsync(rental.Id));
        }
    }
}