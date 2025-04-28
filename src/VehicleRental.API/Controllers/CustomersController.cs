using Microsoft.AspNetCore.Mvc;
using VehicleRental.Core.DTOs;
using VehicleRental.Core.Services;

namespace VehicleRental.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : BaseController
    {
        private readonly ICustomerService _customerService;

        public CustomersController(ICustomerService customerService, ILogger<CustomersController> logger, IConfiguration configuration) : base(logger, configuration)
        {
            _customerService = customerService;
        }

        [HttpPost]
        public async Task<ActionResult<CustomerBasicDetailsResponse>> CreateCustomer(CreateCustomerRequest request)
        {
            try
            {
                var customer = await _customerService.CreateCustomerAsync(request);
                _logger.LogInformation("Customer created with id {CustomerId}", customer.Id);
                return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, customer);
            }
            catch (Exception ex)
            {
                return HandleError<CustomerBasicDetailsResponse>(ex, "CreateCustomer", "CUSTOMER_CREATION_ERROR");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerAllDetailsResponse>> GetCustomer(int id)
        {
            try
            {
                var customer = await _customerService.GetCustomerByIdAsync(id);
                _logger.LogInformation("Customer retrieved with id {CustomerId}", customer.Id);
                return Ok(customer);
            }
            catch (Exception ex)
            {
                return HandleError<CustomerAllDetailsResponse>(ex, "GetCustomer", "CUSTOMER_RETRIEVAL_ERROR");
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerBasicDetailsResponse>>> GetAllCustomers()
        {
            try
            {
                var customers = await _customerService.GetAllCustomersAsync();
                _logger.LogInformation("All customers retrieved with count {CustomerCount}", customers.Count());
                return Ok(customers);
            }
            catch (Exception ex)
            {
                return HandleError<IEnumerable<CustomerBasicDetailsResponse>>(ex, "GetAllCustomers", "CUSTOMERS_RETRIEVAL_ERROR");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<CustomerBasicDetailsResponse>> UpdateCustomerName(int id, UpdateCustomerNameRequest request)
        {
            try
            {
                var customer = await _customerService.UpdateCustomerNameAsync(id, request);
                _logger.LogInformation("Customer updated with id {CustomerId}", customer.Id);
                return Ok(customer);
            }
            catch (Exception ex)
            {
                return HandleError<CustomerBasicDetailsResponse>(ex, "UpdateCustomer", "CUSTOMER_UPDATE_ERROR");
            }
        }

        [HttpPut("{id}/status")]
        public async Task<ActionResult<CustomerBasicDetailsResponse>> UpdateCustomerStatus(int id, [FromBody] UpdateCustomerStatusRequest request)
        {
            try
            {
                var customer = await _customerService.UpdateCustomerStatusAsync(id, request);
                _logger.LogInformation("Customer status updated with id {CustomerId}", customer.Id);
                return Ok(customer);
            }
            catch (Exception ex)
            {
                return HandleError<CustomerBasicDetailsResponse>(ex, "UpdateCustomerStatus", "CUSTOMER_STATUS_UPDATE_ERROR");
            }
        }


        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCustomer(int id)
        {
            try
            {
                await _customerService.DeleteCustomerAsync(id);
                _logger.LogInformation("Customer deleted with id {CustomerId}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return HandleError(ex, "DeleteCustomer", "CUSTOMER_DELETION_ERROR");
            }
        }
    }
}