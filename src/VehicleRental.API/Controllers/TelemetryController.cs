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
        public async Task<ActionResult<decimal>> GetCurrentOdometer(int vehicleId)
        {
            try
            {
                var odometer = await _telemetryService.GetCurrentOdometerAsync(vehicleId);
                return Ok(odometer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving odometer for vehicle {VehicleId}", vehicleId);
                return HandleError<decimal>(ex, "GetCurrentOdometer", "ODOMETER_RETRIEVAL_ERROR");
            }
        }

        [HttpGet("vehicles/{vehicleId}/odometer/history")]
        public async Task<ActionResult<IEnumerable<Telemetry>>> GetOdometerHistory(
            int vehicleId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var history = await _telemetryService.GetOdometerHistoryAsync(vehicleId, startDate, endDate);
                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving odometer history for vehicle {VehicleId}", vehicleId);
                return HandleError<IEnumerable<Telemetry>>(ex, "GetOdometerHistory", "ODOMETER_HISTORY_RETRIEVAL_ERROR");
            }
        }
    }
}