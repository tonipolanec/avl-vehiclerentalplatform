using Microsoft.AspNetCore.Mvc;
using VehicleRental.Core.DTOs;
using VehicleRental.Core.Services;

namespace VehicleRental.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VehiclesController : BaseController
    {
        private readonly IVehicleService _vehicleService;

        public VehiclesController(IVehicleService vehicleService, ILogger<VehiclesController> logger) : base(logger)
        {
            _vehicleService = vehicleService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<VehicleResponse>> GetVehicle(int id)
        {
            try
            {
                var vehicle = await _vehicleService.GetVehicleByIdAsync(id);
                _logger.LogInformation("Vehicle retrieved with id {VehicleId}", id);
                return Ok(vehicle);
            }
            catch (Exception ex)
            {
                return HandleError<VehicleResponse>(ex, "GetVehicle", "VEHICLE_RETRIEVAL_ERROR");
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<VehicleResponse>>> GetAllVehicles()
        {
            try
            {
                var vehicles = await _vehicleService.GetAllVehiclesAsync();
                _logger.LogInformation("All vehicles retrieved with count {VehicleCount}", vehicles.Count());
                return Ok(vehicles);
            }
            catch (Exception ex)
            {
                return HandleError<IEnumerable<VehicleResponse>>(ex, "GetAllVehicles", "VEHICLES_RETRIEVAL_ERROR");
            }
        }

        [HttpPost]
        public async Task<ActionResult<VehicleResponse>> CreateVehicle(CreateVehicleRequest request)
        {
            try
            {
                var vehicle = await _vehicleService.CreateVehicleAsync(request);
                _logger.LogInformation("Vehicle created with id {VehicleId}", vehicle.Id);
                return CreatedAtAction(nameof(GetVehicle), new { id = vehicle.Id }, vehicle);
            }
            catch (Exception ex)
            {
                return HandleError<VehicleResponse>(ex, "CreateVehicle", "VEHICLE_CREATION_ERROR");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteVehicle(int id)
        {
            try
            {
                await _vehicleService.DeleteVehicleAsync(id);
                _logger.LogInformation("Vehicle deleted with id {VehicleId}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return HandleError(ex, "DeleteVehicle", "VEHICLE_DELETION_ERROR");
            }
        }
    }
}