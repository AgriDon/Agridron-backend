using Agridron.Backend.API.FieldManagement.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

namespace Agridron.Backend.API.Tests.FieldManagement.Infrastructure.Persistence;

[Collection(FieldManagementDatabaseCollection.Name)]
public class ParcelRepositoryTests(FieldManagementDatabaseFixture fixture) : PersistenceTestBase(fixture)
{
    [MySqlFact]
    public async Task AddAsync_ShouldPersistTheGeometryAndItsGeneratedColumnsAsync()
    {
        // Arrange
        var farm = await AddFarmAsync();
        var crop = await AddCropAsync();

        // Act
        var parcel = await AddParcelAsync(farm, crop);
        var (areaHectares, srid) = await ReadComputedGeometryAsync("parcels", parcel.Id.Value);

        // Assert
        areaHectares.Should().BeGreaterThan(0m);
        srid.Should().Be(4326);

        await using var context = CreateNewContext();
        var found = await new ParcelRepository(context).FindByIdAsync(parcel.Id.Value);

        found.Should().NotBeNull();
        found!.FarmId.Should().Be(farm.Id);
        found.CropId.Should().Be(crop.Id);
        found.Geometry.GeoJson.Should().Contain("\"Polygon\"");
    }

    [MySqlFact]
    public async Task ListByFarmIdAsync_ShouldReturnOnlyTheFarmParcelsOrderedByNameAsync()
    {
        // Arrange
        var farm = await AddFarmAsync();
        var otherFarm = await AddFarmAsync("Otro Rancho");
        var crop = await AddCropAsync();

        await AddParcelAsync(farm, crop, "Lote B");
        await AddParcelAsync(farm, crop, "Lote A");
        await AddParcelAsync(otherFarm, crop, "Lote C");

        // Act
        var parcels = await new ParcelRepository(Context).ListByFarmIdAsync(farm.Id);

        // Assert
        parcels.Should().HaveCount(2);
        parcels.Select(p => p.Name).Should().ContainInOrder("Lote A", "Lote B");
    }

    [MySqlFact]
    public async Task CountByFarmIdAndCropIdAsync_ShouldCountTheRelatedParcelsAsync()
    {
        // Arrange
        var farm = await AddFarmAsync();
        var crop = await AddCropAsync();
        var otherCrop = await AddCropAsync("Cafe", "Castillo");

        await AddParcelAsync(farm, crop, "Lote 1");
        await AddParcelAsync(farm, crop, "Lote 2");
        await AddParcelAsync(farm, otherCrop, "Lote 3");

        var repository = new ParcelRepository(Context);

        // Act & Assert
        (await repository.CountByFarmIdAsync(farm.Id)).Should().Be(3);
        (await repository.CountByCropIdAsync(crop.Id)).Should().Be(2);
        (await repository.CountByCropIdAsync(otherCrop.Id)).Should().Be(1);
    }
}
