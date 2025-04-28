using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VehicleRental.Application.DTOs;
using VehicleRental.Application.Services;
using VehicleRental.Core.Entities;
using VehicleRental.Core.Entities.Enums;
using VehicleRental.Infrastructure.Data;
using VehicleRental.Infrastructure.Helpers;
using VehicleRental.Infrastructure.Interfaces;

namespace VehicleRental.Infrastructure.Services
{
    public class RentalService : IRentalService
    {
        private readonly VehicleRentalDbContext _context;
        private readonly ILogger<RentalService> _logger;
        private readonly IPricingCalculator _pricingCalculator;
        private readonly IRentalValidator _rentalValidator;

        public RentalService(
            VehicleRentalDbContext context,
            ILogger<RentalService> logger,
            IPricingCalculator pricingCalculator,
            IRentalValidator rentalValidator)
        {
            _context = context;
            _logger = logger;
            _pricingCalculator = pricingCalculator;
            _rentalValidator = rentalValidator;
        }

        public async Task<RentalAllDetailsResponse> CreateRentalAsync(CreateRentalRequest request)
        {
            var (notOverlapping, message) = await _rentalValidator.ValidateAsync(request, _context);
            if (!notOverlapping)
                throw new InvalidOperationException(message);

            var customer = await FetchHelpers.GetCustomerByIdAsync(_context, request.CustomerId);
            var vehicle = await FetchHelpers.GetVehicleByIdAsync(_context, request.VehicleId);

            var rental = new Rental
            {
                CustomerId = request.CustomerId,
                VehicleId = request.VehicleId,
                StartDate = DateTimeOffset.FromUnixTimeSeconds(request.StartDate).UtcDateTime,
                EndDate = DateTimeOffset.FromUnixTimeSeconds(request.EndDate).UtcDateTime,
                Status = RentalStatus.Ordered,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Customer = customer,
                Vehicle = vehicle
            };

            //vehicle.Rentals.Add(rental);

            await _context.Rentals.AddAsync(rental);
            await _context.SaveChangesAsync();


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
            var (notOverlapping, message) = await _rentalValidator.ValidateUpdateAsync(id, request, _context);
            if (!notOverlapping)
                throw new InvalidOperationException(message);

            var rental = await FetchHelpers.GetRentalByIdAsync(_context, id);

            rental.StartDate = request.StartDate.HasValue
                ? DateTimeOffset.FromUnixTimeSeconds(request.StartDate.Value).UtcDateTime
                : rental.StartDate;
            rental.EndDate = request.EndDate.HasValue
                ? DateTimeOffset.FromUnixTimeSeconds(request.EndDate.Value).UtcDateTime
                : rental.EndDate;
            rental.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

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


            var totalCost = _pricingCalculator.CalculateRentalPrice(
                totalKilometers: rental.TotalDistance,
                startDate: rental.StartDate,
                endDate: rental.EndDate,
                pricePerKm: rental.Vehicle.PricePerKmInEuro,
                pricePerDay: rental.Vehicle.PricePerDayInEuro,
                batteryDelta: rental.BatteryDelta
            );

            rental.Status = RentalStatus.Completed;
            rental.TotalCost = totalCost;
            rental.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetRentalByIdAsync(rental.Id);
        }

        public async Task DeleteRentalAsync(int id)
        {
            var rental = await FetchHelpers.GetRentalByIdAsync(_context, id);
            _context.Rentals.Remove(rental);
            await _context.SaveChangesAsync();
        }
    }
}