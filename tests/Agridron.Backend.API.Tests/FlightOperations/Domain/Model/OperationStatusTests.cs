using Agridron.Backend.API.FlightOperations.Domain.Model.Aggregates;
using Agridron.Backend.API.FlightOperations.Domain.Model.ValueObjects;
using FluentAssertions;

namespace Agridron.Backend.API.Tests.FlightOperations.Domain.Model;

public sealed class OperationStatusTests
{
    [Fact]
    public void Constructor_WithValidFields_ShouldSucceed_TR001_TR002()
    {
        var now = DateTimeOffset.UtcNow;
        var coords = new GeoCoordinate(-11.8654m, -77.0381m);
        var op = new OperationStatus(
            location: coords,
            status: "SPRAYING",
            timestamp: now,
            missionId: new MissionId(1),
            droneId: new DroneId(2),
            altitudeMeters: 3.5m,
            speedMetersPerSecond: 6.0m,
            batteryLevel: 75.0m,
            tankVolumeLiters: 15.0m);

        op.Location.Should().Be(coords);
        op.Status.Should().Be("SPRAYING");
        op.Timestamp.Should().Be(now);
        op.MissionId.Should().Be(new MissionId(1));
        op.DroneId.Should().Be(new DroneId(2));
        op.AltitudeMeters.Should().Be(3.5m);
        op.SpeedMetersPerSecond.Should().Be(6.0m);
        op.BatteryLevel.Should().Be(75.0m);
        op.TankVolumeLiters.Should().Be(15.0m);
    }

    [Fact]
    public void UpdateLocation_ShouldChangeCoordinatesAndStatus()
    {
        var op = new OperationStatus(new GeoCoordinate(-11.8654m, -77.0381m), "FLYING", DateTimeOffset.UtcNow);
        var newCoords = new GeoCoordinate(-11.8660m, -77.0390m);

        op.UpdateLocation(newCoords, "SPRAYING");

        op.Location.Should().Be(newCoords);
        op.Status.Should().Be("SPRAYING");
    }
}
