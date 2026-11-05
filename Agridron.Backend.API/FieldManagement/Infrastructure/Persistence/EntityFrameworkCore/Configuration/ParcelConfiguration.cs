using Agridron.Backend.API.FieldManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.FieldManagement.Domain.Model.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Agridron.Backend.API.FieldManagement.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

/// <summary>
///     Entity Framework Core configuration for the <see cref="Parcel" /> aggregate.
/// </summary>
internal sealed class ParcelConfiguration : IEntityTypeConfiguration<Parcel>
{
    private static readonly ValueConverter<ParcelId, long> ParcelIdConverter =
        new(v => v.Value, v => new ParcelId(v));

    private static readonly ValueConverter<FarmId, long> FarmIdConverter =
        new(v => v.Value, v => new FarmId(v));

    private static readonly ValueConverter<CropId, long> CropIdConverter =
        new(v => v.Value, v => new CropId(v));

    private static readonly ValueConverter<GeoJsonPolygon, string> GeoJsonPolygonConverter =
        new(v => v.GeoJson, v => GeoJsonPolygon.FromGeoJson(v));

    public void Configure(EntityTypeBuilder<Parcel> builder)
    {
        builder.ToTable("parcels");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id")
            .HasConversion(ParcelIdConverter)
            .ValueGeneratedOnAdd();

        builder.Property(p => p.FarmId)
            .HasColumnName("farm_id")
            .HasConversion(FarmIdConverter)
            .IsRequired();

        builder.Property(p => p.CropId)
            .HasColumnName("crop_id")
            .HasConversion(CropIdConverter)
            .IsRequired();

        builder.Property(p => p.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(Parcel.MaxNameLength);

        // The GeoJSON string provided by the client (canonical serialization).
        builder.Property(p => p.Geometry)
            .HasColumnName("geometry_json")
            .HasColumnType("LONGTEXT")
            .HasConversion(GeoJsonPolygonConverter)
            .IsRequired();

        // Area in hectares, derived from the geometry.
        builder.Property(p => p.AreaHectares)
            .HasColumnName("area_hectares")
            .HasColumnType("DECIMAL(10,2)")
            .HasComputedColumnSql(
                "ST_Area(ST_GeomFromGeoJSON(`geometry_json`, 4326)) / 10000",
                stored: true);

        builder.Property(p => p.ImageUrl)
            .HasColumnName("image_url")
            .HasMaxLength(Parcel.MaxImageUrlLength);

        builder.HasIndex(p => p.FarmId)
            .HasDatabaseName("ix_parcels_farm_id");

        builder.HasIndex(p => p.CropId)
            .HasDatabaseName("ix_parcels_crop_id");
    }
}
