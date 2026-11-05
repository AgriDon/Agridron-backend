using Agridron.Backend.API.FieldManagement.Application.Internal.CommandServices;
using Agridron.Backend.API.FieldManagement.Application.Internal.QueryServices;
using Agridron.Backend.API.FieldManagement.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Agridron.Backend.API.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Agridron.Backend.API.Tests.FieldManagement.Infrastructure.Persistence;

namespace Agridron.Backend.API.Tests.FieldManagement.Application.CommandServices;

[Collection(FieldManagementDatabaseCollection.Name)]
public class ParcelApplicationServiceTests(FieldManagementDatabaseFixture fixture) : PersistenceTestBase(fixture)
{
    private ParcelCommandService CreateCommandService()
    {
        return new ParcelCommandService(
            new ParcelRepository(Context),
            new FarmRepository(Context),
            new CropRepository(Context),
            new FumigationAreaRepository(Context),
            new UnitOfWork(Context));
    }

    private ParcelQueryService CreateQueryService()
    {
        return new ParcelQueryService(new ParcelRepository(Context));
    }

    [MySqlFact]
    public async Task CreateAsync_ShouldPersistTheParcelAndDeriveItsAreaAsync()
    {
        // Arrange
        var farm = await AddFarmAsync();
        var crop = await AddCropAsync();

        // Act
        var result = await CreateCommandService().CreateAsync(
            farm.Id.Value,
            crop.Id.Value,
            "Lote Norte",
            TestGeometry.SquareGeoJson(-78.50, -0.20, -78.49, -0.19),
            null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.AreaHectares.Should().BeGreaterThan(0m);

        var query = await CreateQueryService().GetByIdAsync(result.Value.Id.Value);
        query.IsSuccess.Should().BeTrue();
        query.Value!.Name.Should().Be("Lote Norte");
        query.Value.FarmId.Should().Be(farm.Id);
        query.Value.CropId.Should().Be(crop.Id);
    }

    [MySqlFact]
    public async Task CreateAsync_ShouldRejectAnUnknownFarmAsync()
    {
        // Arrange
        var crop = await AddCropAsync();

        // Act
        var result = await CreateCommandService().CreateAsync(
            9999,
            crop.Id.Value,
            "Lote Norte",
            TestGeometry.SquareGeoJson(-78.50, -0.20, -78.49, -0.19),
            null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("FarmNotFound");
    }

    [MySqlFact]
    public async Task CreateAsync_ShouldRejectAnInvalidGeometryAsync()
    {
        // Arrange
        var farm = await AddFarmAsync();
        var crop = await AddCropAsync();

        // Act
        var result = await CreateCommandService().CreateAsync(
            farm.Id.Value,
            crop.Id.Value,
            "Lote Norte",
            "{\"type\":\"Point\",\"coordinates\":[0,0]}",
            null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("InvalidPolygonType");
    }

    [MySqlFact]
    public async Task DeleteAsync_ShouldRejectAParcelWithFumigationAreasAsync()
    {
        // Arrange
        var farm = await AddFarmAsync();
        var crop = await AddCropAsync();
        var parcel = await AddParcelAsync(farm, crop);
        await AddFumigationAreaAsync(parcel);

        // Act
        var result = await CreateCommandService().DeleteAsync(parcel.Id.Value);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("ParcelHasFumigationAreas");
    }
}
