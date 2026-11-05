using Agridron.Backend.API.FieldManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Agridron.Backend.API.Tests.FieldManagement.Infrastructure.Persistence;

/// <summary>
///     Verifies the EF Core model without touching the database.
/// </summary>
public class FieldManagementModelTests
{
    private static AppDbContext CreateContext()
    {
        return new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseMySQL(MySqlTestDatabase.ConnectionString)
                .Options);
    }

    [Fact]
    public void Model_ShouldMapTheFieldManagementTables()
    {
        // Arrange
        using var context = CreateContext();

        // Act & Assert
        context.Model.FindEntityType(typeof(Farm))!.GetTableName().Should().Be("farms");
        context.Model.FindEntityType(typeof(Crop))!.GetTableName().Should().Be("crops");
        context.Model.FindEntityType(typeof(Parcel))!.GetTableName().Should().Be("parcels");
        context.Model.FindEntityType(typeof(FumigationArea))!.GetTableName().Should().Be("fumigation_areas");
    }

    [Fact]
    public void Model_ShouldConvertEveryAggregateIdentityToLong()
    {
        // Arrange
        using var context = CreateContext();
        var types = new[] { typeof(Farm), typeof(Crop), typeof(Parcel), typeof(FumigationArea) };

        // Act & Assert
        foreach (var type in types)
        {
            var property = context.Model.FindEntityType(type)!.FindProperty("Id");

            property.Should().NotBeNull();
            property!.GetValueConverter().Should().NotBeNull($"{type.Name}.Id should use a value converter");
            property.GetColumnType().Should().Be("bigint");
        }
    }

    [Fact]
    public void Model_ShouldPersistTheGeometryAsJsonAndComputeTheSpatialColumns()
    {
        // Arrange
        using var context = CreateContext();

        var parcel = context.Model.FindEntityType(typeof(Parcel))!;
        var fumigationArea = context.Model.FindEntityType(typeof(FumigationArea))!;

        // Act & Assert
        foreach (var entity in new[] { parcel, fumigationArea })
        {
            var geometry = entity.FindProperty(nameof(Parcel.Geometry))!;
            geometry.GetColumnName().Should().Be("geometry_json");
            geometry.GetColumnType().Should().Be("LONGTEXT");
            geometry.GetValueConverter().Should().NotBeNull();

            var area = entity.FindProperty(nameof(Parcel.AreaHectares))!;
            area.GetColumnName().Should().Be("area_hectares");
            area.GetColumnType().Should().Be("DECIMAL(10,2)");
            area.GetComputedColumnSql().Should().NotBeNullOrWhiteSpace();
        }
    }

    [Fact]
    public void Model_ShouldExposeTheForeignKeysAsIndexes()
    {
        // Arrange
        using var context = CreateContext();

        // Act
        var parcelIndexes = context.Model.FindEntityType(typeof(Parcel))!
            .GetIndexes()
            .Select(index => index.GetDatabaseName())
            .ToList();

        var fumigationAreaIndexes = context.Model.FindEntityType(typeof(FumigationArea))!
            .GetIndexes()
            .Select(index => index.GetDatabaseName())
            .ToList();

        var cropIndexes = context.Model.FindEntityType(typeof(Crop))!
            .GetIndexes()
            .Select(index => index.GetDatabaseName())
            .ToList();

        // Assert
        parcelIndexes.Should().Contain(["ix_parcels_farm_id", "ix_parcels_crop_id"]);
        fumigationAreaIndexes.Should().Contain("ix_fumigation_areas_parcel_id");
        cropIndexes.Should().Contain("ux_crops_name_variety");
    }
}
