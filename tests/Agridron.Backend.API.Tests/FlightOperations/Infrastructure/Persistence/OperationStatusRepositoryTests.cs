using Agridron.Backend.API.FlightOperations.Domain.Model.Aggregates;
using Agridron.Backend.API.FlightOperations.Domain.Model.ValueObjects;
using Agridron.Backend.API.FlightOperations.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Agridron.Backend.API.Tests.FieldManagement.Infrastructure.Persistence;

namespace Agridron.Backend.API.Tests.FlightOperations.Infrastructure.Persistence;

[Collection(FlightOperationsDatabaseCollection.Name)]
public class OperationStatusRepositoryTests(FlightOperationsDatabaseFixture fixture) : FlightOperationsPersistenceTestBase(fixture)
{
    private OperationStatusRepository Repository => new(Context);
    private MissionRepository Missions => new(Context);
    private DroneRepository Drones => new(Context);

    [MySqlFact]
    public async Task AddAsync_ShouldPersistTelemetryPointAndGetLatestByMissionIdAsync()
    {
        // Arrange
        var drone = new Drone("SN-TEL-DR-01", "DJI T40", 40m);
        await Drones.AddAsync(drone);

        var mission = new Mission("MS-TEL-01", 1, 1, "Area", "Crop", DateTimeOffset.UtcNow, 5m);
        await Missions.AddAsync(mission);
        await Context.SaveChangesAsync();

        var point1 = new OperationStatus(
            GeoCoordinate.Create(-12.046374m, -77.042793m),
            "INICIO",
            DateTimeOffset.UtcNow.AddMinutes(-10),
            mission.Id,
            drone.Id,
            3.0m,
            4.0m,
            95m,
            38m);

        var point2 = new OperationStatus(
            GeoCoordinate.Create(-12.046500m, -77.042900m),
            "EN_CURSO",
            DateTimeOffset.UtcNow,
            mission.Id,
            drone.Id,
            3.5m,
            5.0m,
            85m,
            30m);

        await Repository.AddAsync(point1);
        await Repository.AddAsync(point2);
        await Context.SaveChangesAsync();

        // Act
        var latest = await Repository.GetLatestByMissionIdAsync(mission.Id);

        // Assert
        latest.Should().NotBeNull();
        latest!.Status.Should().Be("EN_CURSO");
        latest.AltitudeMeters.Should().Be(3.5m);
        latest.BatteryLevel.Should().Be(85m);
        latest.Location.Latitude.Should().Be(-12.046500m);
        latest.Location.Longitude.Should().Be(-77.042900m);
    }
}
