using VehicleRental.Core.DTOs;
using VehicleRental.Core.Entities;

namespace VehicleRental.Core.Services
{
    public interface ITelemetryService
    {
        Task ProcessTelemetryAsync(TelemetryRequest telemetry);
        Task<TelemetryResponse> GetCurrentOdometerAsync(int vehicleId);
        Task<IEnumerable<Telemetry>> GetOdometerHistoryAsync(int vehicleId, DateTime? startDate = null, DateTime? endDate = null);
        Task<TelemetryResponse> GetCurrentBatteryAsync(int vehicleId);
        Task<IEnumerable<Telemetry>> GetBatteryHistoryAsync(int vehicleId, DateTime? startDate = null, DateTime? endDate = null);
    }
}