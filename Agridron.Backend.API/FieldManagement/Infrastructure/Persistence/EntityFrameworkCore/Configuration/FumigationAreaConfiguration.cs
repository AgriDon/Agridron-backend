using Agridron.Backend.API.FieldManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.FieldManagement.Domain.Model.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agridron.Backend.API.FieldManagement.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

/// <summary>
///     Entity Framework Core configuration for the <see cref="FumigationArea" /> aggregate.
/// </summary>
internal sealed class FumigationAreaConfiguration : IEntityTypeConfiguration<FumigationArea>
{
    public void Configure(EntityTypeBuilder<FumigationArea> builder)
    {
        builder.ToTable("fumigation_areas");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(f => f.ParcelId)
            .HasColumnName("parcel_id")
            .IsRequired();

        // The GeoJSON string provided by the client (canonical serialization).
        builder.Property(f => f.Geometry)
            .HasColumnName("geometry_json")
            .HasColumnType("LONGTEXT")
            .IsRequired();

        // Native spatial column generated from the JSON.
        builder.Property(f => f.Geometry)
            .HasColumnName("geometry_native")
            .HasColumnType("GEOMETRY SRID 4326")
            .HasComputedColumnSql(
                "ST_GeomFromGeoJSON(`geometry_json`, 4326)",
                stored: true);

        // Area in hectares, derived from the geometry.
        builder.Property(f => f.AreaHectares)
            .HasColumnName("area_hectares")
            .HasColumnType("DECIMAL(10,2)")
            .HasComputedColumnSql(
                "ST_Area(`geometry_native`) / 10000",
                stored: true);

        builder.HasIndex(f => f.ParcelId)
            .HasDatabaseName("ix_fumigation_areas_parcel_id");

        // Spatial index on the native geometry column.
        builder.HasIndex(f => f.Geometry)
            .HasDatabaseName("sx_fumigation_areas_geometry");
    }
}
