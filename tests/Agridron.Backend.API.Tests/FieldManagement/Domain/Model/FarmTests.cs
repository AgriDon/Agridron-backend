using Agridron.Backend.API.FieldManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.FieldManagement.Domain.Model.Errors;
using Agridron.Backend.API.FieldManagement.Domain.Model.Events;
using Agridron.Backend.API.FieldManagement.Domain.Model.ValueObjects;
using Agridron.Backend.API.Shared.Domain.Model.Exceptions;

namespace Agridron.Backend.API.Tests.FieldManagement.Domain.Model;

public class FarmTests
{
    [Fact]
    public void Constructor_ShouldTrimNameAndLocation()
    {
        // Act
        var farm = new Farm("  Finca San Rafael  ", "  Ica, Peru  ", 7);

        // Assert
        farm.Name.Should().Be("Finca San Rafael");
        farm.Location.Should().Be("Ica, Peru");
        farm.OwnerId.Should().Be(7);
        farm.Id.Value.Should().Be(0);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldRejectMissingName(string name)
    {
        // Act
        var action = () => new Farm(name, "Ica", 1);

        // Assert
        action.Should().Throw<DomainException>()
            .Which.Error.Should().Be(FieldManagementErrors.FarmNameRequired);
    }

    [Fact]
    public void Constructor_ShouldRejectNameThatIsTooLong()
    {
        // Act
        var action = () => new Farm(new string('a', Farm.MaxNameLength + 1), "Ica", 1);

        // Assert
        action.Should().Throw<DomainException>()
            .Which.Error.Should().Be(FieldManagementErrors.FarmNameTooLong);
    }

    [Fact]
    public void Constructor_ShouldRejectMissingLocation()
    {
        // Act
        var action = () => new Farm("Finca", " ", 1);

        // Assert
        action.Should().Throw<DomainException>()
            .Which.Error.Should().Be(FieldManagementErrors.FarmLocationRequired);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_ShouldRejectInvalidOwner(long ownerId)
    {
        // Act
        var action = () => new Farm("Finca", "Ica", ownerId);

        // Assert
        action.Should().Throw<DomainException>()
            .Which.Error.Should().Be(FieldManagementErrors.FarmOwnerInvalid);
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("ftp://example.com/farm.jpg")]
    [InlineData("/relative/farm.jpg")]
    public void Constructor_ShouldRejectNonHttpImageUrls(string imageUrl)
    {
        // Act
        var action = () => new Farm("Finca", "Ica", 1, imageUrl);

        // Assert
        action.Should().Throw<DomainException>()
            .Which.Error.Should().Be(FieldManagementErrors.FarmImageUrlInvalid);
    }

    [Fact]
    public void Constructor_ShouldNotRaiseEvents()
    {
        // Act
        var farm = new Farm("Finca", "Ica", 1, "https://example.com/farm.jpg");

        // Assert
        farm.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Rename_ShouldRaiseFarmRenamedEvent()
    {
        // Arrange
        var farm = TestGeometry.WithId(new Farm("Finca", "Ica", 1), new FarmId(10));

        // Act
        farm.Rename("Finca Nueva");

        // Assert
        farm.Name.Should().Be("Finca Nueva");
        farm.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<FarmRenamedEvent>()
            .Which.Name.Should().Be("Finca Nueva");
    }

    [Fact]
    public void Rename_ShouldNotRaiseEvent_WhenNameDoesNotChange()
    {
        // Arrange
        var farm = TestGeometry.WithId(new Farm("Finca", "Ica", 1), new FarmId(10));

        // Act
        farm.Rename("Finca");

        // Assert
        farm.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void ChangeOwner_ShouldRaiseFarmOwnerChangedEvent()
    {
        // Arrange
        var farm = TestGeometry.WithId(new Farm("Finca", "Ica", 1), new FarmId(10));

        // Act
        farm.ChangeOwner(2);

        // Assert
        farm.OwnerId.Should().Be(2);
        farm.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<FarmOwnerChangedEvent>()
            .Which.OwnerId.Should().Be(2);
    }

    [Fact]
    public void MarkAsCreated_ShouldRequireAPersistedIdentity()
    {
        // Arrange
        var farm = new Farm("Finca", "Ica", 1);

        // Act
        var action = () => farm.MarkAsCreated();

        // Assert
        action.Should().Throw<InvalidOperationException>();
        farm.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void MarkAsCreated_ShouldRaiseTheEventOnlyOnce()
    {
        // Arrange
        var farm = TestGeometry.WithId(new Farm("Finca", "Ica", 1), new FarmId(10));

        // Act
        farm.MarkAsCreated();
        farm.MarkAsCreated();

        // Assert
        farm.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<FarmCreatedEvent>()
            .Which.FarmId.Should().Be(10);
    }

    [Fact]
    public void ClearDomainEvents_ShouldRemoveEverything()
    {
        // Arrange
        var farm = TestGeometry.WithId(new Farm("Finca", "Ica", 1), new FarmId(10));
        farm.MarkAsCreated();
        farm.Rename("Otra");

        // Act
        farm.ClearDomainEvents();

        // Assert
        farm.DomainEvents.Should().BeEmpty();
    }
}
