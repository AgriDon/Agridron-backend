using Agridron.Backend.API.FlightOperations.Domain.Model.Aggregates;
using Agridron.Backend.API.FlightOperations.Domain.Model.ValueObjects;
using Agridron.Backend.API.FlightOperations.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Agridron.Backend.API.Tests.FieldManagement.Infrastructure.Persistence;

namespace Agridron.Backend.API.Tests.FlightOperations.Infrastructure.Persistence;

[Collection(FlightOperationsDatabaseCollection.Name)]
public class IncidentRepositoryTests(FlightOperationsDatabaseFixture fixture) : FlightOperationsPersistenceTestBase(fixture)
{
    private IncidentRepository Repository => new(Context);
    private MissionRepository Missions => new(Context);
    private DroneRepository Drones => new(Context);

    [MySqlFact]
    public async Task AddAsync_ShouldPersistIncidentAndFilterBySeverityAsync()
    {
        // Arrange
        var drone = new Drone("SN-INC-DR-01", "DJI T30", 30m);
        await Drones.AddAsync(drone);

        var mission = new Mission("MS-INC-01", 1, 1, "Area", "Crop", DateTimeOffset.UtcNow, 5m);
        await Missions.AddAsync(mission);
        await Context.SaveChangesAsync();

        var incidentLow = new Incident("Obstáculo Leve", "Ramas de árbol detectadas", DateTimeOffset.UtcNow, mission.Id, drone.Id, IncidentSeverity.Low);
        var incidentCrit = new Incident("Batería Crítica", "Batería bajó al 8%", DateTimeOffset.UtcNow, mission.Id, drone.Id, IncidentSeverity.Critical);

        await Repository.AddAsync(incidentLow);
        await Repository.AddAsync(incidentCrit);
        await Context.SaveChangesAsync();

        // Act
        var criticalIncidents = await Repository.ListFilteredAsync(mission.Id, null, null, IncidentSeverity.Critical);

        // Assert
        criticalIncidents.Should().HaveCount(1);
        criticalIncidents[0].Type.Should().Be("Batería Crítica");
        criticalIncidents[0].Severity.Should().Be(IncidentSeverity.Critical);
    }
}
