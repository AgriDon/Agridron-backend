using Agridron.Backend.API.FieldManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.FieldManagement.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Agridron.Backend.API.Tests.FieldManagement.Infrastructure.Persistence;

[Collection(FieldManagementDatabaseCollection.Name)]
public class CropRepositoryTests(FieldManagementDatabaseFixture fixture) : PersistenceTestBase(fixture)
{
    [MySqlFact]
    public async Task ExistsWithNameAndVarietyAsync_ShouldDetectDuplicatesAndRespectTheExcludedIdAsync()
    {
        // Arrange
        var crop = await AddCropAsync("Maiz", "Amarillo");
        var repository = new CropRepository(Context);

        // Act & Assert
        (await repository.ExistsWithNameAndVarietyAsync("Maiz", "Amarillo")).Should().BeTrue();
        (await repository.ExistsWithNameAndVarietyAsync("Maiz", "Amarillo", crop.Id.Value)).Should().BeFalse();
        (await repository.ExistsWithNameAndVarietyAsync("Maiz", "Blanco")).Should().BeFalse();
    }

    [MySqlFact]
    public async Task AddAsync_ShouldRejectDuplicatedNameAndVarietyAsync()
    {
        // Arrange
        await AddCropAsync("Cafe", "Castillo");

        // Act
        var repository = new CropRepository(Context);
        await repository.AddAsync(new Crop("Cafe", "Castillo"));
        var action = async () => await Context.SaveChangesAsync();

        // Assert
        await action.Should().ThrowAsync<DbUpdateException>();
    }
}
