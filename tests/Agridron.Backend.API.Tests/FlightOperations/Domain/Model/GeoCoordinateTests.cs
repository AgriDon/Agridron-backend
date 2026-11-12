using Agridron.Backend.API.FlightOperations.Domain.Model.Errors;
using Agridron.Backend.API.FlightOperations.Domain.Model.ValueObjects;
using Agridron.Backend.API.Shared.Domain.Model.Exceptions;
using FluentAssertions;

namespace Agridron.Backend.API.Tests.FlightOperations.Domain.Model;

public sealed class GeoCoordinateTests
{
    [Fact]
    public void Constructor_WithValidCoordinates_ShouldSucceed()
    {
        var coord = new GeoCoordinate(-14.07m, -76.14m);
        coord.Latitude.Should().Be(-14.07m);
        coord.Longitude.Should().Be(-76.14m);
    }

    [Theory]
    [InlineData(-95.0)]
    [InlineData(95.0)]
    public void Constructor_WithInvalidLatitude_ShouldThrow(decimal lat)
    {
        var act = () => new GeoCoordinate(lat, 0m);
        act.Should().Throw<DomainException>()
            .Which.Error.Code.Should().Be(FlightOperationsErrors.LatitudeOutOfRange.Code);
    }

    [Theory]
    [InlineData(-185.0)]
    [InlineData(185.0)]
    public void Constructor_WithInvalidLongitude_ShouldThrow(decimal lon)
    {
        var act = () => new GeoCoordinate(0m, lon);
        act.Should().Throw<DomainException>()
            .Which.Error.Code.Should().Be(FlightOperationsErrors.LongitudeOutOfRange.Code);
    }
}
