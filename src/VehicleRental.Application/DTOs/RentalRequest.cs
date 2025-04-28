using System.ComponentModel.DataAnnotations;
using VehicleRental.Core.Entities.Enums;

namespace VehicleRental.Application.DTOs
{
    public class CreateRentalRequest
    {
        [Required]
        public int CustomerId { get; set; }

        [Required]
        public int VehicleId { get; set; }

        [Required]
        public long StartDate { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        [Required]
        public long EndDate { get; set; }

    }

    public class UpdateRentalDatesRequest
    {
        public long? StartDate { get; set; }

        public long? EndDate { get; set; }
    }

    public class UpdateRentalStatusRequest
    {
        [Required]
        public RentalStatus Status { get; set; }
    }

}