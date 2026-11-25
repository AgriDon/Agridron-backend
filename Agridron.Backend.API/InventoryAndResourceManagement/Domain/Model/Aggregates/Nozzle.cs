using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.Errors;
using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.Events;
using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.ValueObjects.ValueObjects;
using Agridron.Backend.API.Shared.Domain.Model.Aggregates;
using Agridron.Backend.API.Shared.Domain.Model.Entities;
using Agridron.Backend.API.Shared.Domain.Model.Exceptions;

namespace Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.Aggregates;

public sealed class Nozzle : AggregateRoot<NozzleId>, IAuditableEntity
{
    private Nozzle()
    {

    }

    public Nozzle( NozzleType type,
                    decimal diameterMm,
                    int stockUnits,
                    NozzleStatus status = NozzleStatus.Good) : base(NozzleId.Empty)
    {

        SetType(type);
        SetDiameter(diameterMm);
        SetStock(stockUnits);
        SetStatus(status);
        IsActive = true;
    }

    public NozzleType Type { get; private set; } = NozzleType.FlatFan;
    public decimal DiameterMm { get; private set; }
    public int StockUnits { get; private set; }
    public NozzleStatus Status { get; private set; } = NozzleStatus.Good;
    public bool IsActive { get; private set; } = true;

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public void UpdateDetails(NozzleType type,
                                decimal diameterMm,
                                int stockUnits,
                                NozzleStatus status)
    {

        SetType(type);
        SetDiameter(diameterMm);
        SetStock(stockUnits);
        ChangeStatus(status);
    }

    /*Business rule about the condition alternates between good and worn nozzle
    (button "Cambiar condición")*/

    public void ToggleStatus()
    {
        ChangeStatus(Status == NozzleStatus.Good? NozzleStatus.Worn: NozzleStatus.Good);
    }

    /// <summary>
    /// A worn nozzle must be replaced before being used
    /// </summary>
    /// <returns></returns>
    public bool IsWorn() => Status == NozzleStatus.Worn;

    public void Dactivate(){IsActive = false;}
    public void Activate(){IsActive = true;}

    public void ChangeStatus(NozzleStatus newStatus)
    {
        var oldStatus = Status;
        SetStatus(newStatus);
        if (oldStatus != newStatus && Id.Value > 0)
        {
            AddDomainEvent(new NozzleStatusChangedEvent(Id.Value, oldStatus, newStatus, DateTimeOffset.UtcNow));
        }
    }


    private void SetType(NozzleType type)
    {
        if (!Enum.IsDefined(type))
        {
            throw new DomainException(InventoryErrors.NozzleTypeInvalid);
        }
        Type = type;
    }

    private void SetDiameter(decimal diameterMm)
    {
        if (diameterMm <= 0m)
        {
            throw new DomainException(InventoryErrors.NozzleDiameterInvalid);
        }

        DiameterMm = diameterMm;
    }

    private void SetStock(int stockUnits)
    {
        if (stockUnits < 0)
        {
            throw new DomainException(InventoryErrors.NozzleStockInvalid);
        }
        StockUnits = stockUnits;
    }

    private void SetStatus(NozzleStatus status)
    {
        if (!Enum.IsDefined(status))
        {
            throw new DomainException(InventoryErrors.NozzleStatusInvalid);
        }
        Status = status;
    }


}
