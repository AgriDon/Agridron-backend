using Agridron.Backend.API.FieldManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.FieldManagement.Domain.Model.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Agridron.Backend.API.FieldManagement.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

/// <summary>
///     Entity Framework Core configuration for the <see cref="Farm" /> aggregate.
/// </summary>
internal sealed class FarmConfiguration : IEntityTypeConfiguration<Farm>
{
    private static readonly ValueConverter<FarmId, long> FarmIdConverter =
        new(v => v.Value, v => new FarmId(v));

    public void Configure(EntityTypeBuilder<Farm> builder)
    {
        builder.ToTable("farms");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Id)
            .HasColumnName("id")
            .HasConversion(FarmIdConverter)
            .ValueGeneratedOnAdd();

        builder.Property(f => f.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(Farm.MaxNameLength);

        builder.Property(f => f.Location)
            .HasColumnName("location")
            .IsRequired()
            .HasMaxLength(Farm.MaxLocationLength);

        builder.Property(f => f.OwnerId)
            .HasColumnName("owner_id")
            .IsRequired();

        builder.Property(f => f.ImageUrl)
            .HasColumnName("image_url")
            .HasMaxLength(Farm.MaxImageUrlLength);

        builder.HasIndex(f => f.OwnerId)
            .HasDatabaseName("ix_farms_owner_id");

        builder.HasIndex(f => f.Name)
            .HasDatabaseName("ix_farms_name");
    }
}
