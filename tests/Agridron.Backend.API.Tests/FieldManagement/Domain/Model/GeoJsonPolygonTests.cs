using Agridron.Backend.API.FieldManagement.Domain.Model.Errors;
using Agridron.Backend.API.FieldManagement.Domain.Model.ValueObjects;
using Agridron.Backend.API.Shared.Domain.Model.Exceptions;

namespace Agridron.Backend.API.Tests.FieldManagement.Domain.Model;

public class GeoJsonPolygonTests
{
    [Fact]
    public void FromGeoJson_ShouldNormalizeAndExposeTheExteriorRing()
    {
        // Act
        var polygon = TestGeometry.Square(-76.14, -14.07, -76.13, -14.06);

        // Assert
        polygon.GeoJson.Should().StartWith("{\"type\":\"Polygon\",\"coordinates\":[[[");
        polygon.Rings.Should().HaveCount(1);
        polygon.ExteriorRing.Should().HaveCount(5);
        polygon.ExteriorRing[0].Should().Be(new GeoPoint(-76.14, -14.07));
        polygon.ExteriorRing[^1].Should().Be(polygon.ExteriorRing[0]);
    }

    [Fact]
    public void FromGeoJson_ShouldBeEqual_WhenCoordinatesAreEquivalent()
    {
        // Arrange
        var left = TestGeometry.Square(-76.14, -14.07, -76.13, -14.06);
        var right = GeoJsonPolygon.FromGeoJson(TestGeometry.SquareGeoJson(-76.14, -14.07, -76.13, -14.06));

        // Act & Assert
        left.Should().Be(right);
        (left == right).Should().BeTrue();
        left.GetHashCode().Should().Be(right.GetHashCode());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void FromGeoJson_ShouldRejectMissingInput(string? geoJson)
    {
        // Act
        var action = () => GeoJsonPolygon.FromGeoJson(geoJson);

        // Assert
        action.Should().Throw<DomainException>()
            .Which.Error.Should().Be(FieldManagementErrors.PolygonGeoJsonRequired);
    }

    [Fact]
    public void FromGeoJson_ShouldRejectMalformedJson()
    {
        // Act
        var action = () => GeoJsonPolygon.FromGeoJson("{ not json");

        // Assert
        action.Should().Throw<DomainException>()
            .Which.Error.Code.Should().Be(FieldManagementErrors.PolygonGeoJsonInvalid(string.Empty).Code);
    }

    [Fact]
    public void FromGeoJson_ShouldRejectNonPolygonTypes()
    {
        // Act
        var action = () => GeoJsonPolygon.FromGeoJson("""{"type":"Point","coordinates":[0,0]}""");

        // Assert
        action.Should().Throw<DomainException>()
            .Which.Error.Should().Be(FieldManagementErrors.InvalidPolygonType);
    }

    [Fact]
    public void FromGeoJson_ShouldRejectMissingCoordinates()
    {
        // Act
        var action = () => GeoJsonPolygon.FromGeoJson("""{"type":"Polygon"}""");

        // Assert
        action.Should().Throw<DomainException>()
            .Which.Error.Should().Be(FieldManagementErrors.PolygonCoordinatesMissing);
    }

    [Fact]
    public void FromGeoJson_ShouldRejectRingsWithTooFewPositions()
    {
        // Act
        var action = () => GeoJsonPolygon.FromGeoJson(
            """{"type":"Polygon","coordinates":[[[0,0],[1,0],[0,0]]]}""");

        // Assert
        action.Should().Throw<DomainException>()
            .Which.Error.Should().Be(FieldManagementErrors.PolygonTooFewPoints);
    }

    [Fact]
    public void FromGeoJson_ShouldRejectOpenRings()
    {
        // Act
        var action = () => GeoJsonPolygon.FromGeoJson(
            """{"type":"Polygon","coordinates":[[[0,0],[1,0],[1,1],[0,1]]]}""");

        // Assert
        action.Should().Throw<DomainException>()
            .Which.Error.Should().Be(FieldManagementErrors.PolygonRingNotClosed);
    }

    [Theory]
    [InlineData(181, 0, "longitude")]
    [InlineData(-181, 0, "longitude")]
    [InlineData(0, 91, "latitude")]
    [InlineData(0, -91, "latitude")]
    public void FromGeoJson_ShouldRejectOutOfRangeCoordinates(double longitude, double latitude, string _)
    {
        // Arrange
        var json = $$"""
                     {"type":"Polygon","coordinates":[[[{{longitude}},{{latitude}}],[1,0],[1,1],[0,0]]]}
                     """;

        // Act
        var action = () => GeoJsonPolygon.FromGeoJson(json);

        // Assert
        action.Should().Throw<DomainException>()
            .Which.Error.Should().Be(FieldManagementErrors.CoordinateOutOfRange);
    }

    [Fact]
    public void FromGeoJson_ShouldRejectNonNumericCoordinates()
    {
        // Act
        var action = () => GeoJsonPolygon.FromGeoJson(
            """{"type":"Polygon","coordinates":[[["a","b"],[1,0],[1,1],[0,0]]]}""");

        // Assert
        action.Should().Throw<DomainException>()
            .Which.Error.Code.Should().Be("PolygonGeoJsonInvalid");
    }

    [Fact]
    public void FromGeoJson_ShouldKeepHoles()
    {
        // Arrange
        const string json = """
                            {
                              "type": "Polygon",
                              "coordinates": [
                                [[0,0],[0.01,0],[0.01,0.01],[0,0.01],[0,0]],
                                [[0.0025,0.0025],[0.0075,0.0025],[0.0075,0.0075],[0.0025,0.0075],[0.0025,0.0025]]
                              ]
                            }
                            """;

        // Act
        var polygon = GeoJsonPolygon.FromGeoJson(json);

        // Assert
        polygon.Rings.Should().HaveCount(2);
        polygon.ExteriorRing.Should().HaveCount(5);
    }
}
