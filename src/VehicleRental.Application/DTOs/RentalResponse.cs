using VehicleRental.Core.Entities;

namespace VehicleRental.Application.DTOs
{
    public class RentalAllDetailsResponse
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public int VehicleId { get; set; }
        public string VehicleVIN { get; set; } = string.Empty;
        public string VehicleMake { get; set; } = string.Empty;
        public string VehicleModel { get; set; } = string.Empty;
        public long StartDate { get; set; }
        public long EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal? InitialOdometerReading { get; set; }
        public decimal? FinalOdometerReading { get; set; }
        public decimal? InitialBatteryLevel { get; set; }
        public decimal? FinalBatteryLevel { get; set; }
        public decimal? TotalDistance { get; set; }
        public decimal TotalCost { get; set; }

        public static RentalAllDetailsResponse FromEntity(Rental rental)
        {
            return new RentalAllDetailsResponse
            {
                Id = rental.Id,
                CustomerId = rental.CustomerId,
                CustomerName = rental.Customer.Name,
                VehicleId = rental.VehicleId,
                VehicleVIN = rental.Vehicle.VIN,
                VehicleMake = rental.Vehicle.Make,
                VehicleModel = rental.Vehicle.Model,
                StartDate = ((DateTimeOffset)rental.StartDate).ToUnixTimeSeconds(),
                EndDate = ((DateTimeOffset)rental.EndDate).ToUnixTimeSeconds(),
                Status = rental.Status.ToString(),
                InitialOdometerReading = rental.InitialOdometerReading,
                FinalOdometerReading = rental.FinalOdometerReading,
                InitialBatteryLevel = rental.InitialBatteryLevel,
                FinalBatteryLevel = rental.FinalBatteryLevel,
                TotalDistance = rental.TotalDistance,
                TotalCost = rental.TotalCost
            };
        }
    }

    public class RentalBasicDetailsResponse
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int VehicleId { get; set; }
        public string Status { get; set; } = string.Empty;
        public long StartDate { get; set; }
        public long EndDate { get; set; }

        public static RentalBasicDetailsResponse FromEntity(Rental rental)
        {
            return new RentalBasicDetailsResponse
            {
                Id = rental.Id,
                CustomerId = rental.CustomerId,
                VehicleId = rental.VehicleId,
                Status = rental.Status.ToString(),
                StartDate = ((DateTimeOffset)rental.StartDate).ToUnixTimeSeconds(),
                EndDate = ((DateTimeOffset)rental.EndDate).ToUnixTimeSeconds()
            };
        }
    }

}