using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VehicleRental.Core.Entities;

namespace VehicleRental.Infrastructure.Data;

public static class DatabaseSeeder
{
    public static async Task SeedVehiclesAsync(VehicleRentalDbContext context)
    {

        if (await context.Vehicles.AnyAsync())
        {
            return; // Database has already been seeded
        }

        // Get the solution root directory (where the .sln file is)
        var currentDir = Directory.GetCurrentDirectory();
        var solutionRoot = currentDir;
        while (!File.Exists(Path.Combine(solutionRoot, "VehicleRental.sln")))
        {
            var parentDir = Directory.GetParent(solutionRoot);
            if (parentDir == null)
            {
                throw new FileNotFoundException("Could not find solution root directory.");
            }
            solutionRoot = parentDir.FullName;
        }

        var csvPath = Path.Combine(solutionRoot, "vehicles.csv");
        if (!File.Exists(csvPath))
        {
            throw new FileNotFoundException($"Could not find vehicles.csv in the solution root. Expected path: {csvPath}");
        }

        var vehicles = File.ReadAllLines(csvPath)
            .Skip(1) // Skip header row
            .Select(line =>
            {
                var parts = line.Split(',');
                return new Vehicle
                {
                    VIN = parts[0],
                    Make = parts[1],
                    Model = parts[2],
                    Year = int.Parse(parts[3]),
                    PricePerKmInEuro = decimal.Parse(parts[4]),
                    PricePerDayInEuro = decimal.Parse(parts[5]),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Rentals = new List<Rental>(),
                    TelemetryReadings = new List<Telemetry>()
                };
            });

        await context.Vehicles.AddRangeAsync(vehicles);
        await context.SaveChangesAsync();
    }

    public static async Task SeedTelemetryAsync(VehicleRentalDbContext context)
    {
        if (await context.Telemetry.AnyAsync())
        {
            return; // Database has already been seeded
        }

        // Get the solution root directory (where the .sln file is)
        var currentDir = Directory.GetCurrentDirectory();
        var solutionRoot = currentDir;
        while (!File.Exists(Path.Combine(solutionRoot, "VehicleRental.sln")))
        {
            var parentDir = Directory.GetParent(solutionRoot);
            if (parentDir == null)
            {
                throw new FileNotFoundException("Could not find solution root directory.");
            }
            solutionRoot = parentDir.FullName;
        }

        var csvPath = Path.Combine(solutionRoot, "telemetry.csv");
        if (!File.Exists(csvPath))
        {
            throw new FileNotFoundException($"Could not find telemetry.csv in the solution root. Expected path: {csvPath}");
        }

        // Process lines sequentially
        var telemetryList = new List<Telemetry>();
        foreach (var line in File.ReadAllLines(csvPath).Skip(1))
        {
            var parts = line.Split(',');
            var vin = parts[0];
            var name = parts[1];
            var value = decimal.Parse(parts[2]);
            var timestamp = DateTime.UnixEpoch.AddSeconds(int.Parse(parts[3]));

            var vehicle = await context.Vehicles.FirstOrDefaultAsync(v => v.VIN == vin)
                ?? throw new Exception($"Vehicle with VIN {vin} not found.");
            var telemetrytype = await context.TelemetryTypes.FirstOrDefaultAsync(t => t.Name == name)
                ?? throw new Exception($"Telemetry type with name {name} not found.");

            telemetryList.Add(new Telemetry
            {
                VehicleId = vehicle.Id,
                TelemetryTypeId = telemetrytype.Id,
                Value = value,
                Timestamp = timestamp,
                ProcessedAt = DateTime.UtcNow,
                IsValid = true,
                ValidationMessage = "Valid",

                Vehicle = vehicle,
                TelemetryType = telemetrytype
            });
        }

        await context.Telemetry.AddRangeAsync(telemetryList);
        await context.SaveChangesAsync();
    }
}