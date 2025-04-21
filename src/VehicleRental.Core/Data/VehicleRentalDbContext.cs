using Microsoft.EntityFrameworkCore;

namespace VehicleRental.Core.Data;

public class VehicleRentalDbContext : DbContext
{
    public VehicleRentalDbContext(DbContextOptions<VehicleRentalDbContext> options)
        : base(options)
    {
    }

    // DbSet properties will be added here for your entities
    // Example: public DbSet<Vehicle> Vehicles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Entity configurations will be added here
    }
}