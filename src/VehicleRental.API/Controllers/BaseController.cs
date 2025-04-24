using Microsoft.AspNetCore.Mvc;
using VehicleRental.Core.DTOs;

namespace VehicleRental.API.Controllers
{
    public class BaseController : ControllerBase
    {
        protected ActionResult<T> HandleError<T>(Exception ex, string context, string errorCode)
        {
            return HandleError(ex, context, errorCode ?? "UNKNOWN");
        }

        protected ActionResult HandleError(Exception ex, string context, string errorCode)
        {
            switch (ex)
            {
                case KeyNotFoundException:
                    return NotFound(new ErrorResponse(
                        "The requested resource was not found.",
                        "RESOURCE_NOT_FOUND",
                        new List<string> { ex.Message }
                    ));

                case InvalidOperationException:
                    return BadRequest(new ErrorResponse(
                        ex.Message,
                        errorCode ?? "INVALID_OPERATION",
                        new List<string> { "Please check your request and try again." }
                    ));

                case ArgumentException:
                    return BadRequest(new ErrorResponse(
                        "Invalid input provided.",
                        errorCode ?? "INVALID_INPUT",
                        new List<string> { ex.Message }
                    ));

                default:
                    return StatusCode(500, new ErrorResponse(
                        "An unexpected error occurred.",
                        "INTERNAL_SERVER_ERROR",
                        new List<string> { "Please try again later or contact support if the problem persists." }
                    ));
            }
        }

        protected ActionResult<T> HandleValidationError<T>(string message, List<string> details)
        {
            return HandleValidationError(message, details ?? new List<string>());
        }

        protected ActionResult HandleValidationError(string message, List<string> details)
        {
            return BadRequest(new ErrorResponse(
                message,
                "VALIDATION_ERROR",
                details ?? new List<string>()
            ));
        }
    }
}