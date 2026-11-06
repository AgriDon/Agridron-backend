using Agridron.Backend.API.FlightOperations.Domain.Model.Aggregates;
using Agridron.Backend.API.FlightOperations.Domain.Model.Errors;
using Agridron.Backend.API.FlightOperations.Domain.Model.Events;
using Agridron.Backend.API.FlightOperations.Domain.Model.ValueObjects;
using Agridron.Backend.API.Shared.Domain.Model.Exceptions;
using FluentAssertions;

namespace Agridron.Backend.API.Tests.FlightOperations.Domain.Model;

public sealed class DroneTests
{
    [Fact]
    public void Constructor_ShouldInitializeAvailableDrone()
    {
        // Act
        var drone = new Drone("AGRI-DRN-001", "DJI Agras T40", 40.0m, nozzleId: 2, imageUrl: "https://example.com/drone.jpg");

        // Assert
        drone.SerialNumber.Should().Be("AGRI-DRN-001");
        drone.Model.Should().Be("DJI Agras T40");
        drone.CapacityLiters.Should().Be(40.0m);
        drone.NozzleId.Should().Be(2);
        drone.ImageUrl.Should().Be("https://example.com/drone.jpg");
        drone.Status.Should().Be(DroneStatus.Available);
        drone.BatteryLevel.Should().Be(100.0m);
        drone.FlightHours.Should().Be(0.0m);
        drone.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldRejectMissingSerialNumber(string? serialNumber)
    {
        // Act
        var act = () => new Drone(serialNumber!, "DJI Agras T40", 40.0m);

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Error.Code.Should().Be(FlightOperationsErrors.DroneSerialNumberRequired.Code);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Constructor_ShouldRejectInvalidCapacity(decimal capacity)
    {
        // Act
        var act = () => new Drone("AGRI-001", "DJI Agras T40", capacity);

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Error.Code.Should().Be(FlightOperationsErrors.DroneCapacityInvalid.Code);
    }

    [Fact]
    public void AssignToMission_ShouldTransitionStatusToAssigned()
    {
        // Arrange
        var drone = new Drone("AGRI-001", "Model X", 30m);

        // Act
        drone.AssignToMission();

        // Assert
        drone.Status.Should().Be(DroneStatus.Assigned);
    }

    [Fact]
    public void AssignToMission_WhenNotAvailable_ShouldThrowDomainException()
    {
        // Arrange
        var drone = new Drone("AGRI-001", "Model X", 30m);
        drone.AssignToMission(); // Now Assigned

        // Act
        var act = () => drone.AssignToMission();

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Error.Code.Should().Be("DroneNotAvailable");
    }

    [Fact]
    public void FlightLifecycle_ShouldTransitionCorrectlyAndAccumulateHours()
    {
        // Arrange
        var drone = new Drone("AGRI-001", "Model X", 30m);
        drone.AssignToMission();

        // Act: in flight
        drone.MarkInFlight();
        drone.Status.Should().Be(DroneStatus.InFlight);

        // Act: paused
        drone.MarkPaused();
        drone.Status.Should().Be(DroneStatus.Paused);

        // Act: released
        drone.ReleaseFromMission(1.5m);
        drone.Status.Should().Be(DroneStatus.Available);
        drone.FlightHours.Should().Be(1.5m);
    }

    [Fact]
    public void SendToMaintenance_WhenInFlight_ShouldThrowConflict()
    {
        // Arrange
        var drone = new Drone("AGRI-001", "Model X", 30m);
        drone.AssignToMission();
        drone.MarkInFlight();

        // Act
        var act = () => drone.SendToMaintenance();

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Error.Code.Should().Be("DroneBusy");
    }

    [Fact]
    public void CompleteMaintenance_ShouldReturnToAvailableAndUpdateDates()
    {
        // Arrange
        var drone = new Drone("AGRI-001", "Model X", 30m);
        drone.SendToMaintenance();
        var date = DateTimeOffset.UtcNow;
        var nextDate = date.AddMonths(3);

        // Act
        drone.CompleteMaintenance(date, nextDate);

        // Assert
        drone.Status.Should().Be(DroneStatus.Available);
        drone.LastMaintenanceDate.Should().Be(date);
        drone.NextMaintenanceDate.Should().Be(nextDate);
    }

    [Fact]
    public void Deactivate_WhenAssigned_ShouldThrowConflict()
    {
        // Arrange
        var drone = new Drone("AGRI-001", "Model X", 30m);
        drone.AssignToMission();

        // Act
        var act = () => drone.Deactivate();

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Error.Code.Should().Be("DroneBusy");
    }
}
