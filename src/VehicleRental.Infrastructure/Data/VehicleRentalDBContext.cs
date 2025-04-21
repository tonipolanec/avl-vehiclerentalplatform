using Microsoft.EntityFrameworkCore;
using VehicleRental.Core.Entities;

namespace VehicleRental.Infrastructure.Data;

public class VehicleRentalDbContext : DbContext
{
    public VehicleRentalDbContext(DbContextOptions<VehicleRentalDbContext> options)
        : base(options)
    {
    }

    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Rental> Rentals { get; set; }
    public DbSet<TelemetryType> TelemetryTypes { get; set; }
    public DbSet<Telemetry> Telemetry { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure entities
        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.VIN).IsUnique();
            entity.Property(e => e.VIN).HasMaxLength(17).IsRequired();
            entity.Property(e => e.Make).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Model).HasMaxLength(50).IsRequired();
            entity.Property(e => e.PricePerKmInEuro).HasPrecision(5, 2).IsRequired();
            entity.Property(e => e.PricePerDayInEuro).HasPrecision(5, 2).IsRequired();
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<Rental>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.InitialBatteryLevel).HasPrecision(5, 2);
            entity.Property(e => e.FinalBatteryLevel).HasPrecision(5, 2);
            entity.Property(e => e.TotalCost).HasPrecision(10, 2);

            entity.HasOne(r => r.Customer)
                  .WithMany(c => c.Rentals)
                  .HasForeignKey(r => r.CustomerId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.Vehicle)
                  .WithMany(v => v.Rentals)
                  .HasForeignKey(r => r.VehicleId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.ToTable(t => t.HasCheckConstraint("chk_dates", "\"EndDate\" > \"StartDate\""));
            entity.ToTable(t => t.HasCheckConstraint("chk_battery_levels",
                "\"InitialBatteryLevel\" BETWEEN 0 AND 100 AND \"FinalBatteryLevel\" BETWEEN 0 AND 100"));
        });

        modelBuilder.Entity<Telemetry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Value).HasPrecision(10, 2);
            entity.Property(e => e.ValidationMessage).HasMaxLength(255);

            entity.HasOne(t => t.Vehicle)
                  .WithMany(v => v.TelemetryReadings)
                  .HasForeignKey(t => t.VehicleId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.TelemetryType)
                  .WithMany(tt => tt.Readings)
                  .HasForeignKey(t => t.TelemetryTypeId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TelemetryType>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Unit).HasMaxLength(20);
            entity.Property(e => e.ValidationMin).HasPrecision(10, 2);
            entity.Property(e => e.ValidationMax).HasPrecision(10, 2);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EntityName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Action).HasMaxLength(50).IsRequired();
            entity.Property(e => e.OldValues).HasMaxLength(4000);
            entity.Property(e => e.NewValues).HasMaxLength(4000);
            entity.Property(e => e.Timestamp).IsRequired();
        });
    }
}