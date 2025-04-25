using VehicleRental.Core.DTOs;
using VehicleRental.Core.Entities;

namespace VehicleRental.Core.Services
{
    public interface ICustomerService
    {
        Task<CustomerAllDetailsResponse> CreateCustomerAsync(CreateCustomerRequest request);
        Task<CustomerAllDetailsResponse> UpdateCustomerNameAsync(int id, UpdateCustomerNameRequest request);
        Task<CustomerAllDetailsResponse> UpdateCustomerStatusAsync(int id, UpdateCustomerStatusRequest request);
        Task DeleteCustomerAsync(int id);
        Task<IEnumerable<CustomerBasicDetailsResponse>> GetAllCustomersAsync();
        Task<CustomerAllDetailsResponse> GetCustomerByIdAsync(int id);
    }
}