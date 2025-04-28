using Microsoft.AspNetCore.Mvc;
using VehicleRental.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace VehicleRental.API.Controllers;

[ApiController]
[Route("[controller]")]
public class TestController : BaseController
{
    private readonly VehicleRentalDbContext _context;

    public TestController(VehicleRentalDbContext context, ILogger<TestController> logger, IConfiguration configuration) : base(logger, configuration)
    {
        _context = context;
    }

    [HttpGet("connection")]
    public async Task<IActionResult> TestConnection()
    {
        try
        {
            _logger.LogInformation("Attempting to connect to database...");

            // Get connection string for debugging
            var connectionString = _context.Database.GetConnectionString();
            _logger.LogInformation($"Connection string: {connectionString}");

            // Test connection
            bool canConnect = await _context.Database.CanConnectAsync();

            if (canConnect)
            {
                _logger.LogInformation("Successfully connected to database");
                return Ok(new { Connected = true, Message = "Successfully connected to database" });
            }
            else
            {
                _logger.LogWarning("Could not connect to database");
                return Ok(new { Connected = false, Message = "Could not connect to database" });
            }
        }
        catch (Exception ex)
        {
            return HandleError(ex, "TestConnection", "DATABASE_CONNECTION_ERROR");
        }
    }
}