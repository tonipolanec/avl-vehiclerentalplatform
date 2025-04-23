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

        public async Task<CustomerResponse> CreateCustomerAsync(CustomerRequest request)
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

            _logger.LogInformation("Created new customer {CustomerName} with ID {CustomerId}", customer.Name, customer.Id);

            return await GetCustomerByIdAsync(customer.Id);
        }

        public async Task<CustomerResponse> GetCustomerByIdAsync(int id)
        {
            var customer = await _context.Customers
                .Include(c => c.Rentals)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null)
            {
                throw new KeyNotFoundException($"Customer with ID {id} not found");
            }

            return CustomerResponse.FromEntity(customer);
        }

        public async Task<IEnumerable<CustomerResponse>> GetAllCustomersAsync()
        {
            var customers = await _context.Customers
                .Include(c => c.Rentals)
                .ToListAsync();

            return customers.Select(CustomerResponse.FromEntity);
        }

        public async Task<CustomerResponse> UpdateCustomerAsync(int id, CustomerRequest request)
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

            _logger.LogInformation("Updated customer {CustomerName} with ID {CustomerId}", customer.Name, customer.Id);

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

            _logger.LogInformation("Deleted customer with ID {CustomerId}", id);
        }
    }
}