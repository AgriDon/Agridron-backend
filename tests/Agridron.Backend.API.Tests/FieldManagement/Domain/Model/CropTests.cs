using Agridron.Backend.API.FieldManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.FieldManagement.Domain.Model.Errors;
using Agridron.Backend.API.FieldManagement.Domain.Model.Events;
using Agridron.Backend.API.FieldManagement.Domain.Model.ValueObjects;
using Agridron.Backend.API.Shared.Domain.Model.Exceptions;

namespace Agridron.Backend.API.Tests.FieldManagement.Domain.Model;

public class CropTests
{
    [Fact]
    public void Constructor_ShouldTrimNameAndVariety()
    {
        // Act
        var crop = new Crop("  Maiz  ", "  Amarillo  ");

        // Assert
        crop.Name.Should().Be("Maiz");
        crop.Variety.Should().Be("Amarillo");
    }

    [Fact]
    public void Constructor_ShouldRejectMissingName()
    {
        // Act
        var action = () => new Crop(" ", "Amarillo");

        // Assert
        action.Should().Throw<DomainException>()
            .Which.Error.Should().Be(FieldManagementErrors.CropNameRequired);
    }

    [Fact]
    public void Constructor_ShouldRejectNameThatIsTooLong()
    {
        // Act
        var action = () => new Crop(new string('a', Crop.MaxNameLength + 1), "Amarillo");

        // Assert
        action.Should().Throw<DomainException>()
            .Which.Error.Should().Be(FieldManagementErrors.CropNameTooLong);
    }

    [Fact]
    public void Constructor_ShouldRejectMissingVariety()
    {
        // Act
        var action = () => new Crop("Maiz", "");

        // Assert
        action.Should().Throw<DomainException>()
            .Which.Error.Should().Be(FieldManagementErrors.CropVarietyRequired);
    }

    [Fact]
    public void Constructor_ShouldRejectVarietyThatIsTooLong()
    {
        // Act
        var action = () => new Crop("Maiz", new string('a', Crop.MaxVarietyLength + 1));

        // Assert
        action.Should().Throw<DomainException>()
            .Which.Error.Should().Be(FieldManagementErrors.CropVarietyTooLong);
    }

    [Fact]
    public void UpdateDetails_ShouldReplaceBothValues()
    {
        // Arrange
        var crop = new Crop("Maiz", "Amarillo");

        // Act
        crop.UpdateDetails("Papa", "Canchan");

        // Assert
        crop.Name.Should().Be("Papa");
        crop.Variety.Should().Be("Canchan");
    }

    [Fact]
    public void MarkAsCreated_ShouldRaiseCropCreatedEvent()
    {
        // Arrange
        var crop = TestGeometry.WithId(new Crop("Maiz", "Amarillo"), new CropId(5));

        // Act
        crop.MarkAsCreated();

        // Assert
        crop.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<CropCreatedEvent>()
            .Which.CropId.Should().Be(5);
    }

    [Fact]
    public void MarkAsCreated_ShouldRequireAPersistedIdentity()
    {
        // Arrange
        var crop = new Crop("Maiz", "Amarillo");

        // Act
        var action = () => crop.MarkAsCreated();

        // Assert
        action.Should().Throw<InvalidOperationException>();
    }
}
