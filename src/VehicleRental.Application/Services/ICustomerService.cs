using VehicleRental.Application.DTOs;

namespace VehicleRental.Application.Services
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