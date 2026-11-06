using Agridron.Backend.API.FlightOperations.Domain.Model.Aggregates;
using Agridron.Backend.API.FlightOperations.Domain.Model.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Agridron.Backend.API.FlightOperations.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

internal sealed class DroneConfiguration : IEntityTypeConfiguration<Drone>
{
    private static readonly ValueConverter<DroneId, long> DroneIdConverter =
        new(v => v.Value, v => new DroneId(v));

    public void Configure(EntityTypeBuilder<Drone> builder)
    {
        builder.ToTable("drones");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id)
            .HasColumnName("id")
            .HasConversion(DroneIdConverter)
            .ValueGeneratedOnAdd();

        builder.Property(d => d.SerialNumber)
            .HasColumnName("serial_number")
            .IsRequired()
            .HasMaxLength(Drone.MaxSerialNumberLength);

        builder.Property(d => d.Model)
            .HasColumnName("model")
            .IsRequired()
            .HasMaxLength(Drone.MaxModelLength);

        builder.Property(d => d.CapacityLiters)
            .HasColumnName("capacity_liters")
            .HasPrecision(8, 2)
            .IsRequired();

        builder.Property(d => d.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(d => d.BatteryLevel)
            .HasColumnName("battery_level")
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(d => d.FlightHours)
            .HasColumnName("flight_hours")
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(d => d.NozzleId)
            .HasColumnName("nozzle_id");

        builder.Property(d => d.LastMaintenanceDate)
            .HasColumnName("last_maintenance_date");

        builder.Property(d => d.NextMaintenanceDate)
            .HasColumnName("next_maintenance_date");

        builder.Property(d => d.ImageUrl)
            .HasColumnName("image_url")
            .HasMaxLength(Drone.MaxImageUrlLength);

        builder.Property(d => d.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(d => d.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(d => d.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasIndex(d => d.SerialNumber)
            .IsUnique()
            .HasDatabaseName("ux_drones_serial_number");

        builder.HasIndex(d => d.Status)
            .HasDatabaseName("ix_drones_status");
    }
}
