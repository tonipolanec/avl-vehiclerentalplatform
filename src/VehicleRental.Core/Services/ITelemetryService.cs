using VehicleRental.Core.Entities;

namespace VehicleRental.Core.Services
{
    public interface ITelemetryService
    {
        Task ProcessTelemetryAsync(Telemetry telemetry);
        Task<decimal> GetCurrentOdometerAsync(int vehicleId);
        Task<IEnumerable<Telemetry>> GetOdometerHistoryAsync(int vehicleId, DateTime? startDate = null, DateTime? endDate = null);
        Task<decimal> GetCurrentBatteryLevelAsync(int vehicleId);
        Task<IEnumerable<Telemetry>> GetBatteryLevelHistoryAsync(int vehicleId, DateTime? startDate = null, DateTime? endDate = null);
    }
}