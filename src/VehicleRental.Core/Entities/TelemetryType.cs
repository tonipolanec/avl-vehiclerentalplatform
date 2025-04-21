using System;
using System.Collections.Generic;

namespace VehicleRental.Core.Entities
{
    public class TelemetryType
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required string Unit { get; set; }
        public decimal? ValidationMin { get; set; }
        public decimal? ValidationMax { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation property
        public required ICollection<Telemetry> Readings { get; set; }
    }
}