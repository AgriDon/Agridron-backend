using Agridron.Backend.API.FieldManagement.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

namespace Agridron.Backend.API.Tests.FieldManagement.Infrastructure.Persistence;

[Collection(FieldManagementDatabaseCollection.Name)]
public class FarmRepositoryTests(FieldManagementDatabaseFixture fixture) : PersistenceTestBase(fixture)
{
    [MySqlFact]
    public async Task AddAsync_ShouldRoundTripThroughFindByIdAsync()
    {
        // Arrange
        var farm = await AddFarmAsync("El Paraiso", 42);

        // Act
        await using var context = CreateNewContext();
        var found = await new FarmRepository(context).FindByIdAsync(farm.Id.Value);

        // Assert
        found.Should().NotBeNull();
        found!.Id.Should().Be(farm.Id);
        found.Name.Should().Be("El Paraiso");
        found.Location.Should().Be("Valle del Cauca");
        found.OwnerId.Should().Be(42);
    }

    [MySqlFact]
    public async Task ListByOwnerIdAsync_ShouldReturnOnlyTheOwnerFarmsOrderedByNameAsync()
    {
        // Arrange
        await AddFarmAsync("Zeta", ownerId: 7);
        await AddFarmAsync("Alfa", ownerId: 7);
        await AddFarmAsync("Otra", ownerId: 8);

        // Act
        var farms = await new FarmRepository(Context).ListByOwnerIdAsync(7);

        // Assert
        farms.Should().HaveCount(2);
        farms.Select(f => f.Name).Should().ContainInOrder("Alfa", "Zeta");
        farms.Should().OnlyContain(f => f.OwnerId == 7);
    }

    [MySqlFact]
    public async Task ExistsWithNameAsync_ShouldDetectDuplicatesAndRespectTheExcludedIdAsync()
    {
        // Arrange
        var farm = await AddFarmAsync("La Union");
        var repository = new FarmRepository(Context);

        // Act & Assert
        (await repository.ExistsWithNameAsync("La Union")).Should().BeTrue();
        (await repository.ExistsWithNameAsync("La Union", farm.Id.Value)).Should().BeFalse();
        (await repository.ExistsWithNameAsync("No Existe")).Should().BeFalse();
    }
}
