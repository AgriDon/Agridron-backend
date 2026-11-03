using Agridron.Backend.API.FieldManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.FieldManagement.Domain.Model.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agridron.Backend.API.FieldManagement.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

/// <summary>
///     Entity Framework Core configuration for the <see cref="Parcel" /> aggregate.
/// </summary>
internal sealed class ParcelConfiguration : IEntityTypeConfiguration<Parcel>
{
    public void Configure(EntityTypeBuilder<Parcel> builder)
    {
        builder.ToTable("parcels");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(p => p.FarmId)
            .HasColumnName("farm_id")
            .IsRequired();

        builder.Property(p => p.CropId)
            .HasColumnName("crop_id")
            .IsRequired();

        builder.Property(p => p.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(Parcel.MaxNameLength);

        // The GeoJSON string provided by the client (canonical serialization).
        builder.Property(p => p.Geometry)
            .HasColumnName("geometry_json")
            .HasColumnType("LONGTEXT")
            .IsRequired();

        // Native spatial column generated from the JSON. MySQL 8 supports
        // ST_GeomFromGeoJSON and spatial indexes on GEOMETRY columns.
        builder.Property(p => p.Geometry)
            .HasColumnName("geometry_native")
            .HasColumnType("GEOMETRY SRID 4326")
            .HasComputedColumnSql(
                "ST_GeomFromGeoJSON(`geometry_json`, 4326)",
                stored: true);

        // Area in hectares, derived from the geometry.
        builder.Property(p => p.AreaHectares)
            .HasColumnName("area_hectares")
            .HasColumnType("DECIMAL(10,2)")
            .HasComputedColumnSql(
                "ST_Area(`geometry_native`) / 10000",
                stored: true);

        builder.Property(p => p.ImageUrl)
            .HasColumnName("image_url")
            .HasMaxLength(Parcel.MaxImageUrlLength);

        builder.HasIndex(p => p.FarmId)
            .HasDatabaseName("ix_parcels_farm_id");

        builder.HasIndex(p => p.CropId)
            .HasDatabaseName("ix_parcels_crop_id");

        // Spatial index on the native geometry column.
        builder.HasIndex(p => p.Geometry)
            .HasDatabaseName("sx_parcels_geometry");
    }
}
