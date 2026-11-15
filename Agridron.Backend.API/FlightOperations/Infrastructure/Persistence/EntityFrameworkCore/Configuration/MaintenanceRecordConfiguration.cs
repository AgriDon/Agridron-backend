using Agridron.Backend.API.FlightOperations.Domain.Model.Aggregates;
using Agridron.Backend.API.FlightOperations.Domain.Model.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Agridron.Backend.API.FlightOperations.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

internal sealed class MaintenanceRecordConfiguration : IEntityTypeConfiguration<MaintenanceRecord>
{
    private static readonly ValueConverter<MaintenanceRecordId, long> MaintenanceRecordIdConverter =
        new(v => v.Value, v => new MaintenanceRecordId(v));

    private static readonly ValueConverter<DroneId, long> DroneIdConverter =
        new(v => v.Value, v => new DroneId(v));

    public void Configure(EntityTypeBuilder<MaintenanceRecord> builder)
    {
        builder.ToTable("maintenance_records");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasColumnName("id")
            .HasConversion(MaintenanceRecordIdConverter)
            .ValueGeneratedOnAdd();

        builder.Property(m => m.DroneId)
            .HasColumnName("drone_id")
            .HasConversion(DroneIdConverter)
            .IsRequired();

        builder.Property(m => m.PerformedAt)
            .HasColumnName("performed_at")
            .IsRequired();

        builder.Property(m => m.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(m => m.Technician)
            .HasColumnName("technician")
            .IsRequired()
            .HasMaxLength(MaintenanceRecord.MaxTechnicianLength);

        builder.Property(m => m.Description)
            .HasColumnName("description")
            .HasMaxLength(MaintenanceRecord.MaxDescriptionLength);

        builder.Property(m => m.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(m => m.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasIndex(m => m.DroneId)
            .HasDatabaseName("ix_maintenance_records_drone_id");
    }
}
