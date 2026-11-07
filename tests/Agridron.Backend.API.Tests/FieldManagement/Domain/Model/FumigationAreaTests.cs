using Agridron.Backend.API.FieldManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.FieldManagement.Domain.Model.Errors;
using Agridron.Backend.API.FieldManagement.Domain.Model.Events;
using Agridron.Backend.API.FieldManagement.Domain.Model.ValueObjects;
using Agridron.Backend.API.Shared.Domain.Model.Exceptions;

namespace Agridron.Backend.API.Tests.FieldManagement.Domain.Model;

public class FumigationAreaTests
{
    private static FumigationArea CreateArea(
        ParcelId? parcelId = null,
        GeoJsonPolygon? geometry = null)
    {
        return new FumigationArea(
            parcelId ?? new ParcelId(1),
            geometry ?? TestGeometry.Square(-76.14, -14.07, -76.135, -14.065));
    }

    [Fact]
    public void Constructor_ShouldSetPropertiesAndComputeArea()
    {
        // Act
        var area = CreateArea();

        // Assert
        area.ParcelId.Value.Should().Be(1);
        area.AreaHectares.Should().BeApproximately(29.98m, 0.5m);
        area.Geometry.Should().NotBeNull();
        area.Id.Value.Should().Be(0);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_ShouldRejectInvalidParcelId(long parcelId)
    {
        // Act
        var action = () => CreateArea(parcelId: new ParcelId(parcelId));

        // Assert
        action.Should().Throw<DomainException>()
            .Which.Error.Should().Be(FieldManagementErrors.FumigationAreaParcelInvalid);
    }

    [Fact]
    public void Constructor_ShouldRejectGeometryWithNoArea()
    {
        // Arrange: a degenerate 0.00001 square has negligible area.
        var geometry = TestGeometry.Square(-76.14, -14.07, -76.14 + 0.00001, -14.07 + 0.00001);

        // Act
        var action = () => CreateArea(geometry: geometry);

        // Assert
        action.Should().Throw<DomainException>()
            .Which.Error.Should().Be(FieldManagementErrors.FumigationAreaHasNoArea);
    }

    [Fact]
    public void MoveToParcel_ShouldNotRaiseEvent()
    {
        // Arrange
        var area = TestGeometry.WithId(CreateArea(), new FumigationAreaId(5));

        // Act
        area.MoveToParcel(new ParcelId(3));

        // Assert
        area.ParcelId.Value.Should().Be(3);
        area.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void RedefineGeometry_ShouldRaiseFumigationAreaRedefinedEvent()
    {
        // Arrange
        var area = TestGeometry.WithId(CreateArea(), new FumigationAreaId(5));
        var newGeometry = TestGeometry.Square(-76.14, -14.07, -76.13, -14.06);

        // Act
        area.RedefineGeometry(newGeometry);

        // Assert
        area.AreaHectares.Should().BeApproximately(119.94m, 0.5m);
        area.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<FumigationAreaRedefinedEvent>()
            .Which.FumigationAreaId.Should().Be(5);
    }

    [Fact]
    public void EnsureFitsWithin_ShouldPass_WhenAreaIsSmallerThanParcel()
    {
        // Arrange
        var area = CreateArea(geometry: TestGeometry.Square(-76.14, -14.07, -76.135, -14.065));

        // Act
        area.EnsureFitsWithin(119.94m);

        // Assert - no exception
        area.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void EnsureFitsWithin_ShouldFail_WhenAreaExceedsParcel()
    {
        // Arrange
        var area = CreateArea(geometry: TestGeometry.Square(-76.14, -14.07, -76.13, -14.06));

        // Act
        var action = () => area.EnsureFitsWithin(29.98m);

        // Assert
        action.Should().Throw<DomainException>()
            .Which.Error.Should().Be(FieldManagementErrors.FumigationAreaExceedsParcelArea(
                area.AreaHectares, 29.98m));
    }

    [Fact]
    public void EnsureFitsWithin_ShouldRejectNonPositiveParcelArea()
    {
        // Arrange
        var area = CreateArea();

        // Act
        var action = () => area.EnsureFitsWithin(0);

        // Assert
        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void MarkAsCreated_ShouldRaiseFumigationAreaDefinedEvent()
    {
        // Arrange
        var area = TestGeometry.WithId(CreateArea(), new FumigationAreaId(9));

        // Act
        area.MarkAsCreated();

        // Assert
        area.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<FumigationAreaDefinedEvent>()
            .Which.FumigationAreaId.Should().Be(9);
    }

    [Fact]
    public void MarkAsCreated_ShouldRequireAPersistedIdentity()
    {
        // Arrange
        var area = CreateArea();

        // Act
        var action = () => area.MarkAsCreated();

        // Assert
        action.Should().Throw<InvalidOperationException>();
    }
}
