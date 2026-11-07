using Agridron.Backend.API.FieldManagement.Application.Internal.QueryServices;
using Agridron.Backend.API.FieldManagement.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Agridron.Backend.API.Shared.Application.Model;
using Agridron.Backend.API.Tests.FieldManagement.Infrastructure.Persistence;

namespace Agridron.Backend.API.Tests.FieldManagement.Application.QueryServices;

[Collection(FieldManagementDatabaseCollection.Name)]
public class FieldManagementQueryServiceTests(FieldManagementDatabaseFixture fixture)
    : PersistenceTestBase(fixture)
{
    private FarmQueryService CreateFarmQueryService() => new(new FarmRepository(Context));

    private CropQueryService CreateCropQueryService() => new(new CropRepository(Context));

    private ParcelQueryService CreateParcelQueryService() => new(new ParcelRepository(Context));

    private FumigationAreaQueryService CreateFumigationAreaQueryService() =>
        new(new FumigationAreaRepository(Context));

    [MySqlFact]
    public async Task GetByIdAsync_ShouldReturnNotFoundForEveryAggregateAsync()
    {
        // Act & Assert
        (await CreateFarmQueryService().GetByIdAsync(9999)).Error!.Code.Should().Be("FarmNotFound");
        (await CreateCropQueryService().GetByIdAsync(9999)).Error!.Code.Should().Be("CropNotFound");
        (await CreateParcelQueryService().GetByIdAsync(9999)).Error!.Code.Should().Be("ParcelNotFound");
        (await CreateFumigationAreaQueryService().GetByIdAsync(9999)).Error!.Code
            .Should().Be("FumigationAreaNotFound");
    }

    [MySqlFact]
    public async Task FarmListAsync_ShouldFilterByOwnerAndPaginateAsync()
    {
        // Arrange
        await AddFarmAsync("Zeta", ownerId: 7);
        await AddFarmAsync("Alfa", ownerId: 7);
        await AddFarmAsync("Beta", ownerId: 7);
        await AddFarmAsync("Otra", ownerId: 8);

        // Act
        var page = await CreateFarmQueryService().ListAsync(
            7,
            PaginationParameters.Create(0, 2));

        // Assert
        page.IsSuccess.Should().BeTrue();
        page.Value!.TotalCount.Should().Be(3);
        page.Value.Items.Should().HaveCount(2);
        page.Value.Items.Select(farm => farm.Name).Should().ContainInOrder("Alfa", "Beta");
        page.Value.HasNextPage.Should().BeTrue();
    }

    [MySqlFact]
    public async Task CropListAsync_ShouldReturnEveryCropOrderedByNameAsync()
    {
        // Arrange
        await AddCropAsync("Zapallo", "Criollo");
        await AddCropAsync("Arroz", "Fedearroz");

        // Act
        var result = await CreateCropQueryService().ListAsync(PaginationParameters.Unpaged);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.TotalCount.Should().Be(2);
        result.Value.Items.Select(crop => crop.Name).Should().ContainInOrder("Arroz", "Zapallo");
    }

    [MySqlFact]
    public async Task ParcelListAsync_ShouldFilterByFarmAsync()
    {
        // Arrange
        var farm = await AddFarmAsync();
        var otherFarm = await AddFarmAsync("Otro Rancho");
        var crop = await AddCropAsync();
        await AddParcelAsync(farm, crop, "Lote B");
        await AddParcelAsync(farm, crop, "Lote A");
        await AddParcelAsync(otherFarm, crop, "Lote C");

        // Act
        var result = await CreateParcelQueryService().ListAsync(
            farm.Id.Value,
            PaginationParameters.Unpaged);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.TotalCount.Should().Be(2);
        result.Value.Items.Select(parcel => parcel.Name).Should().ContainInOrder("Lote A", "Lote B");
    }

    [MySqlFact]
    public async Task FumigationAreaListAsync_ShouldFilterByParcelAsync()
    {
        // Arrange
        var farm = await AddFarmAsync();
        var crop = await AddCropAsync();
        var parcel = await AddParcelAsync(farm, crop);
        var otherParcel = await AddParcelAsync(farm, crop, "Lote 2");
        await AddFumigationAreaAsync(parcel);
        await AddFumigationAreaAsync(parcel);
        await AddFumigationAreaAsync(otherParcel);

        // Act
        var result = await CreateFumigationAreaQueryService().ListAsync(
            parcel.Id.Value,
            PaginationParameters.Unpaged);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.TotalCount.Should().Be(2);
        result.Value.Items.Should().OnlyContain(area => area.ParcelId == parcel.Id);
    }
}
