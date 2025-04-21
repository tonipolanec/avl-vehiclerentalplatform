using System;
using System.Collections.Generic;

namespace VehicleRental.Core.Entities
{
    public class Customer
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsActive { get; set; }

        // Navigation property
        public required ICollection<Rental> Rentals { get; set; }
    }
}