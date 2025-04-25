using System.ComponentModel.DataAnnotations;

namespace VehicleRental.Core.DTOs
{
    public class TelemetryRequest
    {
        [Required]
        public int VehicleId { get; set; }

        [Required]
        public int TelemetryTypeId { get; set; }

        [Required]
        public decimal Value { get; set; }

        public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}