
namespace VehicleRental.Core.Entities
{
    public class Telemetry
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public int TelemetryTypeId { get; set; }
        public decimal Value { get; set; }
        public DateTime Timestamp { get; set; }
        public DateTime ProcessedAt { get; set; }
        public bool IsValid { get; set; }
        public required string ValidationMessage { get; set; }

        // Navigation properties
        public required Vehicle Vehicle { get; set; }
        public required TelemetryType TelemetryType { get; set; }
    }
}