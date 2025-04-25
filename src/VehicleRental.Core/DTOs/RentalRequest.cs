using System.ComponentModel.DataAnnotations;
using VehicleRental.Core.Entities.Enums;

namespace VehicleRental.Core.DTOs
{
    public class CreateRentalRequest
    {
        [Required]
        public int CustomerId { get; set; }

        [Required]
        public int VehicleId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public RentalStatus Status { get; set; } = RentalStatus.Ordered;

        [Range(0, 100)]
        public decimal? InitialBatteryLevel { get; set; } = 100;

        [Range(0, 9999999999)]
        public decimal? InitialOdometerReading { get; set; } = 0;

    }

    public class UpdateRentalDatesRequest
    {
        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }
    }

    public class UpdateRentalStatusRequest
    {
        [Required]
        public RentalStatus Status { get; set; }
    }

    public class FinishRentalRequest
    {
        public RentalStatus Status { get; set; } = RentalStatus.Completed;

        [Required]
        public decimal FinalOdometerReading { get; set; }

        [Required]
        public decimal FinalBatteryLevel { get; set; }

        //[Required]
        //public decimal TotalCost { get; set; }
    }
}