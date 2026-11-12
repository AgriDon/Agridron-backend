using Agridron.Backend.API.FieldManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.FieldManagement.Domain.Model.Errors;
using Agridron.Backend.API.FieldManagement.Domain.Model.Events;
using Agridron.Backend.API.FieldManagement.Domain.Model.ValueObjects;
using Agridron.Backend.API.Shared.Domain.Model.Exceptions;

namespace Agridron.Backend.API.Tests.FieldManagement.Domain.Model;

public class ParcelTests
{
    private static Parcel CreateParcel(
        FarmId? farmId = null,
        CropId? cropId = null,
        string name = "Lote 1",
        GeoJsonPolygon? geometry = null)
    {
        return new Parcel(
            farmId ?? new FarmId(1),
            cropId ?? new CropId(1),
            name,
            geometry ?? TestGeometry.Square(-76.14, -14.07, -76.13, -14.06),
            "https://example.com/parcel.jpg");
    }

    [Fact]
    public void Constructor_ShouldTrimNameAndSetProperties()
    {
        // Act
        var parcel = CreateParcel();

        // Assert
        parcel.Name.Should().Be("Lote 1");
        parcel.FarmId.Value.Should().Be(1);
        parcel.CropId.Value.Should().Be(1);
        parcel.AreaHectares.Should().BeApproximately(119.94m, 0.5m);
        parcel.Geometry.Should().NotBeNull();
        parcel.ImageUrl.Should().Be("https://example.com/parcel.jpg");
        parcel.Id.Value.Should().Be(0);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldRejectMissingName(string name)
    {
        // Act
        var action = () => CreateParcel(name: name);

        // Assert
        action.Should().Throw<DomainException>()
            .Which.Error.Should().Be(FieldManagementErrors.ParcelNameRequired);
    }

    [Fact]
    public void Constructor_ShouldRejectNameThatIsTooLong()
    {
        // Act
        var action = () => CreateParcel(name: new string('a', Parcel.MaxNameLength + 1));

        // Assert
        action.Should().Throw<DomainException>()
            .Which.Error.Should().Be(FieldManagementErrors.ParcelNameTooLong);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_ShouldRejectInvalidFarmId(long farmId)
    {
        // Act
        var action = () => CreateParcel(farmId: new FarmId(farmId));

        // Assert
        action.Should().Throw<DomainException>()
            .Which.Error.Should().Be(FieldManagementErrors.ParcelFarmInvalid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_ShouldRejectInvalidCropId(long cropId)
    {
        // Act
        var action = () => CreateParcel(cropId: new CropId(cropId));

        // Assert
        action.Should().Throw<DomainException>()
            .Which.Error.Should().Be(FieldManagementErrors.ParcelCropInvalid);
    }

    [Fact]
    public void Constructor_ShouldRejectInvalidGeometry()
    {
        // Act
        var action = () => CreateParcel(geometry: TestGeometry.Square(0, 0, 0.00001, 0.00001));

        // Assert
        action.Should().Throw<DomainException>()
            .Which.Error.Should().Be(FieldManagementErrors.ParcelAreaOutOfRange);
    }

    [Fact]
    public void Constructor_ShouldRejectNonHttpImageUrls()
    {
        // Act
        var action = () => new Parcel(
            new FarmId(1), new CropId(1), "Lote",
            TestGeometry.Square(-76.14, -14.07, -76.13, -14.06),
            "not-a-url");

        // Assert
        action.Should().Throw<DomainException>()
            .Which.Error.Should().Be(FieldManagementErrors.ParcelImageUrlInvalid);
    }

    [Fact]
    public void Rename_ShouldRaiseNoEvent()
    {
        // Arrange
        var parcel = TestGeometry.WithId(CreateParcel(), new ParcelId(5));

        // Act
        parcel.Rename("Nuevo Lote");

        // Assert
        parcel.Name.Should().Be("Nuevo Lote");
        parcel.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void MoveToFarm_ShouldRaiseNoEvent()
    {
        // Arrange
        var parcel = TestGeometry.WithId(CreateParcel(), new ParcelId(5));

        // Act
        parcel.MoveToFarm(new FarmId(2));

        // Assert
        parcel.FarmId.Value.Should().Be(2);
        parcel.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void AssignCrop_ShouldRaiseParcelCropAssignedEvent()
    {
        // Arrange
        var parcel = TestGeometry.WithId(CreateParcel(), new ParcelId(5));

        // Act
        parcel.AssignCrop(new CropId(3));

        // Assert
        parcel.CropId.Value.Should().Be(3);
        parcel.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ParcelCropAssignedEvent>()
            .Which.CropId.Should().Be(3);
    }

    [Fact]
    public void RedefineGeometry_ShouldRaiseParcelGeometryUpdatedEvent()
    {
        // Arrange
        var parcel = TestGeometry.WithId(CreateParcel(), new ParcelId(5));
        var newGeometry = TestGeometry.Square(-76.14, -14.07, -76.135, -14.065);

        // Act
        parcel.RedefineGeometry(newGeometry);

        // Assert
        parcel.AreaHectares.Should().BeLessThan(119.94m);
        parcel.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ParcelGeometryUpdatedEvent>()
            .Which.ParcelId.Should().Be(5);
    }

    [Fact]
    public void MarkAsCreated_ShouldRaiseParcelCreatedEvent()
    {
        // Arrange
        var parcel = TestGeometry.WithId(CreateParcel(), new ParcelId(7));

        // Act
        parcel.MarkAsCreated();

        // Assert
        parcel.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ParcelCreatedEvent>()
            .Which.ParcelId.Should().Be(7);
    }

    [Fact]
    public void MarkAsCreated_ShouldRequireAPersistedIdentity()
    {
        // Arrange
        var parcel = CreateParcel();

        // Act
        var action = () => parcel.MarkAsCreated();

        // Assert
        action.Should().Throw<InvalidOperationException>();
    }
    [Fact]
    public void UpdatePhenologicalStage_ShouldTrimAndSetStage()
    {
        // Arrange
        var parcel = CreateParcel();

        // Act
        parcel.UpdatePhenologicalStage("  Floracion  ");

        // Assert
        parcel.PhenologicalStage.Should().Be("Floracion");
    }

    [Fact]
    public void UpdatePhenologicalStage_ShouldRejectStageTooLong()
    {
        // Arrange
        var parcel = CreateParcel();

        // Act
        var action = () => parcel.UpdatePhenologicalStage(new string('x', Parcel.MaxPhenologicalStageLength + 1));

        // Assert
        action.Should().Throw<DomainException>()
            .Which.Error.Should().Be(FieldManagementErrors.ParcelPhenologicalStageTooLong);
    }

    [Fact]
    public void DeactivateAndActivate_ShouldToggleIsActive()
    {
        // Arrange
        var parcel = CreateParcel();
        parcel.IsActive.Should().BeTrue();

        // Act
        parcel.Deactivate();

        // Assert
        parcel.IsActive.Should().BeFalse();

        // Act
        parcel.Activate();

        // Assert
        parcel.IsActive.Should().BeTrue();
    }
}
