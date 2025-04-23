using VehicleRental.Core.DTOs;
using VehicleRental.Core.Entities;

namespace VehicleRental.Core.Services
{
    public interface ICustomerService
    {
        Task<CustomerResponse> CreateCustomerAsync(CustomerRequest request);
        Task<CustomerResponse> UpdateCustomerAsync(int id, CustomerRequest request);
        Task DeleteCustomerAsync(int id);
        Task<IEnumerable<CustomerResponse>> GetAllCustomersAsync();
        Task<CustomerResponse> GetCustomerByIdAsync(int id);
    }
}