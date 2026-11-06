using Agridron.Backend.API.FlightOperations.Domain.Model.Errors;
using Agridron.Backend.API.FlightOperations.Domain.Model.Events;
using Agridron.Backend.API.FlightOperations.Domain.Model.Validation;
using Agridron.Backend.API.FlightOperations.Domain.Model.ValueObjects;
using Agridron.Backend.API.Shared.Domain.Model.Aggregates;
using Agridron.Backend.API.Shared.Domain.Model.Entities;
using Agridron.Backend.API.Shared.Domain.Model.Exceptions;

namespace Agridron.Backend.API.FlightOperations.Domain.Model.Aggregates;

/// <summary>
///     Aggregate representing a spray drone in the fleet.
/// </summary>
public sealed class Drone : AggregateRoot<DroneId>, IAuditableEntity
{
    public const int MaxSerialNumberLength = 60;
    public const int MaxModelLength = 100;
    public const int MaxImageUrlLength = 500;

    private bool _creationEventRaised;

    private Drone()
    {
    }

    public Drone(
        string serialNumber,
        string model,
        decimal capacityLiters,
        long? nozzleId = null,
        string? imageUrl = null) : base(DroneId.Empty)
    {
        SetSerialNumber(serialNumber);
        SetModel(model);
        SetCapacity(capacityLiters);
        NozzleId = nozzleId;
        ChangeImage(imageUrl);

        Status = DroneStatus.Available;
        BatteryLevel = 100m;
        FlightHours = 0m;
        IsActive = true;
    }

    public string SerialNumber { get; private set; } = string.Empty;
    public string Model { get; private set; } = string.Empty;
    public decimal CapacityLiters { get; private set; }
    public DroneStatus Status { get; private set; } = DroneStatus.Available;
    public decimal BatteryLevel { get; private set; } = 100m;
    public decimal FlightHours { get; private set; }
    public long? NozzleId { get; private set; }
    public DateTimeOffset? LastMaintenanceDate { get; private set; }
    public DateTimeOffset? NextMaintenanceDate { get; private set; }
    public string? ImageUrl { get; private set; }
    public bool IsActive { get; private set; } = true;

    // IAuditableEntity
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public void SetSerialNumber(string serialNumber)
    {
        SerialNumber = FlightOperationsGuard.RequiredText(
            serialNumber,
            FlightOperationsErrors.DroneSerialNumberRequired,
            MaxSerialNumberLength,
            FlightOperationsErrors.DroneSerialNumberTooLong);
    }

    public void SetModel(string model)
    {
        Model = FlightOperationsGuard.RequiredText(
            model,
            FlightOperationsErrors.DroneModelRequired,
            MaxModelLength,
            FlightOperationsErrors.DroneModelTooLong);
    }

    public void SetCapacity(decimal capacityLiters)
    {
        if (capacityLiters <= 0m)
        {
            throw new DomainException(FlightOperationsErrors.DroneCapacityInvalid);
        }

        CapacityLiters = capacityLiters;
    }

    public void ChangeImage(string? imageUrl)
    {
        ImageUrl = FlightOperationsGuard.OptionalUrl(
            imageUrl,
            MaxImageUrlLength,
            FlightOperationsErrors.DroneImageUrlInvalid);
    }

    public void UpdateDetails(
        string model,
        decimal capacityLiters,
        long? nozzleId,
        string? imageUrl,
        DateTimeOffset? lastMaintenanceDate = null,
        DateTimeOffset? nextMaintenanceDate = null)
    {
        SetModel(model);
        SetCapacity(capacityLiters);
        NozzleId = nozzleId;
        ChangeImage(imageUrl);

        if (lastMaintenanceDate.HasValue)
        {
            LastMaintenanceDate = lastMaintenanceDate;
        }

        if (nextMaintenanceDate.HasValue)
        {
            NextMaintenanceDate = nextMaintenanceDate;
        }
    }

    public void AssignToMission()
    {
        if (Status != DroneStatus.Available)
        {
            throw new DomainException(FlightOperationsErrors.DroneNotAvailable(Id.Value, Status.ToString()));
        }

        ChangeStatus(DroneStatus.Assigned);
    }

    public void MarkInFlight()
    {
        ChangeStatus(DroneStatus.InFlight);
    }

    public void MarkPaused()
    {
        ChangeStatus(DroneStatus.Paused);
    }

    public void ReleaseFromMission(decimal addedFlightHours = 0m)
    {
        if (addedFlightHours > 0m)
        {
            FlightHours += addedFlightHours;
        }

        ChangeStatus(DroneStatus.Available);
    }

    public void SendToMaintenance()
    {
        if (Status == DroneStatus.InFlight)
        {
            throw new DomainException(FlightOperationsErrors.DroneBusy(Id.Value, Status.ToString()));
        }

        ChangeStatus(DroneStatus.Maintenance);
    }

    public void CompleteMaintenance(DateTimeOffset maintenanceDate, DateTimeOffset? nextDate)
    {
        LastMaintenanceDate = maintenanceDate;
        NextMaintenanceDate = nextDate;
        ChangeStatus(DroneStatus.Available);
    }

    public void UpdateBatteryLevel(decimal batteryLevel)
    {
        BatteryLevel = Math.Clamp(batteryLevel, 0m, 100m);
    }

    public void Deactivate()
    {
        if (Status == DroneStatus.Assigned || Status == DroneStatus.InFlight)
        {
            throw new DomainException(FlightOperationsErrors.DroneBusy(Id.Value, Status.ToString()));
        }

        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void MarkAsCreated()
    {
        if (_creationEventRaised) return;
        if (Id.Value <= 0)
        {
            throw new InvalidOperationException("The drone must be persisted before raising its creation event.");
        }

        AddDomainEvent(new DroneRegisteredEvent(
            Id.Value,
            SerialNumber,
            Model,
            CapacityLiters,
            DateTimeOffset.UtcNow));

        _creationEventRaised = true;
    }

    private void ChangeStatus(DroneStatus newStatus)
    {
        if (Status == newStatus) return;

        var oldStatus = Status;
        Status = newStatus;

        if (Id.Value > 0)
        {
            AddDomainEvent(new DroneStatusChangedEvent(Id.Value, oldStatus, newStatus, DateTimeOffset.UtcNow));
        }
    }
}
