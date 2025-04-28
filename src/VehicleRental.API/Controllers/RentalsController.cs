using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using VehicleRental.Core.DTOs;
using VehicleRental.Core.Services;
using VehicleRental.API.Authorization;

namespace VehicleRental.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RentalsController : BaseController
    {
        private readonly IRentalService _rentalService;

        public RentalsController(IRentalService rentalService, ILogger<RentalsController> logger) : base(logger)
        {
            _rentalService = rentalService;
        }

        [HttpPost]
        public async Task<ActionResult<RentalAllDetailsResponse>> CreateRental(CreateRentalRequest request)
        {
            try
            {
                var rental = await _rentalService.CreateRentalAsync(request);
                _logger.LogInformation("Rental created with id {RentalId}", rental.Id);
                return CreatedAtAction(nameof(GetRental), new { id = rental.Id }, rental);
            }
            catch (Exception ex)
            {
                return HandleError<RentalAllDetailsResponse>(ex, "CreateRental", "RENTAL_CREATION_ERROR");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RentalAllDetailsResponse>> GetRental(int id)
        {
            try
            {
                var rental = await _rentalService.GetRentalByIdAsync(id);
                _logger.LogInformation("Rental retrieved with id {RentalId}", rental.Id);
                return Ok(rental);
            }
            catch (Exception ex)
            {
                return HandleError<RentalAllDetailsResponse>(ex, "GetRental", "RENTAL_RETRIEVAL_ERROR");
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RentalBasicDetailsResponse>>> GetAllRentals()
        {
            try
            {
                var rentals = await _rentalService.GetAllRentalsAsync();
                _logger.LogInformation("All rentals retrieved with count {RentalCount}", rentals.Count());
                return Ok(rentals);
            }
            catch (Exception ex)
            {
                return HandleError<IEnumerable<RentalBasicDetailsResponse>>(ex, "GetAllRentals", "RENTALS_RETRIEVAL_ERROR");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<RentalAllDetailsResponse>> UpdateRental(int id, UpdateRentalDatesRequest request)
        {
            try
            {
                var rental = await _rentalService.UpdateRentalAsync(id, request);
                _logger.LogInformation("Rental updated with id {RentalId}", rental.Id);
                return Ok(rental);
            }
            catch (Exception ex)
            {
                return HandleError<RentalAllDetailsResponse>(ex, "UpdateRental", "RENTAL_UPDATE_ERROR");
            }
        }

        [HttpPut("{id}/cancel")]
        public async Task<ActionResult> CancelRental(int id)
        {
            try
            {
                await _rentalService.CancelRentalAsync(id);
                _logger.LogInformation("Rental cancelled with id {RentalId}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return HandleError(ex, "CancelRental", "RENTAL_CANCELLATION_ERROR");
            }
        }

        [HttpPut("{id}/finish")]
        public async Task<ActionResult<RentalAllDetailsResponse>> FinishRental(int id)
        {
            try
            {
                var rental = await _rentalService.FinishRentalAsync(id);
                _logger.LogInformation("Rental completed with id {RentalId}", id);
                return Ok(rental);
            }
            catch (Exception ex)
            {
                return HandleError<RentalAllDetailsResponse>(ex, "FinishRental", "RENTAL_FINISH_ERROR");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "DeleteRentalPolicy")]
        public async Task<ActionResult> DeleteRental(int id)
        {
            try
            {
                await _rentalService.DeleteRentalAsync(id);
                _logger.LogInformation("Rental deleted with id {RentalId}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return HandleError(ex, "DeleteRental", "RENTAL_DELETION_ERROR");
            }
        }
    }
}