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
        private readonly ILogger<VehiclesController> _logger;

        public VehiclesController(IVehicleService vehicleService, ILogger<VehiclesController> logger)
        {
            _vehicleService = vehicleService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<VehicleResponse>> GetVehicle(int id)
        {
            try
            {
                var vehicle = await _vehicleService.GetVehicleByIdAsync(id);
                return Ok(vehicle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vehicle {VehicleId}", id);
                return HandleError<VehicleResponse>(ex, "GetVehicle", "VEHICLE_RETRIEVAL_ERROR");
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<VehicleResponse>>> GetAllVehicles()
        {
            try
            {
                var vehicles = await _vehicleService.GetAllVehiclesAsync();
                return Ok(vehicles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all vehicles");
                return HandleError<IEnumerable<VehicleResponse>>(ex, "GetAllVehicles", "VEHICLES_RETRIEVAL_ERROR");
            }
        }
    }
}