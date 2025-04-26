using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VehicleRental.Core.DTOs;
using VehicleRental.Core.Entities;
using VehicleRental.Core.Entities.Enums;
using VehicleRental.Core.Services;
using VehicleRental.Infrastructure.Data;
using VehicleRental.Infrastructure.Helpers;

namespace VehicleRental.Infrastructure.Services
{
    public class RentalService : IRentalService
    {
        private readonly VehicleRentalDbContext _context;
        private readonly ILogger<RentalService> _logger;
        private readonly IPricingCalculator _pricingCalculator;

        public RentalService(
            VehicleRentalDbContext context,
            ILogger<RentalService> logger,
            IPricingCalculator pricingCalculator)
        {
            _context = context;
            _logger = logger;
            _pricingCalculator = pricingCalculator;
        }

        public async Task<RentalAllDetailsResponse> CreateRentalAsync(CreateRentalRequest request)
        {
            var startDate = DateTimeOffset.FromUnixTimeSeconds(request.StartDate).UtcDateTime;
            var endDate = DateTimeOffset.FromUnixTimeSeconds(request.EndDate).UtcDateTime;

            // Check for overlapping rentals
            var hasOverlap = await _context.Rentals
                .AnyAsync(r => r.VehicleId == request.VehicleId &&
                              r.Status != RentalStatus.Cancelled &&
                              ((r.StartDate <= endDate && r.EndDate >= startDate) ||
                               (startDate <= r.EndDate && endDate >= r.StartDate)));

            if (hasOverlap)
                throw new InvalidOperationException("Vehicle is already rented during the requested period");


            var customer = await FetchHelpers.GetCustomerByIdAsync(_context, request.CustomerId);
            var vehicle = await FetchHelpers.GetVehicleByIdAsync(_context, request.VehicleId);


            var rental = new Rental
            {
                CustomerId = request.CustomerId,
                VehicleId = request.VehicleId,
                StartDate = startDate,
                EndDate = endDate,
                Status = RentalStatus.Ordered,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Customer = customer,
                Vehicle = vehicle
            };

            await _context.Rentals.AddAsync(rental);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created new rental with ID {RentalId}", rental.Id);

            return await GetRentalByIdAsync(rental.Id);
        }

        public async Task<RentalAllDetailsResponse> GetRentalByIdAsync(int id)
        {
            var rental = await FetchHelpers.GetRentalByIdAsync(_context, id);

            return RentalAllDetailsResponse.FromEntity(rental);
        }

        public async Task<IEnumerable<RentalBasicDetailsResponse>> GetAllRentalsAsync()
        {
            var rentals = await _context.Rentals
                .Include(r => r.Customer)
                .Include(r => r.Vehicle)
                .ToListAsync();

            return rentals.Select(RentalBasicDetailsResponse.FromEntity);
        }

        public async Task<RentalAllDetailsResponse> UpdateRentalAsync(int id, UpdateRentalDatesRequest request)
        {
            var rental = await FetchHelpers.GetRentalByIdAsync(_context, id);

            if (rental.Status == RentalStatus.Cancelled)
                throw new InvalidOperationException("Cannot update a cancelled rental");

            var newStartDate = request.StartDate.HasValue
                ? DateTimeOffset.FromUnixTimeSeconds(request.StartDate.Value).UtcDateTime
                : rental.StartDate;
            var newEndDate = request.EndDate.HasValue
                ? DateTimeOffset.FromUnixTimeSeconds(request.EndDate.Value).UtcDateTime
                : rental.EndDate;


            // Check for overlapping rentals excluding the current rental
            var hasOverlap = await _context.Rentals
                .AnyAsync(r => r.Id != id &&
                              r.Status != RentalStatus.Cancelled &&
                              ((r.StartDate <= newEndDate && r.EndDate >= newStartDate) ||
                               (newStartDate <= r.EndDate && newEndDate >= r.StartDate)));

            if (hasOverlap)
                throw new InvalidOperationException("Vehicle is already rented during the requested period");


            rental.StartDate = newStartDate;
            rental.EndDate = newEndDate;
            rental.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated rental with ID {RentalId}", rental.Id);

            return await GetRentalByIdAsync(rental.Id);
        }


        public async Task CancelRentalAsync(int id)
        {
            var rental = await FetchHelpers.GetRentalByIdAsync(_context, id);

            if (rental.Status == RentalStatus.Cancelled)
                throw new InvalidOperationException("Rental is already cancelled");

            rental.Status = RentalStatus.Cancelled;
            rental.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Cancelled rental with ID {RentalId}", rental.Id);
        }


        public async Task<RentalAllDetailsResponse> FinishRentalAsync(int id)
        {
            var rental = await FetchHelpers.GetRentalByIdAsync(_context, id);

            if (rental.Status != RentalStatus.Ordered)
            {
                throw new InvalidOperationException("Rental is not in ordered status");
            }

            var odometerTelemetry = await FetchHelpers.GetTelemetryTypeAsync(_context, "odometer");
            var batteryTelemetry = await FetchHelpers.GetTelemetryTypeAsync(_context, "battery_soc");

            var odometerReadingRentalStart = await FetchHelpers.GetTelemetryReadingsForRentalStartEndAsync(_context, rental.VehicleId, odometerTelemetry.Id, rental.StartDate, true);
            var odometerReadingRentalEnd = await FetchHelpers.GetTelemetryReadingsForRentalStartEndAsync(_context, rental.VehicleId, odometerTelemetry.Id, rental.EndDate, false);

            var batteryReadingRentalStart = await FetchHelpers.GetTelemetryReadingsForRentalStartEndAsync(_context, rental.VehicleId, batteryTelemetry.Id, rental.StartDate, true);
            var batteryReadingRentalEnd = await FetchHelpers.GetTelemetryReadingsForRentalStartEndAsync(_context, rental.VehicleId, batteryTelemetry.Id, rental.EndDate, false);


            rental.InitialOdometerReading = odometerReadingRentalStart.Value;
            rental.FinalOdometerReading = odometerReadingRentalEnd.Value;
            rental.InitialBatteryLevel = batteryReadingRentalStart.Value;
            rental.FinalBatteryLevel = batteryReadingRentalEnd.Value;


            var numberOfDays = (int)Math.Ceiling((rental.EndDate - rental.StartDate).TotalDays);


            var totalCost = _pricingCalculator.CalculateRentalPrice(
                totalKilometers: rental.TotalDistance,
                numberOfDays: numberOfDays,
                pricePerKm: rental.Vehicle.PricePerKmInEuro,
                pricePerDay: rental.Vehicle.PricePerDayInEuro,
                batteryDelta: rental.BatteryDelta
            );

            rental.Status = RentalStatus.Completed;
            rental.TotalCost = totalCost;
            rental.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Finished rental with ID {RentalId}", rental.Id);

            return await GetRentalByIdAsync(rental.Id);
        }
    }
}