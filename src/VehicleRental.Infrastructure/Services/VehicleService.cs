using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VehicleRental.Application.DTOs;
using VehicleRental.Application.Services;
using VehicleRental.Core.Entities;
using VehicleRental.Infrastructure.Data;
using VehicleRental.Infrastructure.Helpers;

namespace VehicleRental.Infrastructure.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly VehicleRentalDbContext _context;
        private readonly ILogger<VehicleService> _logger;

        public VehicleService(VehicleRentalDbContext context, ILogger<VehicleService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<VehicleResponse> GetVehicleByIdAsync(int id)
        {
            var vehicle = await _context.Vehicles
                .Include(v => v.Rentals)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vehicle == null)
            {
                throw new KeyNotFoundException($"Vehicle with ID {id} not found");
            }

            return VehicleResponse.FromEntity(vehicle);
        }

        public async Task<IEnumerable<VehicleResponse>> GetAllVehiclesAsync()
        {
            var vehicles = await _context.Vehicles
                .Include(v => v.Rentals)
                .ToListAsync();

            return vehicles.Select(VehicleResponse.FromEntity);

        }

        // create vehicle
        public async Task<VehicleResponse> CreateVehicleAsync(CreateVehicleRequest request)
        {
            var vehicle = new Vehicle
            {
                VIN = request.VIN,
                Make = request.Make,
                Model = request.Model,
                Year = request.Year,
                PricePerKmInEuro = request.PricePerKmInEuro,
                PricePerDayInEuro = request.PricePerDayInEuro,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Rentals = new List<Rental>(),
                TelemetryReadings = new List<Telemetry>(),
            };

            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();

            return VehicleResponse.FromEntity(vehicle);
        }

        public async Task DeleteVehicleAsync(int id)
        {
            var vehicle = await FetchHelpers.GetVehicleByIdAsync(_context, id);

            _context.Vehicles.Remove(vehicle);
            await _context.SaveChangesAsync();
        }
    }
}