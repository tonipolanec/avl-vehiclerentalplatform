using VehicleRental.Core.Entities;

namespace VehicleRental.Core.DTOs
{
    public class CustomerAllDetailsResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal TotalDistanceDriven { get; set; }
        public decimal TotalPrice { get; set; }
        public bool IsActive { get; set; }

        public static CustomerAllDetailsResponse FromEntity(Customer customer)
        {
            return new CustomerAllDetailsResponse
            {
                Id = customer.Id,
                Name = customer.Name,
                TotalDistanceDriven = customer.Rentals.Sum(r => r.TotalDistance),
                TotalPrice = customer.Rentals.Sum(r => r.TotalCost),
                IsActive = customer.IsActive
            };
        }
    }

    public class CustomerBasicDetailsResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }

        public static CustomerBasicDetailsResponse FromEntity(Customer customer)
        {
            return new CustomerBasicDetailsResponse
            {
                Id = customer.Id,
                Name = customer.Name,
                IsActive = customer.IsActive
            };
        }
    }
}