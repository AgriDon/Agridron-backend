using Agridron.Backend.API.FlightOperations.Domain.Model.Aggregates;
using Agridron.Backend.API.FlightOperations.Domain.Model.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Agridron.Backend.API.FlightOperations.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

internal sealed class IncidentConfiguration : IEntityTypeConfiguration<Incident>
{
    private static readonly ValueConverter<IncidentId, long> IncidentIdConverter =
        new(v => v.Value, v => new IncidentId(v));

    private static readonly ValueConverter<MissionId, long> MissionIdConverter =
        new(v => v.Value, v => new MissionId(v));

    private static readonly ValueConverter<DroneId, long> DroneIdConverter =
        new(v => v.Value, v => new DroneId(v));

    public void Configure(EntityTypeBuilder<Incident> builder)
    {
        builder.ToTable("incidents");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .HasColumnName("id")
            .HasConversion(IncidentIdConverter)
            .ValueGeneratedOnAdd();

        builder.Property(i => i.Type)
            .HasColumnName("type")
            .IsRequired()
            .HasMaxLength(Incident.MaxTypeLength);

        builder.Property(i => i.Description)
            .HasColumnName("description")
            .IsRequired()
            .HasMaxLength(Incident.MaxDescriptionLength);

        builder.Property(i => i.OccurredAt)
            .HasColumnName("occurred_at")
            .IsRequired();

        builder.Property(i => i.MissionId)
            .HasColumnName("mission_id")
            .HasConversion(MissionIdConverter);

        builder.Property(i => i.DroneId)
            .HasColumnName("drone_id")
            .HasConversion(DroneIdConverter);

        builder.Property(i => i.Severity)
            .HasColumnName("severity")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(i => i.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(i => i.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasIndex(i => i.MissionId)
            .HasDatabaseName("ix_incidents_mission_id");

        builder.HasIndex(i => i.DroneId)
            .HasDatabaseName("ix_incidents_drone_id");

        builder.HasIndex(i => i.Severity)
            .HasDatabaseName("ix_incidents_severity");
    }
}
