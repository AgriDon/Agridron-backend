using Agridron.Backend.API.Shared.Domain.Model.Entities;

namespace Agridron.Backend.API.Tests.Shared.Domain.Model;

public class EntityTests
{
    [Fact]
    public void Equals_ShouldReturnTrue_WhenSameTypeAndIdentity()
    {
        // Arrange
        var left = new SampleEntity(42);
        var right = new SampleEntity(42);

        // Act & Assert
        left.Equals(right).Should().BeTrue();
        (left == right).Should().BeTrue();
        left.GetHashCode().Should().Be(right.GetHashCode());
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenIdentityDiffers()
    {
        // Arrange
        var left = new SampleEntity(42);
        var right = new SampleEntity(43);

        // Act & Assert
        left.Equals(right).Should().BeFalse();
        (left != right).Should().BeTrue();
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenTypesDifferEvenWithSameIdentity()
    {
        // Arrange
        var entity = new SampleEntity(42);
        var other = new OtherEntity(42);

        // Act & Assert
        entity.Equals(other).Should().BeFalse();
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenOtherIsNull()
    {
        // Arrange
        var entity = new SampleEntity(42);

        // Act & Assert
        entity.Equals(null).Should().BeFalse();
        (entity == null).Should().BeFalse();
    }

    private sealed class SampleEntity(long id) : Entity<long>(id);

    private sealed class OtherEntity(long id) : Entity<long>(id);
}
