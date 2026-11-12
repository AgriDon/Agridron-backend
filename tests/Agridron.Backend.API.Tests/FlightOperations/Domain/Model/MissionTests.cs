using Agridron.Backend.API.FlightOperations.Domain.Model.Aggregates;
using Agridron.Backend.API.FlightOperations.Domain.Model.Errors;
using Agridron.Backend.API.FlightOperations.Domain.Model.Events;
using Agridron.Backend.API.FlightOperations.Domain.Model.ValueObjects;
using Agridron.Backend.API.Shared.Domain.Model.Exceptions;
using FluentAssertions;

namespace Agridron.Backend.API.Tests.FlightOperations.Domain.Model;

public sealed class MissionTests
{
    private static Mission CreateSampleMission() =>
        new(
            code: "MSN-0001",
            farmId: 1,
            parcelId: 10,
            farmArea: "Parcela Norte",
            cropType: "Maiz",
            scheduledAt: DateTimeOffset.UtcNow.AddDays(1),
            plannedAreaHectares: 12.5m);

    [Fact]
    public void Constructor_ShouldInitializePlannedMission()
    {
        // Act
        var mission = CreateSampleMission();

        // Assert
        mission.Code.Should().Be("MSN-0001");
        mission.FarmId.Should().Be(1);
        mission.ParcelId.Should().Be(10);
        mission.FarmArea.Should().Be("Parcela Norte");
        mission.CropType.Should().Be("Maiz");
        mission.PlannedAreaHectares.Should().Be(12.5m);
        mission.Status.Should().Be(MissionStatus.Planned);
        mission.Progress.Should().Be(0);
        mission.DroneId.Should().BeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_ShouldRejectInvalidPlannedArea(decimal area)
    {
        var act = () => new Mission("MSN-01", 1, 1, "Area", "Crop", DateTimeOffset.UtcNow, area);
        act.Should().Throw<DomainException>()
            .Which.Error.Code.Should().Be(FlightOperationsErrors.MissionPlannedAreaOutOfRange.Code);
    }

    [Fact]
    public void AssignResources_ShouldSetDroneAndInputs_MI002()
    {
        // Arrange
        var mission = CreateSampleMission();
        var droneId = new DroneId(5);

        // Act
        mission.AssignResources(droneId, "Fungicida", "Fungicida 48%", 2.0m, chemicalId: 3);

        // Assert
        mission.DroneId.Should().Be(droneId);
        mission.TreatmentType.Should().Be("Fungicida");
        mission.ProductName.Should().Be("Fungicida 48%");
        mission.ProductDose.Should().Be(2.0m);
        mission.ChemicalId.Should().Be(3);
    }

    [Fact]
    public void AssignOperator_ShouldSetOperator_MI003()
    {
        // Arrange
        var mission = CreateSampleMission();

        // Act
        mission.AssignOperator("Carlos Mendoza", operatorId: 7);

        // Assert
        mission.OperatorName.Should().Be("Carlos Mendoza");
        mission.OperatorId.Should().Be(7);
    }

    [Fact]
    public void ConfigureFlightParameters_ShouldSetParameters_MI004()
    {
        // Arrange
        var mission = CreateSampleMission();
        var parameters = new FlightParameters(3.5m, 5.0m, 20.0m, 25.0m);

        // Act
        mission.ConfigureFlightParameters(parameters);

        // Assert
        mission.FlightParameters.Should().Be(parameters);
        mission.FlightParameters!.AltitudeMeters.Should().Be(3.5m);
    }

    [Fact]
    public void Approve_WithoutDrone_ShouldThrowBusinessRule_MI007()
    {
        // Arrange
        var mission = CreateSampleMission();

        // Act
        var act = () => mission.Approve();

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Error.Code.Should().Be(FlightOperationsErrors.MissionDroneRequiredForApproval.Code);
    }

    [Fact]
    public void Approve_WithAssignedDrone_ShouldTransitionToAuthorized_MI007()
    {
        // Arrange
        var mission = CreateSampleMission();
        mission.AssignResources(new DroneId(1), "Herbicida", "Herbicida 70%", 1.5m);

        // Act
        mission.Approve();

        // Assert
        mission.Status.Should().Be(MissionStatus.Authorized);
    }

    [Fact]
    public void FullLifecycle_StartPauseResumeComplete_ShouldTrackProgressAndDates_TR004_TR005()
    {
        // Arrange
        var mission = CreateSampleMission();
        mission.AssignResources(new DroneId(1), "Insecticida", "Insecticida 25%", 1.0m);
        mission.Approve();

        var startTime = DateTimeOffset.UtcNow;
        mission.Start(startTime);
        mission.Status.Should().Be(MissionStatus.InProgress);
        mission.StartedAt.Should().Be(startTime);

        // Pause with coordinates
        var pauseCoords = new GeoCoordinate(-14.07m, -76.14m);
        mission.Pause(pauseCoords);
        mission.Status.Should().Be(MissionStatus.Paused);
        mission.LastPausePoint.Should().Be(pauseCoords);

        // Resume
        mission.Resume();
        mission.Status.Should().Be(MissionStatus.InProgress);

        // Complete
        var completeTime = startTime.AddMinutes(45);
        mission.Complete(completeTime, treatedAreaHectares: 12.5m, closingNotes: "Misión sin novedades");

        mission.Status.Should().Be(MissionStatus.Completed);
        mission.Progress.Should().Be(100);
        mission.TreatedAreaHectares.Should().Be(12.5m);
        mission.CompletedAt.Should().Be(completeTime);
        mission.Notes.Should().Contain("Misión sin novedades");
    }

    [Fact]
    public void Cancel_WhenCompleted_ShouldThrowBusinessRule()
    {
        // Arrange
        var mission = CreateSampleMission();
        mission.AssignResources(new DroneId(1), null, null, null);
        mission.Approve();
        mission.Start(DateTimeOffset.UtcNow);
        mission.Complete(DateTimeOffset.UtcNow.AddMinutes(30));

        // Act
        var act = () => mission.Cancel("Clima adverso");

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Error.Code.Should().Be("MissionCannotBeCancelled");
    }
}
