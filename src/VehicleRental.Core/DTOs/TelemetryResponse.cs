using System.ComponentModel.DataAnnotations;

namespace VehicleRental.Core.DTOs
{
    public class TelemetryResponse
    {
        public decimal Value { get; set; }
        public long Timestamp { get; set; }
        public bool IsValid { get; set; }
        public string ValidationMessage { get; set; } = string.Empty;

    }
}