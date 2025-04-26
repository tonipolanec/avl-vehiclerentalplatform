using VehicleRental.Core.DTOs;
using VehicleRental.Infrastructure.Interfaces;
using VehicleRental.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace VehicleRental.Infrastructure.Services.Validators
{
    public class OdometerValidator : ITelemetryValidator
    {
        public bool CanValidate(string telemetryType) => telemetryType.ToLower() == "odometer";

        public async Task<(bool IsValid, string Message)> ValidateAsync(TelemetryRequest request, VehicleRentalDbContext context)
        {
            if (request.Value < 0)
            {
                return (false, "Odometer reading cannot be negative");
            }

            var latestOdometer = await context.Telemetry
                .Include(t => t.TelemetryType)
                .Where(t => t.VehicleId == request.VehicleId &&
                        t.TelemetryType.Name.ToLower() == "odometer" &&
                        t.IsValid)
                .OrderByDescending(t => t.Timestamp)
                .FirstOrDefaultAsync();

            if (latestOdometer != null && request.Value < latestOdometer.Value)
            {
                return (false, $"Odometer reading cannot be less than latest value [{request.Value} < {latestOdometer.Value}]");
            }

            return (true, "Valid");
        }

    }
}