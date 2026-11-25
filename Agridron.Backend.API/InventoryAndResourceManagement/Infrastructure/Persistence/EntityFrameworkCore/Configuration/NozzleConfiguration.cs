
using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.ValueObjects.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Agridron.Backend.API.InventoryAndResourceManagement.Infrastructure.Persistence.EntityFrameworkCore.Configuration;


internal sealed class NozzleConfiguration : IEntityTypeConfiguration<Nozzle>
{
    private static readonly ValueConverter<NozzleId, long> NozzleIdConverter =
        new(v => v.Value, v => new NozzleId(v));

    public void Configure(EntityTypeBuilder<Nozzle> builder)
    {
        builder.ToTable("nozzles");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Id)
            .HasColumnName("id")
            .HasConversion(NozzleIdConverter)
            .ValueGeneratedOnAdd();

        builder.Property(n => n.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(n => n.DiameterMm)
            .HasColumnName("diameter_mm")
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(n => n.StockUnits)
            .HasColumnName("stock_units")
            .IsRequired();

        builder.Property(n => n.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(n => n.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(n => n.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(n => n.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasIndex(n => n.Status)
            .HasDatabaseName("ix_nozzles_status");
    }
}
