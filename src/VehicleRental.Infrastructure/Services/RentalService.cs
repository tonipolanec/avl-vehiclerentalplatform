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

        public async Task<RentalResponse> CreateRentalAsync(RentalRequest request)
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

        public async Task<RentalResponse> GetRentalByIdAsync(int id)
        {
            var rental = await _context.Rentals
                .Include(r => r.Customer)
                .Include(r => r.Vehicle)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rental == null)
            {
                throw new KeyNotFoundException($"Rental with ID {id} not found");
            }

            return RentalResponse.FromEntity(rental);
        }

        public async Task<IEnumerable<RentalResponse>> GetAllRentalsAsync()
        {
            var rentals = await _context.Rentals
                .Include(r => r.Customer)
                .Include(r => r.Vehicle)
                .ToListAsync();

            return rentals.Select(RentalResponse.FromEntity);
        }

        public async Task<RentalResponse> UpdateRentalAsync(int id, RentalRequest request)
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
                              r.VehicleId == request.VehicleId &&
                              r.Status != RentalStatus.Cancelled &&
                              ((r.StartDate <= request.EndDate && r.EndDate >= request.StartDate) ||
                               (request.StartDate <= r.EndDate && request.EndDate >= r.StartDate)));

            if (hasOverlap)
            {
                throw new InvalidOperationException("Vehicle is already rented during the requested period");
            }

            var numberOfDays = (int)Math.Ceiling((request.EndDate - request.StartDate).TotalDays);
            var totalCost = _pricingCalculator.CalculateRentalPrice(
                totalKilometers: rental.TotalDistance,
                numberOfDays: numberOfDays,
                pricePerKm: rental.Vehicle.PricePerKmInEuro,
                pricePerDay: rental.Vehicle.PricePerDayInEuro,
                batteryDelta: rental.FinalBatteryLevel - rental.InitialBatteryLevel
            );

            rental.CustomerId = request.CustomerId;
            rental.VehicleId = request.VehicleId;
            rental.StartDate = request.StartDate;
            rental.EndDate = request.EndDate;
            rental.InitialBatteryLevel = request.InitialBatteryLevel;
            rental.TotalCost = totalCost;
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
    }
}