using Agridron.Backend.API.FieldManagement.Interfaces.ACL;
using Agridron.Backend.API.FlightOperations.Application.Internal.CommandServices;
using Agridron.Backend.API.FlightOperations.Domain.Model.Aggregates;
using Agridron.Backend.API.FlightOperations.Domain.Model.Commands;
using Agridron.Backend.API.FlightOperations.Domain.Model.ValueObjects;
using Agridron.Backend.API.Tests.FlightOperations.Application.Fakes;

namespace Agridron.Backend.API.Tests.FlightOperations.Application;

public class MissionCommandServiceTests
{
    private readonly FakeMissionRepository _missionRepository = new();
    private readonly FakeDroneRepository _droneRepository = new();
    private readonly FakeExternalFieldService _fieldService = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly MissionCommandService _service;

    public MissionCommandServiceTests()
    {
        _service = new MissionCommandService(_missionRepository, _droneRepository, _fieldService, _unitOfWork);
    }

    [Fact]
    public async Task Handle_CreateMissionCommand_WithParcelSnapshot_ShouldEnrichAndCreateMission()
    {
        // Arrange
        _fieldService.ActiveFarms.Add(10);
        _fieldService.Parcels.Add(20, new ParcelSnapshotDto(20, 10, 5, "Lote Norte", "Maíz", 12.5m, true));

        var command = new CreateMissionCommand(
            "MS-2026-001",
            10,
            20,
            null, // should be enriched to "Lote Norte"
            null, // should be enriched to "Maíz"
            DateTimeOffset.UtcNow.AddDays(1),
            null, // should be enriched to 12.5m
            "Carlos Piloto",
            "Fumigación Foliar",
            "Fungicida X",
            2.5m,
            "Misión programada de prueba",
            null,
            0);

        // Act
        var result = await _service.Handle(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Code.Should().Be("MS-2026-001");
        result.Value.FarmArea.Should().Be("Lote Norte");
        result.Value.CropType.Should().Be("Maíz");
        result.Value.PlannedAreaHectares.Should().Be(12.5m);
        result.Value.Status.Should().Be(MissionStatus.Planned);
    }

    [Fact]
    public async Task Handle_AssignMissionResourcesCommand_ShouldAssignDroneAndInputs_MI002()
    {
        // Arrange
        var mission = new Mission("MS-RES-01", 1, 1, "Area", "Crop", DateTimeOffset.UtcNow, 10m);
        await _missionRepository.AddAsync(mission);

        var drone = new Drone("SN-RES-DR", "DJI T30", 30m);
        await _droneRepository.AddAsync(drone);

        var command = new AssignMissionResourcesCommand(
            mission.Id.Value,
            drone.Id.Value,
            "Herbicida Total",
            "Glifosato Pro",
            3.0m,
            101);

        // Act
        var result = await _service.Handle(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.DroneId.Should().Be(drone.Id);
        result.Value.TreatmentType.Should().Be("Herbicida Total");
        result.Value.ProductName.Should().Be("Glifosato Pro");
        result.Value.ProductDose.Should().Be(3.0m);
        drone.Status.Should().Be(DroneStatus.Assigned);
    }

    [Fact]
    public async Task Handle_AssignMissionOperatorCommand_ShouldAssignOperator_MI003()
    {
        // Arrange
        var mission = new Mission("MS-OP-01", 1, 1, "Area", "Crop", DateTimeOffset.UtcNow, 5m);
        await _missionRepository.AddAsync(mission);

        var command = new AssignMissionOperatorCommand(mission.Id.Value, "Ing. Roberto Gomez", 42);

        // Act
        var result = await _service.Handle(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.OperatorName.Should().Be("Ing. Roberto Gomez");
        result.Value.OperatorId.Should().Be(42);
    }

    [Fact]
    public async Task Handle_ConfigureFlightParametersCommand_ShouldSetParameters_MI004()
    {
        // Arrange
        var mission = new Mission("MS-PARAM-01", 1, 1, "Area", "Crop", DateTimeOffset.UtcNow, 5m);
        await _missionRepository.AddAsync(mission);

        var command = new ConfigureFlightParametersCommand(
            mission.Id.Value,
            3.5m,
            5.0m,
            15.0m,
            20.0m);

        // Act
        var result = await _service.Handle(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.FlightParameters.Should().NotBeNull();
        result.Value.FlightParameters!.AltitudeMeters.Should().Be(3.5m);
        result.Value.FlightParameters.SpeedMetersPerSecond.Should().Be(5.0m);
        result.Value.FlightParameters.SprayRateLitersPerHectare.Should().Be(15.0m);
        result.Value.FlightParameters.OverlapPercentage.Should().Be(20.0m);
    }

    [Fact]
    public async Task Handle_ApproveMissionCommand_WhenDroneAssigned_ShouldAuthorize_MI007()
    {
        // Arrange
        var drone = new Drone("SN-APP-01", "DJI T40", 40m);
        await _droneRepository.AddAsync(drone);

        var mission = new Mission("MS-APP-01", 1, 1, "Area", "Crop", DateTimeOffset.UtcNow, 5m);
        mission.AssignResources(drone.Id, "Fertilización", "Urea Líquida", 10m);
        await _missionRepository.AddAsync(mission);

        var command = new ApproveMissionCommand(mission.Id.Value);

        // Act
        var result = await _service.Handle(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Status.Should().Be(MissionStatus.Authorized);
    }

    [Fact]
    public async Task Handle_StartMissionCommand_ShouldSetInProgressAndDroneInFlight_TR001()
    {
        // Arrange
        var drone = new Drone("SN-START-01", "DJI T40", 40m);
        await _droneRepository.AddAsync(drone);

        var mission = new Mission("MS-START-01", 1, 1, "Area", "Crop", DateTimeOffset.UtcNow, 5m);
        mission.AssignResources(drone.Id, "Fertilización", "Urea Líquida", 10m);
        mission.Approve();
        await _missionRepository.AddAsync(mission);

        var command = new StartMissionCommand(mission.Id.Value);

        // Act
        var result = await _service.Handle(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Status.Should().Be(MissionStatus.InProgress);
        drone.Status.Should().Be(DroneStatus.InFlight);
    }

    [Fact]
    public async Task Handle_PauseAndResumeMissionCommand_ShouldTransitionCorrectly_TR004()
    {
        // Arrange
        var drone = new Drone("SN-PAUSE-01", "DJI T40", 40m);
        await _droneRepository.AddAsync(drone);

        var mission = new Mission("MS-PAUSE-01", 1, 1, "Area", "Crop", DateTimeOffset.UtcNow, 5m);
        mission.AssignResources(drone.Id, "Fertilización", "Urea Líquida", 10m);
        mission.Start(DateTimeOffset.UtcNow);
        await _missionRepository.AddAsync(mission);

        // Act Pause
        var pauseResult = await _service.Handle(new PauseMissionCommand(mission.Id.Value, -12.046374m, -77.042793m));

        // Assert Pause
        pauseResult.IsSuccess.Should().BeTrue();
        pauseResult.Value!.Status.Should().Be(MissionStatus.Paused);
        pauseResult.Value.LastPausePoint.Should().NotBeNull();
        drone.Status.Should().Be(DroneStatus.Paused);

        // Act Resume
        var resumeResult = await _service.Handle(new ResumeMissionCommand(mission.Id.Value));

        // Assert Resume
        resumeResult.IsSuccess.Should().BeTrue();
        resumeResult.Value!.Status.Should().Be(MissionStatus.InProgress);
        drone.Status.Should().Be(DroneStatus.InFlight);
    }

    [Fact]
    public async Task Handle_CompleteMissionCommand_ShouldCompleteAndReleaseDrone_TR005()
    {
        // Arrange
        var drone = new Drone("SN-COMP-01", "DJI T40", 40m);
        await _droneRepository.AddAsync(drone);

        var mission = new Mission("MS-COMP-01", 1, 1, "Area", "Crop", DateTimeOffset.UtcNow, 10m);
        mission.AssignResources(drone.Id, "Tratamiento", "Producto", 2m);
        mission.Start(DateTimeOffset.UtcNow.AddHours(-1));
        await _missionRepository.AddAsync(mission);

        var command = new CompleteMissionCommand(
            mission.Id.Value,
            DateTimeOffset.UtcNow,
            10.0m,
            "Misión completada con éxito");

        // Act
        var result = await _service.Handle(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Status.Should().Be(MissionStatus.Completed);
        result.Value.Progress.Should().Be(100);
        result.Value.TreatedAreaHectares.Should().Be(10.0m);
        drone.Status.Should().Be(DroneStatus.Available);
        drone.FlightHours.Should().BeGreaterThan(0m);
    }

    [Fact]
    public async Task Handle_CancelMissionCommand_ShouldCancelAndReleaseDrone_MI006()
    {
        // Arrange
        var drone = new Drone("SN-CANC-01", "DJI T40", 40m);
        await _droneRepository.AddAsync(drone);

        var mission = new Mission("MS-CANC-01", 1, 1, "Area", "Crop", DateTimeOffset.UtcNow, 10m);
        mission.AssignResources(drone.Id, "Tratamiento", "Producto", 2m);
        await _missionRepository.AddAsync(mission);

        var command = new CancelMissionCommand(mission.Id.Value, "Lluvia intensa inesperada");

        // Act
        var result = await _service.Handle(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Status.Should().Be(MissionStatus.Cancelled);
        drone.Status.Should().Be(DroneStatus.Available);
    }
}
