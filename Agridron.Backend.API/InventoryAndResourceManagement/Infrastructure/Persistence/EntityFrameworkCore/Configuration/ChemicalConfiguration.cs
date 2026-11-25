using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.ValueObjects.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Agridron.Backend.API.InventoryAndResourceManagement.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

internal sealed class ChemicalConfiguration : IEntityTypeConfiguration<Chemical>
{
    private static readonly ValueConverter<ChemicalId, long> ChemicalIdConverter =
        new(v => v.Value, v => new ChemicalId(v));

    // The expiration is a calendar day: stored in a DATE column.
    private static readonly ValueConverter<DateOnly, DateTime> DateOnlyConverter =
        new(d => d.ToDateTime(TimeOnly.MinValue), dt => DateOnly.FromDateTime(dt));

    public void Configure(EntityTypeBuilder<Chemical> builder)
    {
        builder.ToTable("chemicals");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .HasConversion(ChemicalIdConverter)
            .ValueGeneratedOnAdd();

        builder.Property(c => c.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(Chemical.MaxNameLength);

        builder.Property(c => c.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(c => c.StockLiters)
            .HasColumnName("stock_liters")
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(c => c.MinimumStockLiters)
            .HasColumnName("minimum_stock_liters")
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(c => c.ExpirationDate)
            .HasColumnName("expiration_date")
            .HasConversion(DateOnlyConverter)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(c => c.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasIndex(c => c.Type)
            .HasDatabaseName("ix_chemicals_type");
    }
}
