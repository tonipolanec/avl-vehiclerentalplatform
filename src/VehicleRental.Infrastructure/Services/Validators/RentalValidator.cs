using Microsoft.EntityFrameworkCore;
using VehicleRental.Application.DTOs;
using VehicleRental.Core.Entities.Enums;
using VehicleRental.Infrastructure.Data;
using VehicleRental.Infrastructure.Interfaces;

namespace VehicleRental.Infrastructure.Services.Validators
{
    public class RentalValidator : IRentalValidator
    {
        public async Task<(bool IsValid, string Message)> ValidateAsync(CreateRentalRequest request, VehicleRentalDbContext context)
        {
            var startDate = DateTimeOffset.FromUnixTimeSeconds(request.StartDate).UtcDateTime;
            var endDate = DateTimeOffset.FromUnixTimeSeconds(request.EndDate).UtcDateTime;

            var hasOverlap = await context.Rentals
                .AnyAsync(r => r.VehicleId == request.VehicleId &&
                            r.Status == RentalStatus.Ordered &&
                              ((r.StartDate <= endDate && r.EndDate >= startDate) ||
                               (startDate <= r.EndDate && endDate >= r.StartDate)));

            if (hasOverlap)
                return (false, "Vehicle is already rented during the requested period");

            return (true, "Valid");
        }

        public async Task<(bool IsValid, string Message)> ValidateUpdateAsync(int rentalId, UpdateRentalDatesRequest request, VehicleRentalDbContext context)
        {
            var rental = await context.Rentals.FindAsync(rentalId);
            if (rental == null)
                return (false, "Rental not found");

            var newStartDate = request.StartDate.HasValue
                ? DateTimeOffset.FromUnixTimeSeconds(request.StartDate.Value).UtcDateTime
                : rental.StartDate;
            var newEndDate = request.EndDate.HasValue
                ? DateTimeOffset.FromUnixTimeSeconds(request.EndDate.Value).UtcDateTime
                : rental.EndDate;

            var hasOverlap = await context.Rentals
                .AnyAsync(r => r.Id != rentalId &&
                              r.Status == RentalStatus.Ordered &&
                              ((r.StartDate <= newEndDate && r.EndDate >= newStartDate) ||
                               (newStartDate <= r.EndDate && newEndDate >= r.StartDate)));

            if (hasOverlap)
                return (false, "Vehicle is already rented during the requested period");

            return (true, "Valid");
        }
    }
}