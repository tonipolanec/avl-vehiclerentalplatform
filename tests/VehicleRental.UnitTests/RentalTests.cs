using System;
using Xunit;
using VehicleRental.Core.Entities;
using VehicleRental.Core.Entities.Enums;
using VehicleRental.Core.Services;
using VehicleRental.Infrastructure.Services;
using Moq;
using VehicleRental.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using VehicleRental.Core.DTOs;

namespace VehicleRental.UnitTests
{
    public class RentalTests
    {
        private readonly Mock<ILogger<RentalService>> _loggerMock;
        private readonly DbContextOptions<VehicleRentalDbContext> _options;
        private readonly VehicleRentalDbContext _context;

        public RentalTests()
        {
            _loggerMock = new Mock<ILogger<RentalService>>();
            _options = new DbContextOptionsBuilder<VehicleRentalDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new VehicleRentalDbContext(_options);
        }

        [Fact]
        public async Task CreateRental_WithOverlappingDates_ShouldThrowException()
        {
            // Arrange
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

            var rentalService = new RentalService(_context, _loggerMock.Object, new PricingCalculator());

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
        public async Task CalculateRentalPrice_ShouldReturnCorrectAmount()
        {
            // Arrange
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

            var rental = new Rental
            {
                VehicleId = vehicle.Id,
                CustomerId = customer.Id,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(2),
                Status = RentalStatus.Ordered,
                Vehicle = vehicle,
                Customer = customer,
                InitialOdometerReading = 1000,
                FinalOdometerReading = 1200,
                InitialBatteryLevel = 100,
                FinalBatteryLevel = 80
            };

            var pricingCalculator = new PricingCalculator();

            // Act
            var price = pricingCalculator.CalculateRentalPrice(
                totalKilometers: rental.TotalDistance,
                numberOfDays: 2,
                pricePerKm: vehicle.PricePerKmInEuro,
                pricePerDay: vehicle.PricePerDayInEuro,
                batteryDelta: rental.FinalBatteryLevel - rental.InitialBatteryLevel
            );

            // Assert
            var expectedPrice = (200 * 0.30m) + // kilometers
                              (2 * 45.00m) +    // days
                              (20 * 0.20m);     // battery delta
            Assert.Equal(expectedPrice, price);
        }

        [Fact]
        public async Task CancelRental_ShouldUpdateStatus()
        {
            // Arrange
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

            var rental = new Rental
            {
                VehicleId = vehicle.Id,
                CustomerId = customer.Id,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(2),
                Status = RentalStatus.Ordered,
                Vehicle = vehicle,
                Customer = customer
            };
            await _context.Rentals.AddAsync(rental);
            await _context.SaveChangesAsync();

            var rentalService = new RentalService(_context, _loggerMock.Object, new PricingCalculator());

            // Act
            await rentalService.CancelRentalAsync(rental.Id);

            // Assert
            var updatedRental = await _context.Rentals.FindAsync(rental.Id);
            Assert.Equal(RentalStatus.Cancelled, updatedRental?.Status);
        }
    }
}