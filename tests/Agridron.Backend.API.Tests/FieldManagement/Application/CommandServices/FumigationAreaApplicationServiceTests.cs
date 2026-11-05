using Agridron.Backend.API.FieldManagement.Application.Internal.CommandServices;
using Agridron.Backend.API.FieldManagement.Application.Internal.QueryServices;
using Agridron.Backend.API.FieldManagement.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Agridron.Backend.API.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Agridron.Backend.API.Tests.FieldManagement.Infrastructure.Persistence;

namespace Agridron.Backend.API.Tests.FieldManagement.Application.CommandServices;

[Collection(FieldManagementDatabaseCollection.Name)]
public class FumigationAreaApplicationServiceTests(FieldManagementDatabaseFixture fixture)
    : PersistenceTestBase(fixture)
{
    private FumigationAreaCommandService CreateCommandService()
    {
        return new FumigationAreaCommandService(
            new FumigationAreaRepository(Context),
            new ParcelRepository(Context),
            new UnitOfWork(Context));
    }

    private FumigationAreaQueryService CreateQueryService()
    {
        return new FumigationAreaQueryService(new FumigationAreaRepository(Context));
    }

    [MySqlFact]
    public async Task CreateAsync_ShouldPersistAnAreaThatFitsInTheParcelAsync()
    {
        // Arrange
        var farm = await AddFarmAsync();
        var crop = await AddCropAsync();
        var parcel = await AddParcelAsync(farm, crop);

        // Act
        var result = await CreateCommandService().CreateAsync(
            parcel.Id.Value,
            TestGeometry.SquareGeoJson(-78.50, -0.20, -78.495, -0.195));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.AreaHectares.Should().BeGreaterThan(0m);
        result.Value.ParcelId.Should().Be(parcel.Id);

        (await CreateQueryService().GetByIdAsync(result.Value.Id.Value)).IsSuccess.Should().BeTrue();
    }

    [MySqlFact]
    public async Task CreateAsync_ShouldRejectAnAreaLargerThanTheParcelAsync()
    {
        // Arrange
        var farm = await AddFarmAsync();
        var crop = await AddCropAsync();
        var parcel = await AddParcelAsync(farm, crop);

        // Act
        var result = await CreateCommandService().CreateAsync(
            parcel.Id.Value,
            TestGeometry.SquareGeoJson(-78.51, -0.21, -78.49, -0.19));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("FumigationAreaExceedsParcelArea");
    }

    [MySqlFact]
    public async Task CreateAsync_ShouldReturnNotFoundForAnUnknownParcelAsync()
    {
        // Act
        var result = await CreateCommandService().CreateAsync(
            9999,
            TestGeometry.SquareGeoJson(-78.50, -0.20, -78.495, -0.195));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("ParcelNotFound");
    }

    [MySqlFact]
    public async Task DeleteAsync_ShouldRemoveTheAreaAsync()
    {
        // Arrange
        var farm = await AddFarmAsync();
        var crop = await AddCropAsync();
        var parcel = await AddParcelAsync(farm, crop);
        var area = await AddFumigationAreaAsync(parcel);

        // Act
        var result = await CreateCommandService().DeleteAsync(area.Id.Value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        (await CreateQueryService().GetByIdAsync(area.Id.Value)).IsFailure.Should().BeTrue();
    }
}
