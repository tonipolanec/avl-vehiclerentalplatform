using Microsoft.EntityFrameworkCore;
using VehicleRental.Core.Entities;
using VehicleRental.Infrastructure.Data;

namespace VehicleRental.Infrastructure.Helpers
{
    public static class FetchHelpers
    {

        public static async Task<Rental> GetRentalByIdAsync(VehicleRentalDbContext context, int id)
        {
            var rental = await context.Rentals
                .Include(r => r.Vehicle)
                .Include(r => r.Customer)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rental == null)
                throw new InvalidOperationException($"Rental with ID {id} not found");

            return rental;
        }

        public static async Task<Vehicle> GetVehicleByIdAsync(VehicleRentalDbContext context, int id)
        {
            var vehicle = await context.Vehicles.FindAsync(id);
            if (vehicle == null)
                throw new InvalidOperationException($"Vehicle with ID {id} not found");
            return vehicle;
        }

        public static async Task<Customer> GetCustomerByIdAsync(VehicleRentalDbContext context, int id)
        {
            var customer = await context.Customers.FindAsync(id);
            if (customer == null)
                throw new InvalidOperationException($"Customer with ID {id} not found");
            return customer;
        }

        public static async Task<TelemetryType> GetTelemetryTypeByIdAsync(VehicleRentalDbContext context, int id)
        {
            var type = await context.TelemetryTypes.FindAsync(id);
            if (type == null)
                throw new InvalidOperationException($"{id} telemetry type not found");
            return type;
        }

        public static async Task<TelemetryType> GetTelemetryTypeAsync(VehicleRentalDbContext context, string name)
        {
            var type = await context.TelemetryTypes.FirstOrDefaultAsync(t => t.Name == name);
            if (type == null)
                throw new InvalidOperationException($"{name} telemetry type not found");
            return type;
        }

        public static async Task<Telemetry> GetLatestTelemetryAsync(VehicleRentalDbContext context, int vehicleId, int telemetryTypeId)
        {
            var telemetry = await context.Telemetry
                .Where(t => t.VehicleId == vehicleId &&
                           t.TelemetryTypeId == telemetryTypeId &&
                           t.IsValid)
                .OrderByDescending(t => t.Timestamp)
                .FirstOrDefaultAsync();

            if (telemetry == null)
                throw new InvalidOperationException($"Telemetry reading with id {telemetryTypeId} not found");

            return telemetry;
        }

        public static async Task<Telemetry> GetTelemetryReadingsForRentalStartEndAsync(VehicleRentalDbContext context, int vehicleId, int telemetryTypeId, DateTime timestamp, bool isStart)
        {
            Telemetry? reading;
            if (isStart)
            {
                reading = await context.Telemetry
                    .Where(t => t.VehicleId == vehicleId &&
                                t.TelemetryTypeId == telemetryTypeId &&
                                t.Timestamp <= timestamp)
                    .OrderByDescending(t => t.Timestamp)
                    .FirstOrDefaultAsync();
            }
            else
            {
                reading = await context.Telemetry
                    .Where(t => t.VehicleId == vehicleId &&
                                t.TelemetryTypeId == telemetryTypeId &&
                                t.Timestamp > timestamp)
                    .OrderBy(t => t.Timestamp)
                    .FirstOrDefaultAsync();

                if (reading == null)
                {
                    reading = await context.Telemetry
                        .Where(t => t.VehicleId == vehicleId &&
                                    t.TelemetryTypeId == telemetryTypeId &&
                                    t.Timestamp <= timestamp)
                        .OrderByDescending(t => t.Timestamp)
                        .FirstOrDefaultAsync();
                }
            }

            if (reading == null)
                throw new InvalidOperationException("Telemetry readings not found");


            return reading;
        }
    }
}
