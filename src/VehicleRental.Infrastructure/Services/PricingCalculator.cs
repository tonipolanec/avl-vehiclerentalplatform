using VehicleRental.Application.Services;

namespace VehicleRental.Infrastructure.Services
{
    public class PricingCalculator : IPricingCalculator
    {
        private const decimal BATTERY_CHARGE_COST_MULTIPLIER = 0.20m;

        public decimal CalculateRentalPrice(
            decimal totalKilometers,
            DateTime startDate,
            DateTime endDate,
            decimal pricePerKm,
            decimal pricePerDay,
            decimal? batteryDelta)
        {
            // Total Cost = total_kilometers_per_rental × price_per_km_in_euro +
            //              number_of_rental_days × price_per_day_in_euro +
            //              max(0, -battery_delta_per_rental) × 0.2€

            var distanceCost = totalKilometers * pricePerKm;

            var numberOfDays = (int)Math.Ceiling((endDate - startDate).TotalDays);
            var dailyCost = numberOfDays * pricePerDay;

            var batteryCost = 0m;
            if (batteryDelta.HasValue)
            {
                batteryCost = Math.Max(0, -batteryDelta.Value) * BATTERY_CHARGE_COST_MULTIPLIER;
            }

            return distanceCost + dailyCost + batteryCost;
        }
    }
}