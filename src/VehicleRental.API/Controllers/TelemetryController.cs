using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VehicleRental.Core.DTOs;
using VehicleRental.Core.Entities;
using VehicleRental.Core.Services;
using VehicleRental.Infrastructure.Data;
using VehicleRental.Infrastructure.Helpers;

namespace VehicleRental.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TelemetryController : BaseController
    {
        private readonly ITelemetryService _telemetryService;
        private readonly VehicleRentalDbContext _context;

        public TelemetryController(
            ITelemetryService telemetryService,
            VehicleRentalDbContext context,
            ILogger<TelemetryController> logger) : base(logger)
        {
            _telemetryService = telemetryService;
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult<TelemetryConfirmationResponse>> ProcessTelemetry([FromBody] TelemetryRequest request)
        {
            try
            {
                var response = await _telemetryService.ProcessTelemetryAsync(request);
                if (response.Message.Contains("Valid")){
                    _logger.LogInformation("Telemetry processed successfully for vehicle {VehicleId}", request.VehicleId);
                    return Ok(response);
                } else {
                    _logger.LogError("Telemetry processing failed for vehicle {VehicleId}", request.VehicleId);
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                return HandleError(ex, "ProcessTelemetry", "TELEMETRY_PROCESSING_ERROR");
            }
        }

        [HttpGet("vehicles/{vehicleId}/odometer")]
        public async Task<ActionResult<TelemetryResponse>> GetCurrentOdometer(int vehicleId)
        {
            try
            {
                var currentOdometer = await _telemetryService.GetCurrentOdometerAsync(vehicleId);
                _logger.LogInformation("Current odometer retrieved for vehicle with id {VehicleId}", vehicleId);
                return Ok(currentOdometer);
            }
            catch (Exception ex)
            {
                return HandleError<TelemetryResponse>(ex, "GetCurrentOdometer", "ODOMETER_RETRIEVAL_ERROR");
            }
        }

        [HttpGet("vehicles/{vehicleId}/batterysoc")]
        public async Task<ActionResult<TelemetryResponse>> GetCurrentBattery(int vehicleId)
        {
            try
            {
                var currentBattery = await _telemetryService.GetCurrentBatteryAsync(vehicleId);
                _logger.LogInformation("Current battery retrieved for vehicle with id {VehicleId}", vehicleId);
                return Ok(currentBattery);
            }
            catch (Exception ex)
            {
                return HandleError<TelemetryResponse>(ex, "GetCurrentBatterySoc", "BATTERY_RETRIEVAL_ERROR");
            }
        }

        /* NotImplemented - Odometer history
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
                return HandleError<IEnumerable<TelemetryResponse>>(ex, "GetOdometerHistory", "ODOMETER_HISTORY_RETRIEVAL_ERROR");
            }
        }
        */
    }
}