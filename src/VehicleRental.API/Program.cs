using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using VehicleRental.Core.Services;
using VehicleRental.Infrastructure.Data;
using VehicleRental.Infrastructure.Services;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Routing;
using VehicleRental.Infrastructure.Services.Validators;
using VehicleRental.Infrastructure.Interfaces;
using Serilog;
using Serilog.Events;
using Microsoft.AspNetCore.Authorization;
using VehicleRental.API.Authorization;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/vehicle-rental-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("--------------------------------");
    Log.Information("Starting Vehicle Rental API");
    Log.Information("--------------------------------");

    var builder = WebApplication.CreateBuilder(args);

    // Add Serilog to the logging providers
    builder.Host.UseSerilog();

    builder.Logging.ClearProviders();
    builder.Logging.AddConsole(options =>
    {
        options.FormatterName = "simple";
    });
    builder.Logging.SetMinimumLevel(LogLevel.Information);

    // Add services to the container.
    builder.Services.AddControllers();
    builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Vehicle Rental API",
            Version = "v1",
            Description = "API for managing vehicle rentals",
        });
        //c.OrderActionsBy(apiDesc => apiDesc.HttpMethod);
    });

    // Add DbContext configuration
    builder.Services.AddDbContext<VehicleRentalDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

    // Register services
    builder.Services.AddScoped<ICustomerService, CustomerService>();
    builder.Services.AddScoped<IVehicleService, VehicleService>();
    builder.Services.AddScoped<IRentalService, RentalService>();
    builder.Services.AddScoped<IPricingCalculator, PricingCalculator>();
    builder.Services.AddScoped<ITelemetryService, TelemetryService>();
    builder.Services.AddScoped<ITelemetryValidator, OdometerValidator>();
    builder.Services.AddScoped<ITelemetryValidator, BatteryValidator>();
    builder.Services.AddScoped<IRentalValidator, RentalValidator>();

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("DeleteRentalPolicy", policy =>
            policy.Requirements.Add(new DeleteRentalRequirement()));
    });

    builder.Services.AddSingleton<IAuthorizationHandler, DeleteRentalAuthorizationHandler>();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    //Seed the database
    if (app.Configuration.GetValue<bool>("DatabaseOptions:SeedData"))
    {
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                var context = services.GetRequiredService<VehicleRentalDbContext>();
                await DatabaseSeeder.SeedVehiclesAsync(context);
                await DatabaseSeeder.SeedTelemetryAsync(context);
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred while seeding the database.");
            }
        }
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}


