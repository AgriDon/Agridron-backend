using Agridron.Backend.API.FieldManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.FieldManagement.Domain.Model.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Agridron.Backend.API.FieldManagement.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

/// <summary>
///     Entity Framework Core configuration for the <see cref="FumigationArea" /> aggregate.
/// </summary>
internal sealed class FumigationAreaConfiguration : IEntityTypeConfiguration<FumigationArea>
{
    private static readonly ValueConverter<FumigationAreaId, long> FumigationAreaIdConverter =
        new(v => v.Value, v => new FumigationAreaId(v));

    private static readonly ValueConverter<ParcelId, long> ParcelIdConverter =
        new(v => v.Value, v => new ParcelId(v));

    private static readonly ValueConverter<GeoJsonPolygon, string> GeoJsonPolygonConverter =
        new(v => v.GeoJson, v => GeoJsonPolygon.FromGeoJson(v));

    public void Configure(EntityTypeBuilder<FumigationArea> builder)
    {
        builder.ToTable("fumigation_areas");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Id)
            .HasColumnName("id")
            .HasConversion(FumigationAreaIdConverter)
            .ValueGeneratedOnAdd();

        builder.Property(f => f.ParcelId)
            .HasColumnName("parcel_id")
            .HasConversion(ParcelIdConverter)
            .IsRequired();

        // The GeoJSON string provided by the client (canonical serialization).
        builder.Property(f => f.Geometry)
            .HasColumnName("geometry_json")
            .HasColumnType("LONGTEXT")
            .HasConversion(GeoJsonPolygonConverter)
            .IsRequired();

        // Area in hectares, derived from the geometry.
        builder.Property(f => f.AreaHectares)
            .HasColumnName("area_hectares")
            .HasColumnType("DECIMAL(10,2)")
            .HasComputedColumnSql(
                "ST_Area(ST_GeomFromGeoJSON(`geometry_json`, 4326)) / 10000",
                stored: true);

        builder.HasIndex(f => f.ParcelId)
            .HasDatabaseName("ix_fumigation_areas_parcel_id");
    }
}
