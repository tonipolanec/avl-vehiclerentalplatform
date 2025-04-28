using VehicleRental.Core.Entities;

namespace VehicleRental.Application.Services
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