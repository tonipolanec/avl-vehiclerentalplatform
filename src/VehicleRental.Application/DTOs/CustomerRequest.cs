using System.ComponentModel.DataAnnotations;

namespace VehicleRental.Application.DTOs
{
    public class CreateCustomerRequest
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
    }

    public class UpdateCustomerNameRequest
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

    }

    public class UpdateCustomerStatusRequest
    {
        [Required]
        public bool IsActive { get; set; }
    }
}