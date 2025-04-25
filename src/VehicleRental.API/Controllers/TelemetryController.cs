using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VehicleRental.Core.DTOs;
using VehicleRental.Core.Entities;
using VehicleRental.Core.Services;
using VehicleRental.Infrastructure.Data;

namespace VehicleRental.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TelemetryController : BaseController
    {
        private readonly ITelemetryService _telemetryService;
        private readonly VehicleRentalDbContext _context;
        private readonly ILogger<TelemetryController> _logger;

        public TelemetryController(
            ITelemetryService telemetryService,
            VehicleRentalDbContext context,
            ILogger<TelemetryController> logger)
        {
            _telemetryService = telemetryService;
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> ProcessTelemetry([FromBody] TelemetryRequest request)
        {
            var _isValid = true;
            var _validationMessage = "Valid";
            try
            {
                var vehicle = await _context.Vehicles
                    .FirstOrDefaultAsync(v => v.Id == request.VehicleId);

                var telemetryType = await _context.TelemetryTypes
                    .FirstOrDefaultAsync(t => t.Id == request.TelemetryTypeId);

                if (vehicle == null || telemetryType == null)
                {
                    _isValid = false;
                    _validationMessage = "Vehicle or telemetry type invalid:";
                    if (vehicle == null)
                    {
                        _validationMessage += " vehicle";
                    }
                    if (telemetryType == null)
                    {
                        _validationMessage += " telemetryType";
                    }
                }

                var telemetry = new Telemetry
                {
                    VehicleId = request.VehicleId,
                    TelemetryTypeId = request.TelemetryTypeId,
                    Value = request.Value,
                    Timestamp = DateTimeOffset.FromUnixTimeSeconds(request.Timestamp).UtcDateTime,
                    IsValid = _isValid,
                    ValidationMessage = _validationMessage,
                    Vehicle = vehicle,
                    TelemetryType = telemetryType
                };

                await _telemetryService.ProcessTelemetryAsync(telemetry);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing telemetry for vehicle {VehicleId}", request.VehicleId);
                return HandleError(ex, "ProcessTelemetry", "TELEMETRY_PROCESSING_ERROR");
            }
        }

        [HttpGet("vehicles/{vehicleId}/odometer")]
        public async Task<ActionResult<TelemetryResponse>> GetCurrentOdometer(int vehicleId)
        {
            try
            {
                var latestOdometer = await _context.Telemetry
                    .Include(t => t.TelemetryType)
                    .Include(t => t.Vehicle)
                    .Where(t => t.VehicleId == vehicleId &&
                               t.TelemetryType.Name.ToLower() == "odometer" &&
                               t.IsValid)
                    .OrderByDescending(t => t.Timestamp)
                    .FirstOrDefaultAsync();

                if (latestOdometer == null)
                {
                    return NotFound($"No odometer readings found for vehicle {vehicleId}");
                }

                var response = new TelemetryResponse
                {
                    Value = latestOdometer.Value,
                    Timestamp = ((DateTimeOffset)latestOdometer.Timestamp).ToUnixTimeSeconds(),
                    IsValid = latestOdometer.IsValid,
                    ValidationMessage = latestOdometer.ValidationMessage,
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving odometer for vehicle {VehicleId}", vehicleId);
                return HandleError<TelemetryResponse>(ex, "GetCurrentOdometer", "ODOMETER_RETRIEVAL_ERROR");
            }
        }

        [HttpGet("vehicles/{vehicleId}/batterysoc")]
        public async Task<ActionResult<TelemetryResponse>> GetCurrentBattery(int vehicleId)
        {
            try
            {
                var latestBattery = await _context.Telemetry
                    .Include(t => t.TelemetryType)
                    .Include(t => t.Vehicle)
                    .Where(t => t.VehicleId == vehicleId &&
                               t.TelemetryType.Name.ToLower() == "battery_soc" &&
                               t.IsValid)
                    .OrderByDescending(t => t.Timestamp)
                    .FirstOrDefaultAsync();

                if (latestBattery == null)
                {
                    return NotFound($"No battery SOC readings found for vehicle {vehicleId}");
                }

                var response = new TelemetryResponse
                {
                    Value = latestBattery.Value,
                    Timestamp = ((DateTimeOffset)latestBattery.Timestamp).ToUnixTimeSeconds(),
                    IsValid = latestBattery.IsValid,
                    ValidationMessage = latestBattery.ValidationMessage,
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving battery SOC for vehicle {VehicleId}", vehicleId);
                return HandleError<TelemetryResponse>(ex, "GetCurrentBatterySoc", "BATTERY_RETRIEVAL_ERROR");
            }
        }

        [HttpGet("vehicles/{vehicleId}/odometer/history")]
        public async Task<ActionResult<IEnumerable<TelemetryResponse>>> GetOdometerHistory(
            int vehicleId,
            [FromQuery] long? startDate = null,
            [FromQuery] long? endDate = null)
        {
            try
            {
                var query = _context.Telemetry
                    .Include(t => t.TelemetryType)
                    .Include(t => t.Vehicle)
                    .Where(t => t.VehicleId == vehicleId &&
                               t.TelemetryType.Name.ToLower() == "odometer" &&
                               t.IsValid);

                if (startDate.HasValue)
                {
                    var startDateTime = DateTimeOffset.FromUnixTimeSeconds(startDate.Value).UtcDateTime;
                    query = query.Where(t => t.Timestamp >= startDateTime);
                }

                if (endDate.HasValue)
                {
                    var endDateTime = DateTimeOffset.FromUnixTimeSeconds(endDate.Value).UtcDateTime;
                    query = query.Where(t => t.Timestamp <= endDateTime);
                }

                var history = await query
                    .OrderBy(t => t.Timestamp)
                    .Select(t => new TelemetryResponse
                    {
                        Value = t.Value,
                        Timestamp = ((DateTimeOffset)t.Timestamp).ToUnixTimeSeconds(),
                        IsValid = t.IsValid,
                        ValidationMessage = t.ValidationMessage
                    })
                    .ToListAsync();

                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving odometer history for vehicle {VehicleId}", vehicleId);
                return HandleError<IEnumerable<TelemetryResponse>>(ex, "GetOdometerHistory", "ODOMETER_HISTORY_RETRIEVAL_ERROR");
            }
        }
    }
}