using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VehicleRental.Core.DTOs;
using VehicleRental.Core.Entities;
using VehicleRental.Core.Services;
using VehicleRental.Infrastructure.Data;
using VehicleRental.Infrastructure.Helpers;
using VehicleRental.Infrastructure.Interfaces;

namespace VehicleRental.Infrastructure.Services
{
    public class TelemetryService : ITelemetryService
    {
        private readonly VehicleRentalDbContext _context;
        private readonly ILogger<TelemetryService> _logger;
        private readonly IEnumerable<ITelemetryValidator> _validators;

        public TelemetryService(
            VehicleRentalDbContext context,
            ILogger<TelemetryService> logger,
            IEnumerable<ITelemetryValidator> validators)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _validators = validators ?? throw new ArgumentNullException(nameof(validators));
        }

        public async Task<TelemetryConfirmationResponse> ProcessTelemetryAsync(TelemetryRequest request)
        {
            var telemetryType = await FetchHelpers.GetTelemetryTypeByIdAsync(_context, request.TelemetryTypeId);
            var vehicle = await FetchHelpers.GetVehicleByIdAsync(_context, request.VehicleId);

            var validator = _validators.FirstOrDefault(v => v.CanValidate(telemetryType.Name));
            var (isValid, validationMessage) = validator != null
                ? await validator.ValidateAsync(request, _context)
                : (true, "Valid");

            var telemetry = new Telemetry
            {
                VehicleId = request.VehicleId,
                TelemetryTypeId = request.TelemetryTypeId,
                Value = request.Value,
                Timestamp = DateTimeOffset.FromUnixTimeSeconds(request.Timestamp).UtcDateTime,
                IsValid = isValid,
                ValidationMessage = validationMessage,
                Vehicle = vehicle,
                TelemetryType = telemetryType
            };
            telemetry.ProcessedAt = DateTime.UtcNow;

            // vehicle.TelemetryReadings.Add(telemetry);

            await _context.Telemetry.AddAsync(telemetry);
            await _context.SaveChangesAsync();

            return TelemetryConfirmationResponse.FromEntity(telemetry);
        }

        public async Task<TelemetryResponse> GetCurrentOdometerAsync(int vehicleId)
        {
            var odometerTelemetry = await FetchHelpers.GetTelemetryTypeAsync(_context, "odometer");
            var latestOdometer = await FetchHelpers.GetLatestTelemetryAsync(_context, vehicleId, odometerTelemetry.Id);

            return TelemetryResponse.FromEntity(latestOdometer);
        }

        public Task<IEnumerable<Telemetry>> GetOdometerHistoryAsync(int vehicleId, DateTime? startDate = null, DateTime? endDate = null)
        {
            throw new NotImplementedException();
        }

        public async Task<TelemetryResponse> GetCurrentBatteryAsync(int vehicleId)
        {
            var batteryTelemetry = await FetchHelpers.GetTelemetryTypeAsync(_context, "battery_soc");
            var latestBattery = await FetchHelpers.GetLatestTelemetryAsync(_context, vehicleId, batteryTelemetry.Id);

            return TelemetryResponse.FromEntity(latestBattery);
        }

        public Task<IEnumerable<Telemetry>> GetBatteryHistoryAsync(int vehicleId, DateTime? startDate = null, DateTime? endDate = null)
        {
            throw new NotImplementedException();
        }
    }
}