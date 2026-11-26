using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.Errors;
using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.Events;
using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.ValueObjects.ValueObjects;
using Agridron.Backend.API.Shared.Domain.Model.Entities;
using Agridron.Backend.API.Shared.Domain.Model.Exceptions;

namespace Agridron.Backend.API.Tests.InventoryAndResourceManagement.Domain.Model;

public sealed class NozzleTests
{
    /// <summary>Simulates a nozzle that was already saved in the database (id assigned).</summary>
    private static void SetPersistedId(Nozzle nozzle, long id)
    {
        typeof(Entity<NozzleId>).GetProperty(nameof(Nozzle.Id))!.SetValue(nozzle, new NozzleId(id));
    }

    [Fact]
    public void Constructor_WithValidData_ShouldCreateGoodActiveNozzle()
    {
        // Act
        var nozzle = new Nozzle(NozzleType.FlatFan, 5m, 12);

        // Assert
        nozzle.Type.Should().Be(NozzleType.FlatFan);
        nozzle.DiameterMm.Should().Be(5m);
        nozzle.StockUnits.Should().Be(12);
        nozzle.Status.Should().Be(NozzleStatus.Good);
        nozzle.IsWorn().Should().BeFalse();
        nozzle.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void Constructor_WithZeroOrNegativeDiameter_ShouldThrowValidation(decimal diameter)
    {
        // Act
        var act = () => new Nozzle(NozzleType.FlatFan, diameter, 12);

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Error.Code.Should().Be(InventoryErrors.NozzleDiameterInvalid.Code);
    }

    [Fact]
    public void Constructor_WithNegativeStock_ShouldThrowValidation()
    {
        // Act
        var act = () => new Nozzle(NozzleType.HollowCone, 3m, -1);

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Error.Code.Should().Be(InventoryErrors.NozzleStockInvalid.Code);
    }

    [Fact]
    public void Constructor_WithZeroStock_ShouldBeAllowed()
    {
        // Act
        var nozzle = new Nozzle(NozzleType.HollowCone, 3m, 0);

        // Assert
        nozzle.StockUnits.Should().Be(0);
    }

    [Fact]
    public void Constructor_WithUndefinedType_ShouldThrowValidation()
    {
        // Act
        var act = () => new Nozzle((NozzleType)99, 3m, 4);

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Error.Code.Should().Be(InventoryErrors.NozzleTypeInvalid.Code);
    }

    [Fact]
    public void Constructor_WithUndefinedStatus_ShouldThrowValidation()
    {
        // Act
        var act = () => new Nozzle(NozzleType.FlatFan, 3m, 4, (NozzleStatus)99);

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Error.Code.Should().Be(InventoryErrors.NozzleStatusInvalid.Code);
    }

    [Fact]
    public void ToggleStatus_ShouldAlternateBetweenGoodAndWorn()
    {
        // Arrange
        var nozzle = new Nozzle(NozzleType.FlatFan, 5m, 12);

        // Act & Assert
        nozzle.ToggleStatus();
        nozzle.Status.Should().Be(NozzleStatus.Worn);
        nozzle.IsWorn().Should().BeTrue();

        nozzle.ToggleStatus();
        nozzle.Status.Should().Be(NozzleStatus.Good);
    }

    [Fact]
    public void ToggleStatus_OnPersistedNozzle_ShouldRaiseStatusChangedEvent()
    {
        // Arrange
        var nozzle = new Nozzle(NozzleType.HollowCone, 3m, 4);
        SetPersistedId(nozzle, 2);

        // Act
        nozzle.ToggleStatus();

        // Assert
        var domainEvent = nozzle.DomainEvents.Should().ContainSingle().Which
            .Should().BeOfType<NozzleStatusChangedEvent>().Subject;
        domainEvent.NozzleId.Should().Be(2);
        domainEvent.OldStatus.Should().Be(NozzleStatus.Good);
        domainEvent.NewStatus.Should().Be(NozzleStatus.Worn);
    }

    [Fact]
    public void UpdateDetails_WithValidData_ShouldReplaceValues()
    {
        // Arrange
        var nozzle = new Nozzle(NozzleType.FlatFan, 5m, 12);

        // Act
        nozzle.UpdateDetails(NozzleType.HollowCone, 4m, 10, NozzleStatus.Worn);

        // Assert
        nozzle.Type.Should().Be(NozzleType.HollowCone);
        nozzle.DiameterMm.Should().Be(4m);
        nozzle.StockUnits.Should().Be(10);
        nozzle.Status.Should().Be(NozzleStatus.Worn);
    }

    [Fact]
    public void Deactivate_ShouldSoftDeleteNozzle()
    {
        // Arrange
        var nozzle = new Nozzle(NozzleType.FlatFan, 5m, 12);

        // Act
        nozzle.Deactivate();

        // Assert
        nozzle.IsActive.Should().BeFalse();
    }
}
