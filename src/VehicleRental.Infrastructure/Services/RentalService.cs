using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VehicleRental.Core.DTOs;
using VehicleRental.Core.Entities;
using VehicleRental.Core.Entities.Enums;
using VehicleRental.Core.Services;
using VehicleRental.Infrastructure.Data;

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
            // Check for overlapping rentals
            var hasOverlap = await _context.Rentals
                .AnyAsync(r => r.VehicleId == request.VehicleId &&
                              r.Status != RentalStatus.Cancelled &&
                              ((r.StartDate <= request.EndDate && r.EndDate >= request.StartDate) ||
                               (request.StartDate <= r.EndDate && request.EndDate >= r.StartDate)));

            if (hasOverlap)
            {
                throw new InvalidOperationException("Vehicle is already rented during the requested period");
            }

            var customer = await _context.Customers.FindAsync(request.CustomerId);
            var vehicle = await _context.Vehicles.FindAsync(request.VehicleId);

            if (customer == null || vehicle == null)
            {
                throw new InvalidOperationException("Customer or Vehicle not found");
            }

            // var numberOfDays = (int)Math.Ceiling((request.EndDate - request.StartDate).TotalDays);
            // var totalCost = _pricingCalculator.CalculateRentalPrice(
            //     totalKilometers: 0,
            //     numberOfDays: numberOfDays,
            //     pricePerKm: vehicle.PricePerKmInEuro,
            //     pricePerDay: vehicle.PricePerDayInEuro,
            //     batteryDelta: null
            // );

            var rental = new Rental
            {
                CustomerId = request.CustomerId,
                VehicleId = request.VehicleId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Status = RentalStatus.Ordered,
                InitialBatteryLevel = request.InitialBatteryLevel,
                InitialOdometerReading = request.InitialOdometerReading,
                TotalCost = 0m,
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
            var rental = await _context.Rentals
                .Include(r => r.Customer)
                .Include(r => r.Vehicle)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rental == null)
            {
                throw new KeyNotFoundException($"Rental with ID {id} not found");
            }

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
            var rental = await _context.Rentals
                .Include(r => r.Vehicle)
                .Include(r => r.Customer)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rental == null)
            {
                throw new KeyNotFoundException($"Rental with ID {id} not found");
            }

            if (rental.Status == RentalStatus.Cancelled)
            {
                throw new InvalidOperationException("Cannot update a cancelled rental");
            }



            // Check for overlapping rentals excluding the current rental
            var hasOverlap = await _context.Rentals
                .AnyAsync(r => r.Id != id &&
                              r.Status != RentalStatus.Cancelled &&
                              ((r.StartDate <= request.EndDate && r.EndDate >= request.StartDate) ||
                               (request.StartDate <= r.EndDate && request.EndDate >= r.StartDate)));

            if (hasOverlap)
            {
                throw new InvalidOperationException("Vehicle is already rented during the requested period");
            }

            rental.StartDate = request.StartDate;
            rental.EndDate = request.EndDate;
            rental.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated rental with ID {RentalId}", rental.Id);

            return await GetRentalByIdAsync(rental.Id);
        }


        public async Task CancelRentalAsync(int id)
        {
            var rental = await _context.Rentals
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rental == null)
            {
                throw new KeyNotFoundException($"Rental with ID {id} not found");
            }

            if (rental.Status == RentalStatus.Cancelled)
            {
                throw new InvalidOperationException("Rental is already cancelled");
            }

            rental.Status = RentalStatus.Cancelled;
            rental.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Cancelled rental with ID {RentalId}", rental.Id);
        }


        public async Task<RentalAllDetailsResponse> FinishRentalAsync(int id, FinishRentalRequest request)
        {
            var rental = await _context.Rentals
                .Include(r => r.Vehicle)
                .Include(r => r.Customer)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rental == null)
            {
                throw new KeyNotFoundException($"Rental with ID {id} not found");
            }

            if (rental.Status != RentalStatus.Ordered)
            {
                throw new InvalidOperationException("Rental is not in ordered status");
            }

            var numberOfDays = (int)Math.Ceiling((rental.EndDate - rental.StartDate).TotalDays);
            var totalCost = _pricingCalculator.CalculateRentalPrice(
                totalKilometers: rental.TotalDistance,
                numberOfDays: numberOfDays,
                pricePerKm: rental.Vehicle.PricePerKmInEuro,
                pricePerDay: rental.Vehicle.PricePerDayInEuro,
                batteryDelta: rental.FinalBatteryLevel - rental.InitialBatteryLevel
            );

            rental.Status = request.Status;
            rental.FinalBatteryLevel = request.FinalBatteryLevel;
            rental.FinalOdometerReading = request.FinalOdometerReading;
            rental.TotalCost = totalCost;
            rental.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Finished rental with ID {RentalId}", rental.Id);

            return await GetRentalByIdAsync(rental.Id);
        }
    }
}