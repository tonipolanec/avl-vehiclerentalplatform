using VehicleRental.Core.Entities;

namespace VehicleRental.Core.Services
{
    public interface IPricingCalculator
    {
        decimal CalculateRentalPrice(
            decimal totalKilometers,
            int numberOfDays,
            decimal pricePerKm,
            decimal pricePerDay,
            decimal? batteryDelta);
    }
}