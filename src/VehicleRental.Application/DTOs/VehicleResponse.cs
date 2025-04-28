using VehicleRental.Core.Entities;
using System.Linq;

namespace VehicleRental.Application.DTOs
{
    public class VehicleResponse
    {
        public int Id { get; set; }
        public string VIN { get; set; } = string.Empty;
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public decimal PricePerKmInEuro { get; set; }
        public decimal PricePerDayInEuro { get; set; }
        public decimal TotalDistanceDriven { get; set; }
        public int TotalRentalCount { get; set; }
        public decimal TotalRentalIncome { get; set; }

        public static VehicleResponse FromEntity(Vehicle vehicle)
        {
            return new VehicleResponse
            {
                Id = vehicle.Id,
                VIN = vehicle.VIN,
                Make = vehicle.Make,
                Model = vehicle.Model,
                Year = vehicle.Year,
                PricePerKmInEuro = vehicle.PricePerKmInEuro,
                PricePerDayInEuro = vehicle.PricePerDayInEuro,
                TotalDistanceDriven = vehicle.Rentals.Sum(r => r.TotalDistance),
                TotalRentalCount = vehicle.Rentals.Count,
                TotalRentalIncome = vehicle.Rentals.Sum(r => r.TotalCost)
            };
        }
    }
}