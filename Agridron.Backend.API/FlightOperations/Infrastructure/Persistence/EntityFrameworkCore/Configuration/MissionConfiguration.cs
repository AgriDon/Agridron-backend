using Agridron.Backend.API.FlightOperations.Domain.Model.Aggregates;
using Agridron.Backend.API.FlightOperations.Domain.Model.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Agridron.Backend.API.FlightOperations.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

internal sealed class MissionConfiguration : IEntityTypeConfiguration<Mission>
{
    private static readonly ValueConverter<MissionId, long> MissionIdConverter =
        new(v => v.Value, v => new MissionId(v));

    private static readonly ValueConverter<DroneId, long> DroneIdConverter =
        new(v => v.Value, v => new DroneId(v));

    public void Configure(EntityTypeBuilder<Mission> builder)
    {
        builder.ToTable("missions");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasColumnName("id")
            .HasConversion(MissionIdConverter)
            .ValueGeneratedOnAdd();

        builder.Property(m => m.Code)
            .HasColumnName("code")
            .IsRequired()
            .HasMaxLength(Mission.MaxCodeLength);

        builder.Property(m => m.FarmId)
            .HasColumnName("farm_id");

        builder.Property(m => m.ParcelId)
            .HasColumnName("parcel_id");

        builder.Property(m => m.DroneId)
            .HasColumnName("drone_id")
            .HasConversion(DroneIdConverter);

        builder.Property(m => m.ChemicalId)
            .HasColumnName("chemical_id");

        builder.Property(m => m.OperatorId)
            .HasColumnName("operator_id");

        builder.Property(m => m.FarmArea)
            .HasColumnName("farm_area")
            .IsRequired()
            .HasMaxLength(Mission.MaxFarmAreaLength);

        builder.Property(m => m.CropType)
            .HasColumnName("crop_type")
            .IsRequired()
            .HasMaxLength(Mission.MaxCropTypeLength);

        builder.Property(m => m.OperatorName)
            .HasColumnName("operator_name")
            .HasMaxLength(Mission.MaxOperatorNameLength);

        builder.Property(m => m.TreatmentType)
            .HasColumnName("treatment_type")
            .HasMaxLength(Mission.MaxTreatmentTypeLength);

        builder.Property(m => m.ProductName)
            .HasColumnName("product_name")
            .HasMaxLength(Mission.MaxProductNameLength);

        builder.Property(m => m.ProductDose)
            .HasColumnName("product_dose")
            .HasPrecision(8, 2);

        builder.Property(m => m.Notes)
            .HasColumnName("notes")
            .HasMaxLength(Mission.MaxNotesLength);

        builder.Property(m => m.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(m => m.ScheduledAt)
            .HasColumnName("scheduled_at")
            .IsRequired();

        builder.Property(m => m.PlannedAreaHectares)
            .HasColumnName("planned_area_hectares")
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(m => m.TreatedAreaHectares)
            .HasColumnName("treated_area_hectares")
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(m => m.Progress)
            .HasColumnName("progress")
            .IsRequired();

        builder.Property(m => m.StartedAt)
            .HasColumnName("started_at");

        builder.Property(m => m.CompletedAt)
            .HasColumnName("completed_at");

        builder.Property(m => m.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(m => m.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(m => m.UpdatedAt)
            .HasColumnName("updated_at");

        builder.OwnsOne(m => m.FlightParameters, fp =>
        {
            fp.Property(p => p.AltitudeMeters)
                .HasColumnName("flight_altitude_meters")
                .HasPrecision(5, 2);

            fp.Property(p => p.SpeedMetersPerSecond)
                .HasColumnName("flight_speed_mps")
                .HasPrecision(5, 2);

            fp.Property(p => p.SprayRateLitersPerHectare)
                .HasColumnName("flight_spray_rate_lph")
                .HasPrecision(6, 2);

            fp.Property(p => p.OverlapPercentage)
                .HasColumnName("flight_overlap_percentage")
                .HasPrecision(5, 2);
        });

        builder.OwnsOne(m => m.LastPausePoint, pp =>
        {
            pp.Property(p => p.Latitude)
                .HasColumnName("last_pause_latitude")
                .HasPrecision(9, 6);

            pp.Property(p => p.Longitude)
                .HasColumnName("last_pause_longitude")
                .HasPrecision(9, 6);
        });

        builder.HasIndex(m => m.Code)
            .IsUnique()
            .HasDatabaseName("ux_missions_code");

        builder.HasIndex(m => m.FarmId)
            .HasDatabaseName("ix_missions_farm_id");

        builder.HasIndex(m => m.ParcelId)
            .HasDatabaseName("ix_missions_parcel_id");

        builder.HasIndex(m => m.DroneId)
            .HasDatabaseName("ix_missions_drone_id");

        builder.HasIndex(m => m.Status)
            .HasDatabaseName("ix_missions_status");

        builder.HasIndex(m => m.ScheduledAt)
            .HasDatabaseName("ix_missions_scheduled_at");
    }
}
