using VehicleRental.Core.DTOs;
using VehicleRental.Core.Entities;

namespace VehicleRental.Core.Services
{
    public interface IRentalService
    {
        Task<RentalResponse> CreateRentalAsync(RentalRequest request);
        Task<RentalResponse> GetRentalByIdAsync(int id);
        Task<IEnumerable<RentalResponse>> GetAllRentalsAsync();
        Task<RentalResponse> UpdateRentalAsync(int id, RentalRequest request);
        // Task CompleteRentalAsync(int id, RentalRequest request);
        Task CancelRentalAsync(int id);
    }
}