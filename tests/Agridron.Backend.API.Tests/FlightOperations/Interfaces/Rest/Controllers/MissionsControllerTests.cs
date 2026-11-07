using Agridron.Backend.API.FieldManagement.Interfaces.ACL;
using Agridron.Backend.API.FlightOperations.Application.Internal.CommandServices;
using Agridron.Backend.API.FlightOperations.Application.Internal.QueryServices;
using Agridron.Backend.API.FlightOperations.Domain.Model.Aggregates;
using Agridron.Backend.API.FlightOperations.Interfaces.Rest.Controllers;
using Agridron.Backend.API.FlightOperations.Interfaces.Rest.Resources;
using Agridron.Backend.API.Shared.Interfaces.Rest.ProblemDetails;
using Agridron.Backend.API.Tests.FlightOperations.Application.Fakes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Agridron.Backend.API.Tests.FlightOperations.Interfaces.Rest.Controllers;

public class MissionsControllerTests
{
    private readonly FakeMissionRepository _missionRepository = new();
    private readonly FakeDroneRepository _droneRepository = new();
    private readonly FakeExternalFieldService _fieldService = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly MissionsController _controller;

    public MissionsControllerTests()
    {
        var commandService = new MissionCommandService(_missionRepository, _droneRepository, _fieldService, _unitOfWork);
        var queryService = new MissionQueryService(_missionRepository);
        var problemDetailsFactory = new ProblemDetailsFactory(null!, null!, null!);

        _controller = new MissionsController(commandService, queryService, problemDetailsFactory)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Fact]
    public async Task GetMissionsAsync_ShouldReturnOkWithMissionsList()
    {
        // Arrange
        await _missionRepository.AddAsync(new Mission("MS-01", 1, 1, "Area 1", "Maíz", DateTimeOffset.UtcNow, 5m));
        await _missionRepository.AddAsync(new Mission("MS-02", 1, 2, "Area 2", "Trigo", DateTimeOffset.UtcNow, 8m));

        // Act
        var result = await _controller.GetMissionsAsync(null, null, null, null, null, null, null, null, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = okResult.Value.Should().BeAssignableTo<IEnumerable<MissionResource>>().Subject;
        items.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateMissionAsync_WhenValid_ShouldReturnCreatedAtRoute()
    {
        // Arrange
        _fieldService.ActiveFarms.Add(1);
        _fieldService.Parcels.Add(2, new ParcelSnapshotDto(2, 1, 1, "Parcela A", "Cacao", 10m, true));

        var resource = new CreateMissionResource(
            "MS-NEW-01",
            1,
            2,
            null,
            null,
            DateTimeOffset.UtcNow.AddDays(1),
            10m,
            "Operador 1",
            "Fumigación",
            "Fungicida",
            2.0m,
            "Notas",
            null,
            0);

        // Act
        var result = await _controller.CreateMissionAsync(resource, CancellationToken.None);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtRouteResult>().Subject;
        var createdResource = createdResult.Value.Should().BeOfType<MissionResource>().Subject;
        createdResource.Code.Should().Be("MS-NEW-01");
        createdResource.FarmArea.Should().Be("Parcela A");
        createdResource.CropType.Should().Be("Cacao");
    }

    [Fact]
    public async Task AssignResourcesAsync_ShouldAssignResourcesAndReturnOk_MI002()
    {
        // Arrange
        var mission = new Mission("MS-RES-01", 1, 1, "Area", "Crop", DateTimeOffset.UtcNow, 5m);
        await _missionRepository.AddAsync(mission);

        var drone = new Drone("SN-DR-01", "DJI T30", 30m);
        await _droneRepository.AddAsync(drone);

        var resource = new AssignMissionResourcesResource(drone.Id.Value, "Herbicida", "Producto X", 3.5m, 10);

        // Act
        var result = await _controller.AssignResourcesAsync(mission.Id.Value, resource, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var missionResource = okResult.Value.Should().BeOfType<MissionResource>().Subject;
        missionResource.DroneId.Should().Be(drone.Id.Value);
        missionResource.TreatmentType.Should().Be("Herbicida");
        missionResource.ProductName.Should().Be("Producto X");
        missionResource.ProductDose.Should().Be(3.5m);
    }

    [Fact]
    public async Task ConfigureFlightParametersAsync_ShouldConfigureAndReturnOk_MI004()
    {
        // Arrange
        var mission = new Mission("MS-PARAM-01", 1, 1, "Area", "Crop", DateTimeOffset.UtcNow, 5m);
        await _missionRepository.AddAsync(mission);

        var resource = new ConfigureFlightParametersResource(4.0m, 6.0m, 20.0m, 25.0m);

        // Act
        var result = await _controller.ConfigureFlightParametersAsync(mission.Id.Value, resource, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var missionResource = okResult.Value.Should().BeOfType<MissionResource>().Subject;
        missionResource.FlightParameters.Should().NotBeNull();
        missionResource.FlightParameters!.AltitudeMeters.Should().Be(4.0m);
        missionResource.FlightParameters.SpeedMetersPerSecond.Should().Be(6.0m);
        missionResource.FlightParameters.SprayRateLitersPerHectare.Should().Be(20.0m);
        missionResource.FlightParameters.OverlapPercentage.Should().Be(25.0m);
    }

    [Fact]
    public async Task ApproveMissionAsync_ShouldApproveAndReturnOk_MI007()
    {
        // Arrange
        var drone = new Drone("SN-DR-APP", "DJI T40", 40m);
        await _droneRepository.AddAsync(drone);

        var mission = new Mission("MS-APP-01", 1, 1, "Area", "Crop", DateTimeOffset.UtcNow, 5m);
        mission.AssignResources(drone.Id, "Tratamiento", "Producto", 1m);
        await _missionRepository.AddAsync(mission);

        // Act
        var result = await _controller.ApproveMissionAsync(mission.Id.Value, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var missionResource = okResult.Value.Should().BeOfType<MissionResource>().Subject;
        missionResource.Status.Should().Be("AUTHORIZED");
    }

    [Fact]
    public async Task StartMissionAsync_ShouldStartAndReturnOk_TR001()
    {
        // Arrange
        var drone = new Drone("SN-DR-START", "DJI T40", 40m);
        await _droneRepository.AddAsync(drone);

        var mission = new Mission("MS-START-01", 1, 1, "Area", "Crop", DateTimeOffset.UtcNow, 5m);
        mission.AssignResources(drone.Id, "Tratamiento", "Producto", 1m);
        mission.Approve();
        await _missionRepository.AddAsync(mission);

        // Act
        var result = await _controller.StartMissionAsync(mission.Id.Value, null, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var missionResource = okResult.Value.Should().BeOfType<MissionResource>().Subject;
        missionResource.Status.Should().Be("IN_PROGRESS");
    }

    [Fact]
    public async Task CompleteMissionAsync_ShouldCompleteAndReturnOk_TR005()
    {
        // Arrange
        var drone = new Drone("SN-DR-COMP", "DJI T40", 40m);
        await _droneRepository.AddAsync(drone);

        var mission = new Mission("MS-COMP-01", 1, 1, "Area", "Crop", DateTimeOffset.UtcNow, 10m);
        mission.AssignResources(drone.Id, "Tratamiento", "Producto", 1m);
        mission.Start(DateTimeOffset.UtcNow.AddMinutes(-45));
        await _missionRepository.AddAsync(mission);

        var resource = new CompleteMissionResource(DateTimeOffset.UtcNow, 10.0m, "Misión finalizada sin inconvenientes");

        // Act
        var result = await _controller.CompleteMissionAsync(mission.Id.Value, resource, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var missionResource = okResult.Value.Should().BeOfType<MissionResource>().Subject;
        missionResource.Status.Should().Be("COMPLETED");
        missionResource.Progress.Should().Be(100);
        missionResource.TreatedAreaHectares.Should().Be(10.0m);
    }
}
