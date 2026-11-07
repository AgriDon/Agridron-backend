using Agridron.Backend.API.FlightOperations.Domain.Model.Aggregates;
using Agridron.Backend.API.FlightOperations.Domain.Model.ValueObjects;
using Agridron.Backend.API.FlightOperations.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Agridron.Backend.API.Tests.FieldManagement.Infrastructure.Persistence;

namespace Agridron.Backend.API.Tests.FlightOperations.Infrastructure.Persistence;

[Collection(FlightOperationsDatabaseCollection.Name)]
public class MissionRepositoryTests(FlightOperationsDatabaseFixture fixture) : FlightOperationsPersistenceTestBase(fixture)
{
    private MissionRepository Repository => new(Context);
    private DroneRepository Drones => new(Context);

    [MySqlFact]
    public async Task AddAsync_ShouldPersistMissionWithFlightParametersAndPausePointAsync()
    {
        // Arrange
        var drone = new Drone("SN-MS-DRONE-01", "DJI T40", 40m);
        await Drones.AddAsync(drone);
        await Context.SaveChangesAsync();

        var scheduledAt = DateTimeOffset.UtcNow.AddDays(2);
        var mission = new Mission(
            "MS-TEST-001",
            10,
            20,
            "Lote Norte",
            "Maíz",
            scheduledAt,
            15.5m,
            "Carlos Piloto",
            "Fumigación Foliar",
            "Fungicida Pro",
            2.5m,
            "Notas de misión",
            drone.Id,
            0);

        mission.ConfigureFlightParameters(FlightParameters.Create(3.5m, 5.0m, 15.0m, 20.0m));
        mission.Start(DateTimeOffset.UtcNow);
        mission.Pause(GeoCoordinate.Create(-12.046374m, -77.042793m));

        // Act
        await Repository.AddAsync(mission);
        await Context.SaveChangesAsync();

        // Assert
        mission.Id.Value.Should().BeGreaterThan(0);

        var retrieved = await Repository.FindByIdAsync(mission.Id.Value);
        retrieved.Should().NotBeNull();
        retrieved!.Code.Should().Be("MS-TEST-001");
        retrieved.FarmArea.Should().Be("Lote Norte");
        retrieved.CropType.Should().Be("Maíz");
        retrieved.Status.Should().Be(MissionStatus.Paused);
        retrieved.FlightParameters.Should().NotBeNull();
        retrieved.FlightParameters!.AltitudeMeters.Should().Be(3.5m);
        retrieved.FlightParameters.SpeedMetersPerSecond.Should().Be(5.0m);
        retrieved.FlightParameters.SprayRateLitersPerHectare.Should().Be(15.0m);
        retrieved.FlightParameters.OverlapPercentage.Should().Be(20.0m);
        retrieved.LastPausePoint.Should().NotBeNull();
        retrieved.LastPausePoint!.Latitude.Should().Be(-12.046374m);
        retrieved.LastPausePoint.Longitude.Should().Be(-77.042793m);
    }

    [MySqlFact]
    public async Task FindByCodeAsync_ShouldFindMissionByCodeAsync()
    {
        // Arrange
        var mission = new Mission("MS-FIND-01", 1, 1, "Area", "Crop", DateTimeOffset.UtcNow, 10m);
        await Repository.AddAsync(mission);
        await Context.SaveChangesAsync();

        // Act
        var found = await Repository.FindByCodeAsync("MS-FIND-01");

        // Assert
        found.Should().NotBeNull();
        found!.Id.Value.Should().Be(mission.Id.Value);
    }

    [MySqlFact]
    public async Task ListFilteredAsync_ShouldFilterByFarmAndStatusAsync()
    {
        // Arrange
        var mission1 = new Mission("MS-FILT-01", 10, 1, "Area 1", "Crop", DateTimeOffset.UtcNow, 5m);
        var mission2 = new Mission("MS-FILT-02", 10, 2, "Area 2", "Crop", DateTimeOffset.UtcNow, 8m);
        var mission3 = new Mission("MS-FILT-03", 20, 3, "Area 3", "Crop", DateTimeOffset.UtcNow, 12m);

        await Repository.AddAsync(mission1);
        await Repository.AddAsync(mission2);
        await Repository.AddAsync(mission3);
        await Context.SaveChangesAsync();

        // Act
        var results = await Repository.ListFilteredAsync(10, null, null, MissionStatus.Planned, null, null);

        // Assert
        results.Should().HaveCount(2);
        results.Select(m => m.Code).Should().Contain(["MS-FILT-01", "MS-FILT-02"]);
    }
}
