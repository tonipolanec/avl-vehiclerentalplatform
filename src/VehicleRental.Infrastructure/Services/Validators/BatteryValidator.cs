using VehicleRental.Application.DTOs;
using VehicleRental.Infrastructure.Interfaces;
using VehicleRental.Infrastructure.Data;

namespace VehicleRental.Infrastructure.Services.Validators
{
    public class BatteryValidator : ITelemetryValidator
    {
        public bool CanValidate(string telemetryType) => telemetryType.ToLower() == "battery_soc";

        public Task<(bool IsValid, string Message)> ValidateAsync(TelemetryRequest request, VehicleRentalDbContext context)
        {
            if (request.Value < 0 || request.Value > 100)
            {
                return Task.FromResult((false, "Battery level must be between 0 and 100 percent"));
            }

            return Task.FromResult((true, "Valid"));
        }
    }
}