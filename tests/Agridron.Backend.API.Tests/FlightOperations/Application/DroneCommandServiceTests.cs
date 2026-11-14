using Agridron.Backend.API.FlightOperations.Application.Internal.CommandServices;
using Agridron.Backend.API.FlightOperations.Domain.Model.Aggregates;
using Agridron.Backend.API.FlightOperations.Domain.Model.Commands;
using Agridron.Backend.API.FlightOperations.Domain.Model.ValueObjects;
using Agridron.Backend.API.Tests.FlightOperations.Application.Fakes;

namespace Agridron.Backend.API.Tests.FlightOperations.Application;

public class DroneCommandServiceTests
{
    private readonly FakeDroneRepository _droneRepository = new();
    private readonly FakeIncidentRepository _incidentRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly DroneCommandService _service;

    public DroneCommandServiceTests()
    {
        _service = new DroneCommandService(_droneRepository, _incidentRepository,_unitOfWork);
    }

    [Fact]
    public async Task Handle_CreateDroneCommand_WhenSerialNumberUnique_ShouldCreateDrone()
    {
        // Arrange
        var command = new CreateDroneCommand(
            "SN-DJI-T40-999",
            "DJI Agras T40",
            40.0m,
            null,
            "https://cdn.agridron.com/drones/t40.jpg");

        // Act
        var result = await _service.Handle(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.SerialNumber.Should().Be("SN-DJI-T40-999");
        result.Value.Model.Should().Be("DJI Agras T40");
        result.Value.CapacityLiters.Should().Be(40.0m);
        result.Value.Status.Should().Be(DroneStatus.Available);
        _unitOfWork.CommitsCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_CreateDroneCommand_WhenSerialNumberDuplicate_ShouldReturnConflict()
    {
        // Arrange
        var existing = new Drone("SN-DUP-01", "Model X", 20m);
        await _droneRepository.AddAsync(existing);

        var command = new CreateDroneCommand("SN-DUP-01", "Model Y", 30m);

        // Act
        var result = await _service.Handle(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("DroneSerialNumberAlreadyExists");
    }

    [Fact]
    public async Task Handle_UpdateDroneCommand_WhenDroneExists_ShouldUpdateProperties()
    {
        // Arrange
        var drone = new Drone("SN-UPD-01", "Model Alpha", 15m);
        await _droneRepository.AddAsync(drone);

        var command = new UpdateDroneCommand(
            drone.Id.Value,
            "Model Alpha Pro",
            25m,
            null,
            "https://cdn.agridron.com/drones/alpha-pro.png");

        // Act
        var result = await _service.Handle(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Model.Should().Be("Model Alpha Pro");
        result.Value.CapacityLiters.Should().Be(25m);
    }

    [Fact]
    public async Task Handle_DeleteDroneCommand_WhenDroneAvailable_ShouldDeactivateDrone()
    {
        // Arrange
        var drone = new Drone("SN-DEL-01", "Model Del", 10m);
        await _droneRepository.AddAsync(drone);

        var command = new DeleteDroneCommand(drone.Id.Value);

        // Act
        var result = await _service.Handle(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        drone.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_DeleteDroneCommand_WhenDroneInFlight_ShouldReturnConflict()
    {
        // Arrange
        var drone = new Drone("SN-FLY-01", "Model Flyer", 10m);
        await _droneRepository.AddAsync(drone);
        drone.AssignToMission();
        drone.MarkInFlight();

        var command = new DeleteDroneCommand(drone.Id.Value);

        // Act
        var result = await _service.Handle(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("DroneBusy");
        drone.IsActive.Should().BeTrue();
    }

        [Fact]
    public async Task Handle_SendDroneToMaintenanceCommand_WhenAvailable_ShouldSetMaintenance()
    {
        // Arrange
        var drone = new Drone("SN-MNT-01", "Model Alpha", 15m);
        await _droneRepository.AddAsync(drone);

        // Act
        var result = await _service.Handle(new SendDroneToMaintenanceCommand(drone.Id.Value));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Status.Should().Be(DroneStatus.Maintenance);
    }

    [Fact]
    public async Task Handle_CompleteDroneMaintenanceCommand_WhenInMaintenance_ShouldReturnToAvailable()
    {
        // Arrange
        var drone = new Drone("SN-MNT-02", "Model Alpha", 15m);
        await _droneRepository.AddAsync(drone);
        drone.SendToMaintenance();
        var date = new DateTimeOffset(2026, 11, 13, 0, 0, 0, TimeSpan.Zero);

        // Act
        var result = await _service.Handle(new CompleteDroneMaintenanceCommand(drone.Id.Value, date, date.AddMonths(3)));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Status.Should().Be(DroneStatus.Available);
        result.Value.LastMaintenanceDate.Should().Be(date);
        result.Value.NextMaintenanceDate.Should().Be(date.AddMonths(3));
    }

    [Fact]
    public async Task Handle_ReportDroneFailureCommand_ShouldSetOutOfServiceAndRecordIncident()
    {
        // Arrange
        var drone = new Drone("SN-FAIL-01", "Model Alpha", 15m);
        await _droneRepository.AddAsync(drone);

        // Act
        var result = await _service.Handle(new ReportDroneFailureCommand(drone.Id.Value, "La batería se descarga muy rápido"));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Status.Should().Be(DroneStatus.OutOfService);

        var incidents = await _incidentRepository.ListFilteredAsync(null, drone.Id, IncidentTypes.EquipmentFailure, null);
        incidents.Should().ContainSingle();
        incidents[0].Description.Should().Be("La batería se descarga muy rápido");
        incidents[0].Severity.Should().Be(IncidentSeverity.High);
    }

    [Fact]
    public async Task Handle_ReportDroneFailureCommand_WithoutDescription_ShouldFail()
    {
        // Arrange
        var drone = new Drone("SN-FAIL-02", "Model Alpha", 15m);
        await _droneRepository.AddAsync(drone);

        // Act
        var result = await _service.Handle(new ReportDroneFailureCommand(drone.Id.Value, "   "));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("DroneFailureDescriptionRequired");
        drone.Status.Should().Be(DroneStatus.Available);
    }

}
