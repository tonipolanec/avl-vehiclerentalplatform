using VehicleRental.Core.DTOs;
using VehicleRental.Core.Entities;

namespace VehicleRental.Core.Services
{
    public interface IVehicleService
    {
        Task<IEnumerable<VehicleResponse>> GetAllVehiclesAsync();
        Task<VehicleResponse> GetVehicleByIdAsync(int id);
    }
}