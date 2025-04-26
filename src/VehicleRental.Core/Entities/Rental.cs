using System;
using VehicleRental.Core.Entities.Enums;

namespace VehicleRental.Core.Entities
{
    public class Rental
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int VehicleId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public RentalStatus Status { get; set; }
        public decimal? InitialOdometerReading { get; set; }
        public decimal? FinalOdometerReading { get; set; }
        public decimal? InitialBatteryLevel { get; set; }
        public decimal? FinalBatteryLevel { get; set; }
        public decimal TotalCost { get; set; } = 0m;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public required Customer Customer { get; set; }
        public required Vehicle Vehicle { get; set; }

        public decimal TotalDistance => FinalOdometerReading - InitialOdometerReading ?? 0m;
        public decimal BatteryDelta => InitialBatteryLevel - FinalBatteryLevel ?? 0m;
    }
}