using Agridron.Backend.API.FieldManagement.Application.Internal.CommandServices;
using Agridron.Backend.API.FieldManagement.Application.Internal.QueryServices;
using Agridron.Backend.API.FieldManagement.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Agridron.Backend.API.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Agridron.Backend.API.Tests.FieldManagement.Infrastructure.Persistence;

namespace Agridron.Backend.API.Tests.FieldManagement.Application.CommandServices;

[Collection(FieldManagementDatabaseCollection.Name)]
public class FarmApplicationServiceTests(FieldManagementDatabaseFixture fixture) : PersistenceTestBase(fixture)
{
    private FarmCommandService CreateCommandService()
    {
        return new FarmCommandService(
            new FarmRepository(Context),
            new ParcelRepository(Context),
            new UnitOfWork(Context));
    }

    private FarmQueryService CreateQueryService()
    {
        return new FarmQueryService(new FarmRepository(Context));
    }

    [MySqlFact]
    public async Task CreateAsync_ShouldPersistTheFarmAndRaiseItsIdentityAsync()
    {
        // Act
        var result = await CreateCommandService().CreateAsync(
            "  El Mirador  ",
            "Cauca",
            15,
            "https://example.com/farm.jpg");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Value.Should().BeGreaterThan(0);
        result.Value.Name.Should().Be("El Mirador");

        var query = await CreateQueryService().GetByIdAsync(result.Value.Id.Value);
        query.IsSuccess.Should().BeTrue();
        query.Value!.OwnerId.Should().Be(15);
        query.Value.ImageUrl.Should().Be("https://example.com/farm.jpg");
    }

    [MySqlFact]
    public async Task CreateAsync_ShouldRejectAnInvalidOwnerAsync()
    {
        // Act
        var result = await CreateCommandService().CreateAsync("El Mirador", "Cauca", 0, null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("FarmOwnerInvalid");
    }

    [MySqlFact]
    public async Task UpdateAsync_ShouldReturnNotFoundForAnUnknownFarmAsync()
    {
        // Act
        var result = await CreateCommandService().UpdateAsync(9999, "Nueva", "Cauca", 1, null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("FarmNotFound");
    }

    [MySqlFact]
    public async Task DeleteAsync_ShouldRejectAFarmWithParcelsAsync()
    {
        // Arrange
        var farm = await AddFarmAsync();
        var crop = await AddCropAsync();
        await AddParcelAsync(farm, crop);

        // Act
        var result = await CreateCommandService().DeleteAsync(farm.Id.Value);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("FarmHasParcels");
    }

    [MySqlFact]
    public async Task DeleteAsync_ShouldRemoveAnEmptyFarmAsync()
    {
        // Arrange
        var farm = await AddFarmAsync();

        // Act
        var result = await CreateCommandService().DeleteAsync(farm.Id.Value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        (await CreateQueryService().GetByIdAsync(farm.Id.Value)).IsFailure.Should().BeTrue();
    }
}
