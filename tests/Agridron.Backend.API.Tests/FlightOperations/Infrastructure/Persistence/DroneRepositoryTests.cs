using Agridron.Backend.API.FlightOperations.Domain.Model.Aggregates;
using Agridron.Backend.API.FlightOperations.Domain.Model.ValueObjects;
using Agridron.Backend.API.FlightOperations.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Agridron.Backend.API.Tests.FieldManagement.Infrastructure.Persistence;

namespace Agridron.Backend.API.Tests.FlightOperations.Infrastructure.Persistence;

[Collection(FlightOperationsDatabaseCollection.Name)]
public class DroneRepositoryTests(FlightOperationsDatabaseFixture fixture) : FlightOperationsPersistenceTestBase(fixture)
{
    private DroneRepository Repository => new(Context);

    [MySqlFact]
    public async Task AddAsync_ShouldPersistDroneAndAssignIdentityAsync()
    {
        // Arrange
        var drone = new Drone(
            "SN-T40-TEST-01",
            "DJI Agras T40",
            40.0m,
            null,
            "https://cdn.agridron.com/drones/t40.jpg");

        // Act
        await Repository.AddAsync(drone);
        await Context.SaveChangesAsync();

        // Assert
        drone.Id.Value.Should().BeGreaterThan(0);

        var retrieved = await Repository.FindByIdAsync(drone.Id.Value);
        retrieved.Should().NotBeNull();
        retrieved!.SerialNumber.Should().Be("SN-T40-TEST-01");
        retrieved.Model.Should().Be("DJI Agras T40");
        retrieved.CapacityLiters.Should().Be(40.0m);
        retrieved.Status.Should().Be(DroneStatus.Available);
        retrieved.IsActive.Should().BeTrue();
    }

    [MySqlFact]
    public async Task FindBySerialNumberAsync_ShouldFindExistingDroneAsync()
    {
        // Arrange
        var drone = new Drone("SN-SEARCH-01", "DJI T30", 30m);
        await Repository.AddAsync(drone);
        await Context.SaveChangesAsync();

        // Act
        var found = await Repository.FindBySerialNumberAsync("SN-SEARCH-01");

        // Assert
        found.Should().NotBeNull();
        found!.Id.Value.Should().Be(drone.Id.Value);
    }

    [MySqlFact]
    public async Task ExistsWithSerialNumberAsync_ShouldDetectDuplicatesAndRespectExcludedIdAsync()
    {
        // Arrange
        var drone = new Drone("SN-DUP-01", "DJI T20", 20m);
        await Repository.AddAsync(drone);
        await Context.SaveChangesAsync();

        // Act & Assert
        var exists = await Repository.ExistsWithSerialNumberAsync("SN-DUP-01");
        exists.Should().BeTrue();

        var existsExcludingSelf = await Repository.ExistsWithSerialNumberAsync("SN-DUP-01", drone.Id);
        existsExcludingSelf.Should().BeFalse();
    }

    [MySqlFact]
    public async Task ListFilteredAsync_ShouldFilterByStatusAndCapacityAsync()
    {
        // Arrange
        var drone1 = new Drone("SN-F-01", "DJI T10", 10m);
        var drone2 = new Drone("SN-F-02", "DJI T30", 30m);
        var drone3 = new Drone("SN-F-03", "DJI T40", 40m);
        drone3.AssignToMission();

        await Repository.AddAsync(drone1);
        await Repository.AddAsync(drone2);
        await Repository.AddAsync(drone3);
        await Context.SaveChangesAsync();

        // Act - filter available with capacity >= 20
        var results = await Repository.ListFilteredAsync(DroneStatus.Available, 20m, null);

        // Assert
        results.Should().HaveCount(1);
        results[0].SerialNumber.Should().Be("SN-F-02");
    }
}
