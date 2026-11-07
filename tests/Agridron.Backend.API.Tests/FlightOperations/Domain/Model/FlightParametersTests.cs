using Agridron.Backend.API.FlightOperations.Domain.Model.Errors;
using Agridron.Backend.API.FlightOperations.Domain.Model.ValueObjects;
using Agridron.Backend.API.Shared.Domain.Model.Exceptions;
using FluentAssertions;

namespace Agridron.Backend.API.Tests.FlightOperations.Domain.Model;

public sealed class FlightParametersTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldSucceed()
    {
        var fp = new FlightParameters(3.0m, 5.0m, 20.0m, 30.0m);
        fp.AltitudeMeters.Should().Be(3.0m);
        fp.SpeedMetersPerSecond.Should().Be(5.0m);
        fp.SprayRateLitersPerHectare.Should().Be(20.0m);
        fp.OverlapPercentage.Should().Be(30.0m);
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(35.0)]
    public void Constructor_WithAltitudeOutOfRange_ShouldThrow(decimal altitude)
    {
        var act = () => new FlightParameters(altitude, 5.0m, 20.0m, 30.0m);
        act.Should().Throw<DomainException>()
            .Which.Error.Code.Should().Be(FlightOperationsErrors.FlightAltitudeOutOfRange.Code);
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(25.0)]
    public void Constructor_WithSpeedOutOfRange_ShouldThrow(decimal speed)
    {
        var act = () => new FlightParameters(3.0m, speed, 20.0m, 30.0m);
        act.Should().Throw<DomainException>()
            .Which.Error.Code.Should().Be(FlightOperationsErrors.FlightSpeedOutOfRange.Code);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(150.0)]
    public void Constructor_WithSprayRateOutOfRange_ShouldThrow(decimal rate)
    {
        var act = () => new FlightParameters(3.0m, 5.0m, rate, 30.0m);
        act.Should().Throw<DomainException>()
            .Which.Error.Code.Should().Be(FlightOperationsErrors.SprayRateOutOfRange.Code);
    }

    [Theory]
    [InlineData(-5.0)]
    [InlineData(90.0)]
    public void Constructor_WithOverlapOutOfRange_ShouldThrow(decimal overlap)
    {
        var act = () => new FlightParameters(3.0m, 5.0m, 20.0m, overlap);
        act.Should().Throw<DomainException>()
            .Which.Error.Code.Should().Be(FlightOperationsErrors.OverlapPercentageOutOfRange.Code);
    }
}
