using Agridron.Backend.API.FieldManagement.Domain.Model.Services;
using Agridron.Backend.API.FieldManagement.Domain.Model.ValueObjects;

namespace Agridron.Backend.API.Tests.FieldManagement.Domain.Model;

public class GeodesicAreaCalculatorTests
{
    [Fact]
    public void CalculateHectares_ShouldReturnTheAreaOfASmallSquare()
    {
        // Arrange: 0.01 degrees near the equator is roughly 123.6 hectares.
        var polygon = TestGeometry.Square(0, 0, 0.01, 0.01);

        // Act
        var area = GeodesicAreaCalculator.CalculateHectares(polygon);

        // Assert
        area.Should().BeApproximately(123.64m, 0.5m);
    }

    [Fact]
    public void CalculateHectares_ShouldReturnTheAreaOfAParcelInIca()
    {
        // Arrange: 0.01 degrees at latitude -14 is roughly 120 hectares.
        var polygon = TestGeometry.Square(-76.14, -14.07, -76.13, -14.06);

        // Act
        var area = GeodesicAreaCalculator.CalculateHectares(polygon);

        // Assert
        area.Should().BeApproximately(119.94m, 0.5m);
    }

    [Fact]
    public void CalculateHectares_ShouldRoundToTwoDecimals()
    {
        // Arrange
        var polygon = TestGeometry.Square(-76.14, -14.07, -76.14 + 0.001, -14.07 + 0.001);

        // Act
        var area = GeodesicAreaCalculator.CalculateHectares(polygon);

        // Assert
        area.Should().BeApproximately(1.20m, 0.01m);
        decimal.Round(area, 2).Should().Be(area);
    }

    [Fact]
    public void CalculateHectares_ShouldSubtractHoles()
    {
        // Arrange: a 0.01 square with a centered 0.005 hole keeps 75% of the area.
        const string json = """
                            {
                              "type": "Polygon",
                              "coordinates": [
                                [[0,0],[0.01,0],[0.01,0.01],[0,0.01],[0,0]],
                                [[0.0025,0.0025],[0.0075,0.0025],[0.0075,0.0075],[0.0025,0.0075],[0.0025,0.0025]]
                              ]
                            }
                            """;

        var polygon = GeoJsonPolygon.FromGeoJson(json);

        // Act
        var area = GeodesicAreaCalculator.CalculateHectares(polygon);

        // Assert
        area.Should().BeApproximately(92.73m, 0.5m);
    }

    [Fact]
    public void CalculateSquareMeters_ShouldReturnZero_WhenTheRingHasNoArea()
    {
        // Arrange: four collinear points are closed but enclose nothing.
        const string json = """{"type":"Polygon","coordinates":[[[0,0],[1,0],[2,0],[0,0]]]}""";
        var polygon = GeoJsonPolygon.FromGeoJson(json);

        // Act
        var area = GeodesicAreaCalculator.CalculateSquareMeters(polygon);

        // Assert
        area.Should().Be(0d);
    }

    [Fact]
    public void CalculateHectares_ShouldNotMutateThePolygon()
    {
        // Arrange
        var polygon = TestGeometry.Square(-76.14, -14.07, -76.13, -14.06);
        var original = polygon.GeoJson;

        // Act
        GeodesicAreaCalculator.CalculateHectares(polygon);

        // Assert
        polygon.GeoJson.Should().Be(original);
    }
}
