
namespace VehicleRental.Application.DTOs
{
    public class ErrorResponse
    {
        public string Message { get; set; }
        public string ErrorCode { get; set; }
        public List<string> Details { get; set; }

        public ErrorResponse(string message, string errorCode, List<string> details)
        {
            Message = message ?? string.Empty;
            ErrorCode = errorCode ?? string.Empty;
            Details = details ?? new List<string>();
        }
    }
}