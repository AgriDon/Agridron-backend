using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.Errors;
using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.Events;
using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.ValueObjects.ValueObjects;
using Agridron.Backend.API.Shared.Domain.Model.Aggregates;
using Agridron.Backend.API.Shared.Domain.Model.Entities;
using Agridron.Backend.API.Shared.Domain.Model.Exceptions;

namespace Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.Aggregates;

public sealed class Chemical : AggregateRoot<ChemicalId>, IAuditableEntity
{

    private Chemical()
    {

    }

    public Chemical(string name,
        ChemicalType type,
        decimal stockLiters,
        decimal minimumStockLiters,
        DateOnly expirationDate) : base(ChemicalId.Empty)
    {
        SetName(name);
        SetType(type);
        SetStock(stockLiters);
        SetMinimumStock(minimumStockLiters);
        SetExpirationDate(expirationDate);
        IsActive = true;
    }

    public string Name { get; private set; } = string.Empty;
    public ChemicalType Type { get; private set; } = ChemicalType.Herbicide;
    public decimal StockLiters { get; private set; }
    public decimal MinimumStockLiters { get; private set; }
    public DateOnly ExpirationDate { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }


    /**
    * Edits the product data. The stock can be corrected by hand here.
    */
    public void UpdateDetails(
        string name,
        ChemicalType type,
        decimal stockLiters,
        decimal minimumStockLiters,
        DateOnly expirationDate)
    {
        SetName(name);
        SetType(type);
        SetStock(stockLiters);
        SetMinimumStock(minimumStockLiters);
        SetExpirationDate(expirationDate);
    }



/*Business rule about restock
Always added a positive amount of liters to the current stock*/

    public void Restock(decimal liters)
    {
        if (liters <= 0) { throw new DomainException(InventoryErrors.ChemicalRestockAmountInvalid); }

        StockLiters += liters;
        if (Id.Value > 0)
        {
            AddDomainEvent(new ChemicalRestockedEvent(Id.Value, liters, StockLiters, DateTimeOffset.UtcNow));
        }

    }

    public bool IsLowStock() => StockLiters <= MinimumStockLiters;

    public void Activate() { IsActive = true; }
    public void Deactivate() { IsActive = false; }

   public const int MaxNameLength = 100;

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException(InventoryErrors.ChemicalNameRequired);
        }

        ///Elimated all witheSpace in the line of text
        var trimmed = name.Trim();
        if (trimmed.Length > MaxNameLength)
        {
            throw new DomainException(InventoryErrors.ChemicalNameTooLong);
        }
        Name = trimmed;
    }

    private void SetType(ChemicalType type)
    {
        if (!Enum.IsDefined(type))
        {
            throw new DomainException(InventoryErrors.ChemicalTypeInvalid);
        }
        Type = type;
    }

    private void SetStock(decimal stockLiters)
    {
        if (stockLiters < 0m)
        {
            throw new DomainException(InventoryErrors.ChemicalStockInvalid);
        }

        StockLiters = stockLiters;
    }

    private void SetMinimumStock(decimal minimumStockLiters)
    {
        if (minimumStockLiters < 0m)
        {
            throw new DomainException(InventoryErrors.ChemicalMinimumStockInvalid);
        }
        MinimumStockLiters = minimumStockLiters;
    }

    private void SetExpirationDate(DateOnly expirationDate)
    {
        if (expirationDate == default)
        {
            throw new DomainException(InventoryErrors.ChemicalExpirationDateRequired);
        }
        ExpirationDate = expirationDate;
    }



}
