using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VehicleRental.Core.DTOs;
using VehicleRental.Core.Entities;
using VehicleRental.Core.Services;
using VehicleRental.Infrastructure.Data;

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
    }
}