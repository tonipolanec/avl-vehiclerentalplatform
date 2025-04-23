using Microsoft.AspNetCore.Mvc;
using VehicleRental.Core.DTOs;
using VehicleRental.Core.Services;

namespace VehicleRental.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RentalsController : ControllerBase
    {
        private readonly IRentalService _rentalService;
        private readonly ILogger<RentalsController> _logger;

        public RentalsController(IRentalService rentalService, ILogger<RentalsController> logger)
        {
            _rentalService = rentalService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<RentalResponse>> CreateRental(RentalRequest request)
        {
            try
            {
                var rental = await _rentalService.CreateRentalAsync(request);
                return CreatedAtAction(nameof(GetRental), new { id = rental.Id }, rental);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Failed to create rental");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating rental");
                return StatusCode(500, "An error occurred while creating the rental");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RentalResponse>> GetRental(int id)
        {
            try
            {
                var rental = await _rentalService.GetRentalByIdAsync(id);
                return Ok(rental);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving rental {RentalId}", id);
                return StatusCode(500, "An error occurred while retrieving the rental");
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RentalResponse>>> GetAllRentals()
        {
            try
            {
                var rentals = await _rentalService.GetAllRentalsAsync();
                return Ok(rentals);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all rentals");
                return StatusCode(500, "An error occurred while retrieving rentals");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<RentalResponse>> UpdateRental(int id, RentalRequest request)
        {
            try
            {
                var rental = await _rentalService.UpdateRentalAsync(id, request);
                return Ok(rental);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Failed to update rental {RentalId}", id);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating rental {RentalId}", id);
                return StatusCode(500, "An error occurred while updating the rental");
            }
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelRental(int id)
        {
            try
            {
                await _rentalService.CancelRentalAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Failed to cancel rental {RentalId}", id);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling rental {RentalId}", id);
                return StatusCode(500, "An error occurred while cancelling the rental");
            }
        }
    }
}