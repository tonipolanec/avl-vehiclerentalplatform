using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VehicleRental.Core.Entities;
using VehicleRental.Core.Services;
using VehicleRental.Infrastructure.Data;

namespace VehicleRental.Infrastructure.Services
{
    public class TelemetryService : ITelemetryService
    {
        private readonly VehicleRentalDbContext _context;
        private readonly ILogger<TelemetryService> _logger;

        public TelemetryService(VehicleRentalDbContext context, ILogger<TelemetryService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task ProcessTelemetryAsync(Telemetry telemetry)
        {
            try
            {
                // Validate telemetry
                if (!telemetry.IsValid)
                {
                    _logger.LogWarning("Received invalid telemetry for vehicle {VehicleId}", telemetry.VehicleId);
                    return;
                }

                // Get telemetry type
                var telemetryType = await _context.TelemetryTypes
                    .FirstOrDefaultAsync(t => t.Id == telemetry.TelemetryTypeId);

                if (telemetryType == null)
                {
                    _logger.LogError("Telemetry type {TelemetryTypeId} not found", telemetry.TelemetryTypeId);
                    return;
                }

                // If this is an odometer reading, validate against the latest reading
                if (telemetryType.Name.ToLower() == "odometer")
                {
                    // Check if the odometer reading is positive
                    if (telemetry.Value < 0)
                    {
                        _logger.LogWarning("Received negative odometer reading {Value} for vehicle {VehicleId}", telemetry.Value, telemetry.VehicleId);
                        telemetry.IsValid = false;
                        telemetry.ValidationMessage = "Odometer reading cannot be negative";
                    } else {

                        var latestOdometer = await _context.Telemetry
                            .Include(t => t.TelemetryType)
                            .Where(t => t.VehicleId == telemetry.VehicleId &&
                                    t.TelemetryType.Name.ToLower() == "odometer" &&
                                    t.IsValid)
                            .OrderByDescending(t => t.Timestamp)
                            .FirstOrDefaultAsync();

                        if (latestOdometer != null && telemetry.Value < latestOdometer.Value)
                        {
                            _logger.LogWarning("Received odometer reading {NewValue} less than latest value {LatestValue} for vehicle {VehicleId}",
                                telemetry.Value, latestOdometer.Value, telemetry.VehicleId);
                            telemetry.IsValid = false;
                            telemetry.ValidationMessage = "Odometer reading cannot be less than latest value";
                        }
                    }
                }


                // Save telemetry reading
                telemetry.ProcessedAt = DateTime.UtcNow;
                await _context.Telemetry.AddAsync(telemetry);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Processed telemetry for vehicle {VehicleId}", telemetry.VehicleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing telemetry for vehicle {VehicleId}", telemetry.VehicleId);
                throw;
            }
        }

        public async Task<decimal> GetCurrentOdometerAsync(int vehicleId)
        {
            var latestOdometer = await _context.Telemetry
                .Include(t => t.TelemetryType)
                .Where(t => t.VehicleId == vehicleId &&
                           t.TelemetryType.Name.ToLower() == "odometer" &&
                           t.IsValid)
                .OrderByDescending(t => t.Timestamp)
                .FirstOrDefaultAsync();

            if (latestOdometer == null)
            {
                throw new KeyNotFoundException($"No odometer readings found for vehicle {vehicleId}");
            }

            return latestOdometer.Value;
        }

        public async Task<IEnumerable<Telemetry>> GetOdometerHistoryAsync(int vehicleId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Telemetry
                .Include(t => t.TelemetryType)
                .Where(t => t.VehicleId == vehicleId &&
                           t.TelemetryType.Name.ToLower() == "odometer" &&
                           t.IsValid);

            if (startDate.HasValue)
            {
                query = query.Where(t => t.Timestamp >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(t => t.Timestamp <= endDate.Value);
            }

            return await query
                .OrderBy(t => t.Timestamp)
                .ToListAsync();
        }

        public Task<decimal> GetCurrentBatteryLevelAsync(int vehicleId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Telemetry>> GetBatteryLevelHistoryAsync(int vehicleId, DateTime? startDate = null, DateTime? endDate = null)
        {
            throw new NotImplementedException();
        }
    }
}