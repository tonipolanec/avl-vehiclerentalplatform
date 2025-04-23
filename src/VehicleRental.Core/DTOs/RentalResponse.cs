using VehicleRental.Core.Entities;

namespace VehicleRental.Core.DTOs
{
    public class RentalResponse
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public int VehicleId { get; set; }
        public string VehicleVIN { get; set; } = string.Empty;
        public string VehicleMake { get; set; } = string.Empty;
        public string VehicleModel { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal? InitialBatteryLevel { get; set; }
        public decimal? FinalBatteryLevel { get; set; }
        public decimal? TotalDistance { get; set; }
        public decimal TotalCost { get; set; }

        public static RentalResponse FromEntity(Rental rental)
        {
            return new RentalResponse
            {
                Id = rental.Id,
                CustomerId = rental.CustomerId,
                CustomerName = rental.Customer.Name,
                VehicleId = rental.VehicleId,
                VehicleVIN = rental.Vehicle.VIN,
                VehicleMake = rental.Vehicle.Make,
                VehicleModel = rental.Vehicle.Model,
                StartDate = rental.StartDate,
                EndDate = rental.EndDate,
                Status = rental.Status.ToString(),
                InitialBatteryLevel = rental.InitialBatteryLevel,
                FinalBatteryLevel = rental.FinalBatteryLevel,
                TotalDistance = rental.TotalDistance,
                TotalCost = rental.TotalCost
            };
        }
    }
}