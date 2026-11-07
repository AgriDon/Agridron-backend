using Agridron.Backend.API.FieldManagement.Application.Internal.CommandServices;
using Agridron.Backend.API.FieldManagement.Application.Internal.QueryServices;
using Agridron.Backend.API.FieldManagement.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Agridron.Backend.API.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Agridron.Backend.API.Tests.FieldManagement.Infrastructure.Persistence;

namespace Agridron.Backend.API.Tests.FieldManagement.Application.CommandServices;

[Collection(FieldManagementDatabaseCollection.Name)]
public class CropApplicationServiceTests(FieldManagementDatabaseFixture fixture) : PersistenceTestBase(fixture)
{
    private CropCommandService CreateCommandService()
    {
        return new CropCommandService(
            new CropRepository(Context),
            new ParcelRepository(Context),
            new UnitOfWork(Context));
    }

    private CropQueryService CreateQueryService()
    {
        return new CropQueryService(new CropRepository(Context));
    }

    [MySqlFact]
    public async Task CreateAsync_ShouldPersistTheCropAsync()
    {
        // Act
        var result = await CreateCommandService().CreateAsync("  Cafe  ", "  Castillo  ");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Value.Should().BeGreaterThan(0);
        result.Value.Name.Should().Be("Cafe");
        result.Value.Variety.Should().Be("Castillo");

        (await CreateQueryService().GetByIdAsync(result.Value.Id.Value)).IsSuccess.Should().BeTrue();
    }

    [MySqlFact]
    public async Task CreateAsync_ShouldRejectADuplicatedCropAsync()
    {
        // Arrange
        await AddCropAsync("Maiz", "Amarillo");

        // Act
        var result = await CreateCommandService().CreateAsync("Maiz", "Amarillo");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("CropAlreadyExists");
    }

    [MySqlFact]
    public async Task UpdateAsync_ShouldRejectADuplicateAndRespectItsOwnIdAsync()
    {
        // Arrange
        var crop = await AddCropAsync("Maiz", "Amarillo");
        await AddCropAsync("Cafe", "Castillo");
        var service = CreateCommandService();

        // Act
        var duplicated = await service.UpdateAsync(crop.Id.Value, "Cafe", "Castillo");
        var sameValues = await service.UpdateAsync(crop.Id.Value, "Maiz", "Amarillo");

        // Assert
        duplicated.Error!.Code.Should().Be("CropAlreadyExists");
        sameValues.IsSuccess.Should().BeTrue();
    }

    [MySqlFact]
    public async Task DeleteAsync_ShouldRejectACropAssignedToAParcelAsync()
    {
        // Arrange
        var farm = await AddFarmAsync();
        var crop = await AddCropAsync();
        await AddParcelAsync(farm, crop);

        // Act
        var result = await CreateCommandService().DeleteAsync(crop.Id.Value);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("CropHasParcels");
    }
}
