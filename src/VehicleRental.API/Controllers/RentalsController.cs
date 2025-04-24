using Microsoft.AspNetCore.Mvc;
using VehicleRental.Core.DTOs;
using VehicleRental.Core.Services;

namespace VehicleRental.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RentalsController : BaseController
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating rental");
                return HandleError<RentalResponse>(ex, "CreateRental", "RENTAL_CREATION_ERROR");
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving rental {RentalId}", id);
                return HandleError<RentalResponse>(ex, "GetRental", "RENTAL_RETRIEVAL_ERROR");
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
                return HandleError<IEnumerable<RentalResponse>>(ex, "GetAllRentals", "RENTALS_RETRIEVAL_ERROR");
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating rental {RentalId}", id);
                return HandleError<RentalResponse>(ex, "UpdateRental", "RENTAL_UPDATE_ERROR");
            }
        }

        [HttpPost("{id}/cancel")]
        public async Task<ActionResult> CancelRental(int id)
        {
            try
            {
                await _rentalService.CancelRentalAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling rental {RentalId}", id);
                return HandleError(ex, "CancelRental", "RENTAL_CANCELLATION_ERROR");
            }
        }
    }
}