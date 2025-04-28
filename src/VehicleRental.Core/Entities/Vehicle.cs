
namespace VehicleRental.Core.Entities
{
    public class Vehicle
    {
        public int Id { get; set; }
        public required string VIN { get; set; }
        public required string Make { get; set; }
        public required string Model { get; set; }
        public required int Year { get; set; }
        public required decimal PricePerKmInEuro { get; set; }
        public required decimal PricePerDayInEuro { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public required ICollection<Rental> Rentals { get; set; }
        public required ICollection<Telemetry> TelemetryReadings { get; set; }
    }
}