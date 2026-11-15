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

public class DronesControllerTests
{
    private readonly FakeDroneRepository _droneRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly DronesController _controller;

    public DronesControllerTests()
    {
        var commandService = new DroneCommandService(_droneRepository, new FakeIncidentRepository(), _unitOfWork);
        var queryService = new DroneQueryService(_droneRepository);
        var problemDetailsFactory = new ProblemDetailsFactory(null!, null!, null!);



        _controller = new DronesController(commandService, queryService, problemDetailsFactory)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Fact]
    public async Task GetDronesAsync_ShouldReturnOkWithDronesList()
    {
        // Arrange
        await _droneRepository.AddAsync(new Drone("SN-01", "Model A", 20m));
        await _droneRepository.AddAsync(new Drone("SN-02", "Model B", 30m));

        // Act
        var result = await _controller.GetDronesAsync(null, null, null, null, null, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = okResult.Value.Should().BeAssignableTo<IEnumerable<DroneResource>>().Subject;
        items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetDroneByIdAsync_WhenFound_ShouldReturnOkWithResource()
    {
        // Arrange
        var drone = new Drone("SN-01", "Model A", 20m);
        await _droneRepository.AddAsync(drone);

        // Act
        var result = await _controller.GetDroneByIdAsync(drone.Id.Value, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var resource = okResult.Value.Should().BeOfType<DroneResource>().Subject;
        resource.SerialNumber.Should().Be("SN-01");
        resource.Model.Should().Be("Model A");
    }

    [Fact]
    public async Task CreateDroneAsync_WhenValid_ShouldReturnCreatedAtRoute()
    {
        // Arrange
        var resource = new CreateDroneResource("SN-NEW-99", "DJI T50", 50m, 50m, null, null, "https://cdn.example.com/img.png");

        // Act
        var result = await _controller.CreateDroneAsync(resource, CancellationToken.None);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtRouteResult>().Subject;
        var createdResource = createdResult.Value.Should().BeOfType<DroneResource>().Subject;
        createdResource.SerialNumber.Should().Be("SN-NEW-99");
        createdResource.CapacityLiters.Should().Be(50m);
    }

    [Fact]
    public async Task DeleteDroneAsync_WhenAvailable_ShouldReturnNoContent()
    {
        // Arrange
        var drone = new Drone("SN-DEL", "Model Del", 20m);
        await _droneRepository.AddAsync(drone);

        // Act
        var result = await _controller.DeleteDroneAsync(drone.Id.Value, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }
}
