using VehicleRental.Core.Entities;

namespace VehicleRental.Core.DTOs
{
    public class CustomerResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal TotalDistanceDriven { get; set; }
        public decimal TotalPrice { get; set; }
        public int TotalRentals { get; set; }

        public static CustomerResponse FromEntity(Customer customer)
        {
            return new CustomerResponse
            {
                Id = customer.Id,
                Name = customer.Name,
                TotalDistanceDriven = customer.Rentals.Sum(r => r.TotalDistance),
                TotalPrice = customer.Rentals.Sum(r => r.TotalCost),
                TotalRentals = customer.Rentals.Count
            };
        }
    }
}