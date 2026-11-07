using Agridron.Backend.API.FlightOperations.Application.Internal.CommandServices;
using Agridron.Backend.API.FlightOperations.Domain.Model.Aggregates;
using Agridron.Backend.API.FlightOperations.Domain.Model.Commands;
using Agridron.Backend.API.FlightOperations.Domain.Model.ValueObjects;
using Agridron.Backend.API.Tests.FlightOperations.Application.Fakes;

namespace Agridron.Backend.API.Tests.FlightOperations.Application;

public class OperationStatusCommandServiceTests
{
    private readonly FakeOperationStatusRepository _statusRepository = new();
    private readonly FakeMissionRepository _missionRepository = new();
    private readonly FakeDroneRepository _droneRepository = new();
    private readonly FakeIncidentRepository _incidentRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly OperationStatusCommandService _service;

    public OperationStatusCommandServiceTests()
    {
        _service = new OperationStatusCommandService(
            _statusRepository,
            _missionRepository,
            _droneRepository,
            _incidentRepository,
            _unitOfWork);
    }

    [Fact]
    public async Task Handle_CreateOperationStatusCommand_WhenValid_ShouldPersistTelemetryAndBattery_TR002()
    {
        // Arrange
        var drone = new Drone("SN-STATUS-01", "DJI T30", 30m);
        await _droneRepository.AddAsync(drone);

        var mission = new Mission("MS-STAT-01", 1, 1, "Area", "Crop", DateTimeOffset.UtcNow, 5m);
        mission.AssignResources(drone.Id, "Tratamiento", "Producto", 2m);
        await _missionRepository.AddAsync(mission);

        var command = new CreateOperationStatusCommand(
            -12.046374m,
            -77.042793m,
            "EN_CURSO",
            DateTimeOffset.UtcNow,
            mission.Id.Value,
            drone.Id.Value,
            4.5m,
            6.0m,
            88.5m,
            25.0m);

        // Act
        var result = await _service.Handle(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Location.Latitude.Should().Be(-12.046374m);
        result.Value.Location.Longitude.Should().Be(-77.042793m);
        result.Value.BatteryLevel.Should().Be(88.5m);
        drone.BatteryLevel.Should().Be(88.5m);
    }

    [Fact]
    public async Task Handle_CreateOperationStatusCommand_WhenBatteryLow_ShouldCreateAnomalyIncident_TR003()
    {
        // Arrange
        var drone = new Drone("SN-BAT-01", "DJI T30", 30m);
        await _droneRepository.AddAsync(drone);

        var mission = new Mission("MS-BAT-01", 1, 1, "Area", "Crop", DateTimeOffset.UtcNow, 5m);
        mission.AssignResources(drone.Id, "Tratamiento", "Producto", 2m);
        await _missionRepository.AddAsync(mission);

        var command = new CreateOperationStatusCommand(
            -12.046374m,
            -77.042793m,
            "ADVERTENCIA",
            DateTimeOffset.UtcNow,
            mission.Id.Value,
            drone.Id.Value,
            4.0m,
            5.0m,
            9.0m, // Below 10% -> Critical
            5.0m);

        // Act
        var result = await _service.Handle(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var incidents = await _incidentRepository.ListFilteredAsync(
            new MissionId(mission.Id.Value),
            new DroneId(drone.Id.Value),
            null,
            null);

        incidents.Should().NotBeEmpty();
        incidents.Should().Contain(i => i.Type == "Batería Crítica" && i.Severity == IncidentSeverity.Critical);
    }

    [Fact]
    public async Task Handle_CreateOperationStatusCommand_WhenAltitudeDeviates_ShouldCreateAnomalyIncident_TR003()
    {
        // Arrange
        var drone = new Drone("SN-ALT-01", "DJI T30", 30m);
        await _droneRepository.AddAsync(drone);

        var mission = new Mission("MS-ALT-01", 1, 1, "Area", "Crop", DateTimeOffset.UtcNow, 5m);
        mission.AssignResources(drone.Id, "Tratamiento", "Producto", 2m);
        mission.ConfigureFlightParameters(FlightParameters.Create(3.0m, 5.0m, 15.0m, 20.0m));
        await _missionRepository.AddAsync(mission);

        var command = new CreateOperationStatusCommand(
            -12.046374m,
            -77.042793m,
            "EN_CURSO",
            DateTimeOffset.UtcNow,
            mission.Id.Value,
            drone.Id.Value,
            25.0m, // Deviation is 25 - 3 = 22m > 15m threshold!
            5.0m,
            80.0m,
            20.0m);

        // Act
        var result = await _service.Handle(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var incidents = await _incidentRepository.ListFilteredAsync(
            new MissionId(mission.Id.Value),
            new DroneId(drone.Id.Value),
            null,
            null);

        incidents.Should().NotBeEmpty();
        incidents.Should().Contain(i => i.Type == "Desviación de Altitud" && i.Severity == IncidentSeverity.Medium);
    }
}
