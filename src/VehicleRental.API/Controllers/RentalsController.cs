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
        public async Task<ActionResult<RentalAllDetailsResponse>> CreateRental(CreateRentalRequest request)
        {
            try
            {
                var rental = await _rentalService.CreateRentalAsync(request);
                return CreatedAtAction(nameof(GetRental), new { id = rental.Id }, rental);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating rental");
                return HandleError<RentalAllDetailsResponse>(ex, "CreateRental", "RENTAL_CREATION_ERROR");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RentalAllDetailsResponse>> GetRental(int id)
        {
            try
            {
                var rental = await _rentalService.GetRentalByIdAsync(id);
                return Ok(rental);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving rental {RentalId}", id);
                return HandleError<RentalAllDetailsResponse>(ex, "GetRental", "RENTAL_RETRIEVAL_ERROR");
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RentalBasicDetailsResponse>>> GetAllRentals()
        {
            try
            {
                var rentals = await _rentalService.GetAllRentalsAsync();
                return Ok(rentals);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all rentals");
                return HandleError<IEnumerable<RentalBasicDetailsResponse>>(ex, "GetAllRentals", "RENTALS_RETRIEVAL_ERROR");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<RentalAllDetailsResponse>> UpdateRental(int id, UpdateRentalDatesRequest request)
        {
            try
            {
                var rental = await _rentalService.UpdateRentalAsync(id, request);
                return Ok(rental);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating rental {RentalId}", id);
                return HandleError<RentalAllDetailsResponse>(ex, "UpdateRental", "RENTAL_UPDATE_ERROR");
            }
        }

        [HttpPut("{id}/cancel")]
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

        [HttpPut("{id}/finish")]
        public async Task<ActionResult<RentalAllDetailsResponse>> FinishRental(int id)
        {
            try
            {
                var rental = await _rentalService.FinishRentalAsync(id);
                return Ok(rental);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finishing rental {RentalId}", id);
                return HandleError<RentalAllDetailsResponse>(ex, "FinishRental", "RENTAL_FINISH_ERROR");
            }
        }
    }
}