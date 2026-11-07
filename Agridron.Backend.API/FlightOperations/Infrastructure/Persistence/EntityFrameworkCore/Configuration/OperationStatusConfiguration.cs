using Agridron.Backend.API.FlightOperations.Domain.Model.Aggregates;
using Agridron.Backend.API.FlightOperations.Domain.Model.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Agridron.Backend.API.FlightOperations.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

internal sealed class OperationStatusConfiguration : IEntityTypeConfiguration<OperationStatus>
{
    private static readonly ValueConverter<OperationStatusId, long> OperationStatusIdConverter =
        new(v => v.Value, v => new OperationStatusId(v));

    private static readonly ValueConverter<MissionId, long> MissionIdConverter =
        new(v => v.Value, v => new MissionId(v));

    private static readonly ValueConverter<DroneId, long> DroneIdConverter =
        new(v => v.Value, v => new DroneId(v));

    public void Configure(EntityTypeBuilder<OperationStatus> builder)
    {
        builder.ToTable("operation_statuses");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .HasColumnName("id")
            .HasConversion(OperationStatusIdConverter)
            .ValueGeneratedOnAdd();

        builder.Property(o => o.MissionId)
            .HasColumnName("mission_id")
            .HasConversion(MissionIdConverter);

        builder.Property(o => o.DroneId)
            .HasColumnName("drone_id")
            .HasConversion(DroneIdConverter);

        builder.OwnsOne(o => o.Location, loc =>
        {
            loc.Property(p => p.Latitude)
                .HasColumnName("latitude")
                .HasPrecision(9, 6)
                .IsRequired();

            loc.Property(p => p.Longitude)
                .HasColumnName("longitude")
                .HasPrecision(9, 6)
                .IsRequired();
        });

        builder.Property(o => o.AltitudeMeters)
            .HasColumnName("altitude_meters")
            .HasPrecision(6, 2);

        builder.Property(o => o.SpeedMetersPerSecond)
            .HasColumnName("speed_mps")
            .HasPrecision(5, 2);

        builder.Property(o => o.BatteryLevel)
            .HasColumnName("battery_level")
            .HasPrecision(5, 2);

        builder.Property(o => o.TankVolumeLiters)
            .HasColumnName("tank_volume_liters")
            .HasPrecision(8, 2);

        builder.Property(o => o.Status)
            .HasColumnName("status")
            .IsRequired()
            .HasMaxLength(OperationStatus.MaxStateDescriptionLength);

        builder.Property(o => o.Timestamp)
            .HasColumnName("timestamp")
            .IsRequired();

        builder.Property(o => o.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(o => o.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasIndex(o => o.MissionId)
            .HasDatabaseName("ix_operation_statuses_mission_id");

        builder.HasIndex(o => o.DroneId)
            .HasDatabaseName("ix_operation_statuses_drone_id");

        builder.HasIndex(o => o.Timestamp)
            .HasDatabaseName("ix_operation_statuses_timestamp");
    }
}
