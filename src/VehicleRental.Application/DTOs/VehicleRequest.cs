using System.ComponentModel.DataAnnotations;

namespace VehicleRental.Application.DTOs
{
    public class CreateVehicleRequest
    {
        [Required]
        [StringLength(17)]
        public string VIN { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Make { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Model { get; set; } = string.Empty;

        [Required]
        [Range(1900, 2100)]
        public int Year { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal PricePerKmInEuro { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal PricePerDayInEuro { get; set; }
    }
}