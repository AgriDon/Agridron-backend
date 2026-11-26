using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.Errors;
using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.Events;
using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.ValueObjects.ValueObjects;
using Agridron.Backend.API.Shared.Domain.Model.Entities;
using Agridron.Backend.API.Shared.Domain.Model.Exceptions;

namespace Agridron.Backend.API.Tests.InventoryAndResourceManagement.Domain.Model;

public sealed class ChemicalTests
{
    private static readonly DateOnly Expiration = new(2027, 8, 30);

    private static Chemical CreateChemical(decimal stock = 120m, decimal minimum = 40m) =>
        new("Glifosato 48%", ChemicalType.Herbicide, stock, minimum, Expiration);

    /// <summary>Simulates a chemical that was already saved in the database (id assigned).</summary>
    private static void SetPersistedId(Chemical chemical, long id)
    {
        typeof(Entity<ChemicalId>).GetProperty(nameof(Chemical.Id))!.SetValue(chemical, new ChemicalId(id));
    }

    [Fact]
    public void Constructor_WithValidData_ShouldCreateActiveChemical()
    {
        // Act
        var chemical = new Chemical("  Glifosato 48%  ", ChemicalType.Herbicide, 120m, 40m, Expiration);

        // Assert
        chemical.Name.Should().Be("Glifosato 48%");
        chemical.Type.Should().Be(ChemicalType.Herbicide);
        chemical.StockLiters.Should().Be(120m);
        chemical.MinimumStockLiters.Should().Be(40m);
        chemical.ExpirationDate.Should().Be(Expiration);
        chemical.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithoutName_ShouldThrowValidation(string? name)
    {
        // Act
        var act = () => new Chemical(name!, ChemicalType.Herbicide, 10m, 5m, Expiration);

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Error.Code.Should().Be(InventoryErrors.ChemicalNameRequired.Code);
    }

    [Fact]
    public void Constructor_WithTooLongName_ShouldThrowValidation()
    {
        // Act
        var act = () => new Chemical(new string('x', 101), ChemicalType.Herbicide, 10m, 5m, Expiration);

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Error.Code.Should().Be(InventoryErrors.ChemicalNameTooLong.Code);
    }

    [Fact]
    public void Constructor_WithUndefinedType_ShouldThrowValidation()
    {
        // Act
        var act = () => new Chemical("Glifosato 48%", (ChemicalType)99, 10m, 5m, Expiration);

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Error.Code.Should().Be(InventoryErrors.ChemicalTypeInvalid.Code);
    }

    [Fact]
    public void Constructor_WithNegativeStock_ShouldThrowValidation()
    {
        // Act
        var act = () => CreateChemical(stock: -1m);

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Error.Code.Should().Be(InventoryErrors.ChemicalStockInvalid.Code);
    }

    [Fact]
    public void Constructor_WithNegativeMinimumStock_ShouldThrowValidation()
    {
        // Act
        var act = () => CreateChemical(minimum: -1m);

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Error.Code.Should().Be(InventoryErrors.ChemicalMinimumStockInvalid.Code);
    }

    [Fact]
    public void Constructor_WithoutExpirationDate_ShouldThrowValidation()
    {
        // Act
        var act = () => new Chemical("Glifosato 48%", ChemicalType.Herbicide, 10m, 5m, default);

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Error.Code.Should().Be(InventoryErrors.ChemicalExpirationDateRequired.Code);
    }

    [Fact]
    public void Restock_WithPositiveLiters_ShouldAddToStock()
    {
        // Arrange
        var chemical = CreateChemical(stock: 25m, minimum: 30m);

        // Act
        chemical.Restock(20m);

        // Assert
        chemical.StockLiters.Should().Be(45m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Restock_WithZeroOrNegativeLiters_ShouldThrowValidation(decimal liters)
    {
        // Arrange
        var chemical = CreateChemical(stock: 25m);

        // Act
        var act = () => chemical.Restock(liters);

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Error.Code.Should().Be(InventoryErrors.ChemicalRestockAmountInvalid.Code);
        chemical.StockLiters.Should().Be(25m);
    }

    [Fact]
    public void Restock_OnPersistedChemical_ShouldRaiseRestockedEvent()
    {
        // Arrange
        var chemical = CreateChemical(stock: 25m);
        SetPersistedId(chemical, 2);

        // Act
        chemical.Restock(20m);

        // Assert
        var domainEvent = chemical.DomainEvents.Should().ContainSingle().Which
            .Should().BeOfType<ChemicalRestockedEvent>().Subject;
        domainEvent.ChemicalId.Should().Be(2);
        domainEvent.AddedLiters.Should().Be(20m);
        domainEvent.NewStockLiters.Should().Be(45m);
    }

    [Theory]
    [InlineData(25, 30, true)]
    [InlineData(30, 30, true)]
    [InlineData(120, 40, false)]
    public void IsLowStock_ShouldCompareStockWithMinimum(decimal stock, decimal minimum, bool expected)
    {
        // Arrange
        var chemical = CreateChemical(stock, minimum);

        // Act & Assert
        chemical.IsLowStock().Should().Be(expected);
    }

    [Fact]
    public void UpdateDetails_WithValidData_ShouldReplaceValues()
    {
        // Arrange
        var chemical = CreateChemical();
        var newExpiration = new DateOnly(2028, 1, 15);

        // Act
        chemical.UpdateDetails("Mancozeb 80%", ChemicalType.Fungicide, 60m, 20m, newExpiration);

        // Assert
        chemical.Name.Should().Be("Mancozeb 80%");
        chemical.Type.Should().Be(ChemicalType.Fungicide);
        chemical.StockLiters.Should().Be(60m);
        chemical.MinimumStockLiters.Should().Be(20m);
        chemical.ExpirationDate.Should().Be(newExpiration);
    }

    [Fact]
    public void Deactivate_ShouldSoftDeleteChemical()
    {
        // Arrange
        var chemical = CreateChemical();

        // Act
        chemical.Deactivate();

        // Assert
        chemical.IsActive.Should().BeFalse();
    }
}
