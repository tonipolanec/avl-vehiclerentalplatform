using Microsoft.AspNetCore.Mvc;
using VehicleRental.Core.DTOs;
using Microsoft.Extensions.Logging;
using Npgsql;
using Microsoft.EntityFrameworkCore;

namespace VehicleRental.API.Controllers
{
    public class BaseController : ControllerBase
    {
        protected readonly ILogger<BaseController> _logger;

        protected BaseController(ILogger<BaseController> logger)
        {
            _logger = logger;
        }

        protected ActionResult<T> HandleError<T>(Exception ex, string context, string errorCode)
        {
            return HandleError(ex, context, errorCode ?? "UNKNOWN");
        }

        protected ActionResult HandleError(Exception ex, string context, string errorCode)
        {
            switch (ex)
            {
                case KeyNotFoundException:
                    _logger.LogError(ex, "Resource not found: {Message}", ex.Message);
                    return NotFound(new ErrorResponse(
                        "The requested resource was not found.",
                        "RESOURCE_NOT_FOUND",
                        new List<string> { ex.Message }
                    ));

                case InvalidOperationException:
                    _logger.LogError(ex, "Invalid operation: {Message}", ex.Message);
                    return BadRequest(new ErrorResponse(
                        ex.Message,
                        errorCode ?? "INVALID_OPERATION",
                        new List<string> { "Please check your request and try again." }
                    ));

                case ArgumentException:
                    _logger.LogError(ex, "Invalid input: {Message}", ex.Message);
                    return BadRequest(new ErrorResponse(
                        "Invalid input provided.",
                        errorCode ?? "INVALID_INPUT",
                        new List<string> { ex.Message }
                    ));

                case DbUpdateException dbEx when dbEx.InnerException is PostgresException pgEx:
                    string constraintType = pgEx.SqlState switch
                    {
                        "23505" => "Unique constraint violation",
                        "23503" => "Foreign key violation",
                        "23502" => "Not null violation",
                        _ => "Database constraint violation"
                    };

                    _logger.LogError(ex, "{ConstraintType}: {Message}", constraintType, ex.Message);

                    return BadRequest(new ErrorResponse(
                        constraintType + ".",
                        "DATABASE_CONSTRAINT_ERROR",
                        new List<string>
                        {
                            pgEx.Detail ?? ex.Message,
                            $"Constraint: {pgEx.ConstraintName}"
                        }
                    ));

                default:
                    _logger.LogError(ex, "An unexpected error occurred: {Message}", ex.Message);
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
            _logger.LogError("Validation error: {Message}", message);
            return BadRequest(new ErrorResponse(
                message,
                "VALIDATION_ERROR",
                details ?? new List<string>()
            ));
        }
    }
}