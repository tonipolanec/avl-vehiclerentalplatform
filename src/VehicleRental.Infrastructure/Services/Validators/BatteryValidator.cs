using VehicleRental.Core.DTOs;
using VehicleRental.Core.Services;
using VehicleRental.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace VehicleRental.Infrastructure.Services.Validators
{
    public class BatteryValidator : ITelemetryValidator
    {
        public bool CanValidate(string telemetryType) => telemetryType.ToLower() == "battery_soc";

        public async Task<(bool IsValid, string Message)> ValidateAsync(TelemetryRequest request, VehicleRentalDbContext context)
        {
            if (request.Value < 0 || request.Value > 100)
            {
                return (false, "Battery level must be between 0 and 100 percent");
            }

            return (true, "Valid");
        }
    }
}