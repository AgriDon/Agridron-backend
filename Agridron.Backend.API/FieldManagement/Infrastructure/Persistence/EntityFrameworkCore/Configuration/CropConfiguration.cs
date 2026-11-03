using Agridron.Backend.API.FieldManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.FieldManagement.Domain.Model.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Agridron.Backend.API.FieldManagement.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

/// <summary>
///     Entity Framework Core configuration for the <see cref="Crop" /> aggregate.
/// </summary>
internal sealed class CropConfiguration : IEntityTypeConfiguration<Crop>
{
    public void Configure(EntityTypeBuilder<Crop> builder)
    {
        builder.ToTable("crops");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(c => c.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(Crop.MaxNameLength);

        builder.Property(c => c.Variety)
            .HasColumnName("variety")
            .IsRequired()
            .HasMaxLength(Crop.MaxVarietyLength);

        builder.HasIndex(c => new { c.Name, c.Variety })
            .IsUnique()
            .HasDatabaseName("ux_crops_name_variety");
    }
}
