using System.ComponentModel.DataAnnotations;
using VehicleRental.Core.Entities.Enums;

namespace VehicleRental.Core.DTOs
{
    public class RentalRequest
    {
        [Required]
        public int CustomerId { get; set; }

        [Required]
        public int VehicleId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Range(0, 100)]
        public decimal? InitialBatteryLevel { get; set; }

        [Range(0, 100)]
        public decimal? FinalBatteryLevel { get; set; }

        public RentalStatus? Status { get; set; }
    }

}