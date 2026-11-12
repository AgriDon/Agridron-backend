using Agridron.Backend.API.FieldManagement.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

namespace Agridron.Backend.API.Tests.FieldManagement.Infrastructure.Persistence;

[Collection(FieldManagementDatabaseCollection.Name)]
public class FumigationAreaRepositoryTests(FieldManagementDatabaseFixture fixture) : PersistenceTestBase(fixture)
{
    [MySqlFact]
    public async Task AddAsync_ShouldPersistTheGeometryAndItsGeneratedColumnsAsync()
    {
        // Arrange
        var farm = await AddFarmAsync();
        var crop = await AddCropAsync();
        var parcel = await AddParcelAsync(farm, crop);

        // Act
        var area = await AddFumigationAreaAsync(parcel);
        var (areaHectares, srid) = await ReadComputedGeometryAsync("fumigation_areas", area.Id.Value);

        // Assert
        areaHectares.Should().BeGreaterThan(0m);
        srid.Should().Be(4326);

        await using var context = CreateNewContext();
        var found = await new FumigationAreaRepository(context).FindByIdAsync(area.Id.Value);

        found.Should().NotBeNull();
        found!.ParcelId.Should().Be(parcel.Id);
        found.Geometry.GeoJson.Should().Contain("\"Polygon\"");
    }

    [MySqlFact]
    public async Task ListByParcelIdAsync_ShouldReturnOnlyTheParcelAreasAsync()
    {
        // Arrange
        var farm = await AddFarmAsync();
        var crop = await AddCropAsync();
        var parcel = await AddParcelAsync(farm, crop);
        var otherParcel = await AddParcelAsync(farm, crop, "Lote 2");

        await AddFumigationAreaAsync(parcel);
        await AddFumigationAreaAsync(parcel);
        await AddFumigationAreaAsync(otherParcel);

        var repository = new FumigationAreaRepository(Context);

        // Act
        var areas = await repository.ListByParcelIdAsync(parcel.Id);

        // Assert
        areas.Should().HaveCount(2);
        areas.Should().OnlyContain(a => a.ParcelId == parcel.Id);
        (await repository.CountByParcelIdAsync(parcel.Id)).Should().Be(2);
        (await repository.CountByParcelIdAsync(otherParcel.Id)).Should().Be(1);
    }
}
