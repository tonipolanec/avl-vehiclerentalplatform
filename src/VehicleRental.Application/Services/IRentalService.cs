using VehicleRental.Application.DTOs;
using VehicleRental.Core.Entities;

namespace VehicleRental.Application.Services
{
    public interface IRentalService
    {
        Task<RentalAllDetailsResponse> CreateRentalAsync(CreateRentalRequest request);
        Task<RentalAllDetailsResponse> GetRentalByIdAsync(int id);
        Task<IEnumerable<RentalBasicDetailsResponse>> GetAllRentalsAsync();
        Task<RentalAllDetailsResponse> UpdateRentalAsync(int id, UpdateRentalDatesRequest request);
        Task<RentalAllDetailsResponse> FinishRentalAsync(int id);
        Task CancelRentalAsync(int id);
        Task DeleteRentalAsync(int id);
    }
}