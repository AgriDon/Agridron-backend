using Agridron.Backend.API.Shared.Domain.Model.ValueObjects;

namespace Agridron.Backend.API.Tests.Shared.Domain.Model;

public class ValueObjectTests
{
    [Fact]
    public void Equals_ShouldReturnTrue_WhenEveryComponentMatches()
    {
        // Arrange
        var left = new Coordinates(-14.07, -76.14);
        var right = new Coordinates(-14.07, -76.14);

        // Act & Assert
        left.Equals(right).Should().BeTrue();
        (left == right).Should().BeTrue();
        left.GetHashCode().Should().Be(right.GetHashCode());
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenAnyComponentDiffers()
    {
        // Arrange
        var left = new Coordinates(-14.07, -76.14);
        var right = new Coordinates(-14.07, -76.15);

        // Act & Assert
        left.Equals(right).Should().BeFalse();
        (left != right).Should().BeTrue();
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenTypesDifferEvenWithSameComponents()
    {
        // Arrange
        var coordinates = new Coordinates(1, 2);
        var point = new Point(1, 2);

        // Act & Assert
        coordinates.Equals(point).Should().BeFalse();
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenOtherIsNull()
    {
        // Arrange
        var valueObject = new Coordinates(1, 2);

        // Act & Assert
        valueObject.Equals(null).Should().BeFalse();
    }

    private sealed class Coordinates(double latitude, double longitude) : ValueObject
    {
        public double Latitude { get; } = latitude;

        public double Longitude { get; } = longitude;

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Latitude;
            yield return Longitude;
        }
    }

    private sealed class Point(double latitude, double longitude) : ValueObject
    {
        public double Latitude { get; } = latitude;

        public double Longitude { get; } = longitude;

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Latitude;
            yield return Longitude;
        }
    }
}
