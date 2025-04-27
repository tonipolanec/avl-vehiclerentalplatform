using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VehicleRental.Core.DTOs;
using VehicleRental.Core.Entities;
using VehicleRental.Core.Services;
using VehicleRental.Infrastructure.Data;

namespace VehicleRental.Infrastructure.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly VehicleRentalDbContext _context;
        private readonly ILogger<CustomerService> _logger;

        public CustomerService(VehicleRentalDbContext context, ILogger<CustomerService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<CustomerAllDetailsResponse> CreateCustomerAsync(CreateCustomerRequest request)
        {
            var customer = new Customer
            {
                Name = request.Name,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true,
                Rentals = new List<Rental>()
            };

            await _context.Customers.AddAsync(customer);
            await _context.SaveChangesAsync();

            return await GetCustomerByIdAsync(customer.Id);
        }

        public async Task<CustomerAllDetailsResponse> GetCustomerByIdAsync(int id)
        {
            var customer = await _context.Customers
                .Include(c => c.Rentals)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null)
            {
                throw new KeyNotFoundException($"Customer with ID {id} not found");
            }

            return CustomerAllDetailsResponse.FromEntity(customer);
        }

        public async Task<IEnumerable<CustomerBasicDetailsResponse>> GetAllCustomersAsync()
        {
            var customers = await _context.Customers
                .Include(c => c.Rentals)
                .ToListAsync();

            return customers.Select(CustomerBasicDetailsResponse.FromEntity);
        }

        public async Task<CustomerAllDetailsResponse> UpdateCustomerNameAsync(int id, UpdateCustomerNameRequest request)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null)
            {
                throw new KeyNotFoundException($"Customer with ID {id} not found");
            }

            customer.Name = request.Name;
            customer.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetCustomerByIdAsync(customer.Id);
        }

        public async Task<CustomerAllDetailsResponse> UpdateCustomerStatusAsync(int id, UpdateCustomerStatusRequest request)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null)
            {
                throw new KeyNotFoundException($"Customer with ID {id} not found");
            }

            customer.IsActive = request.IsActive;
            customer.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetCustomerByIdAsync(customer.Id);
        }

        public async Task DeleteCustomerAsync(int id)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null)
            {
                throw new KeyNotFoundException($"Customer with ID {id} not found");
            }

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
        }
    }
}