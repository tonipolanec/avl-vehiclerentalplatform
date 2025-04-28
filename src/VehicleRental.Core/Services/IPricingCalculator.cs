using VehicleRental.Core.Entities;

namespace VehicleRental.Core.Services
{
    public interface IPricingCalculator
    {
        decimal CalculateRentalPrice(
            decimal totalKilometers,
            DateTime startDate,
            DateTime endDate,
            decimal pricePerKm,
            decimal pricePerDay,
            decimal? batteryDelta);
    }
}