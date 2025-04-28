using VehicleRental.Application.DTOs;
using VehicleRental.Infrastructure.Data;

namespace VehicleRental.Infrastructure.Interfaces
{
    public interface IRentalValidator
    {
        Task<(bool IsValid, string Message)> ValidateAsync(CreateRentalRequest request, VehicleRentalDbContext context);
        Task<(bool IsValid, string Message)> ValidateUpdateAsync(int rentalId, UpdateRentalDatesRequest request, VehicleRentalDbContext context);
    }
}