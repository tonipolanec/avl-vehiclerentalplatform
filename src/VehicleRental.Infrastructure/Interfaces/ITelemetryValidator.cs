using VehicleRental.Core.DTOs;
using VehicleRental.Infrastructure.Data;

namespace VehicleRental.Infrastructure.Interfaces
{
    public interface ITelemetryValidator
    {
        bool CanValidate(string telemetryType);
        Task<(bool IsValid, string Message)> ValidateAsync(TelemetryRequest request, VehicleRentalDbContext context);
    }
}