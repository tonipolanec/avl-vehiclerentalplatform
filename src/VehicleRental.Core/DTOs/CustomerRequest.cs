using System.ComponentModel.DataAnnotations;

namespace VehicleRental.Core.DTOs
{
    public class CustomerRequest
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public bool? IsActive { get; set; } = true;
    }
}