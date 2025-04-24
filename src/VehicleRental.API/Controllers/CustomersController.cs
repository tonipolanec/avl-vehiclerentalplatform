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
        private readonly ILogger<CustomersController> _logger;

        public CustomersController(ICustomerService customerService, ILogger<CustomersController> logger)
        {
            _customerService = customerService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<CustomerResponse>> CreateCustomer(CustomerRequest request)
        {
            try
            {
                var customer = await _customerService.CreateCustomerAsync(request);
                return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer");
                return HandleError<CustomerResponse>(ex, "CreateCustomer", "CUSTOMER_CREATION_ERROR");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerResponse>> GetCustomer(int id)
        {
            try
            {
                var customer = await _customerService.GetCustomerByIdAsync(id);
                return Ok(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer {CustomerId}", id);
                return HandleError<CustomerResponse>(ex, "GetCustomer", "CUSTOMER_RETRIEVAL_ERROR");
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerResponse>>> GetAllCustomers()
        {
            try
            {
                var customers = await _customerService.GetAllCustomersAsync();
                return Ok(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all customers");
                return HandleError<IEnumerable<CustomerResponse>>(ex, "GetAllCustomers", "CUSTOMERS_RETRIEVAL_ERROR");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<CustomerResponse>> UpdateCustomer(int id, CustomerRequest request)
        {
            try
            {
                var customer = await _customerService.UpdateCustomerAsync(id, request);
                return Ok(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer {CustomerId}", id);
                return HandleError<CustomerResponse>(ex, "UpdateCustomer", "CUSTOMER_UPDATE_ERROR");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCustomer(int id)
        {
            try
            {
                await _customerService.DeleteCustomerAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer {CustomerId}", id);
                return HandleError(ex, "DeleteCustomer", "CUSTOMER_DELETION_ERROR");
            }
        }
    }
}