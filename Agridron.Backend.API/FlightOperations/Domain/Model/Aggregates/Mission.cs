using Agridron.Backend.API.FlightOperations.Domain.Model.Errors;
using Agridron.Backend.API.FlightOperations.Domain.Model.Events;
using Agridron.Backend.API.FlightOperations.Domain.Model.Validation;
using Agridron.Backend.API.FlightOperations.Domain.Model.ValueObjects;
using Agridron.Backend.API.Shared.Domain.Model.Aggregates;
using Agridron.Backend.API.Shared.Domain.Model.Entities;
using Agridron.Backend.API.Shared.Domain.Model.Exceptions;

namespace Agridron.Backend.API.FlightOperations.Domain.Model.Aggregates;

/// <summary>
///     Aggregate representing an agricultural spraying flight mission.
/// </summary>
public sealed class Mission : AggregateRoot<MissionId>, IAuditableEntity
{
    public const int MaxCodeLength = 30;
    public const int MaxFarmAreaLength = 120;
    public const int MaxCropTypeLength = 80;
    public const int MaxOperatorNameLength = 120;
    public const int MaxTreatmentTypeLength = 60;
    public const int MaxProductNameLength = 100;
    public const int MaxNotesLength = 1000;
    public const decimal MaxAreaHectares = 100_000m;

    private bool _creationEventRaised;

    private Mission()
    {
    }

    public Mission(
        string code,
        long? farmId,
        long? parcelId,
        string farmArea,
        string cropType,
        DateTimeOffset scheduledAt,
        decimal plannedAreaHectares,
        string? operatorName = null,
        string? treatmentType = null,
        string? productName = null,
        decimal? productDose = null,
        string? notes = null,
        DroneId? droneId = null,
        int progress = 0) : base(MissionId.Empty)
    {
        SetCode(code);
        FarmId = farmId;
        ParcelId = parcelId;
        SetFarmArea(farmArea);
        SetCropType(cropType);
        ScheduledAt = scheduledAt;
        SetPlannedArea(plannedAreaHectares);

        OperatorName = FlightOperationsGuard.OptionalText(operatorName, MaxOperatorNameLength, FlightOperationsErrors.MissionOperatorNameTooLong);
        TreatmentType = FlightOperationsGuard.OptionalText(treatmentType, MaxTreatmentTypeLength, FlightOperationsErrors.MissionTreatmentTypeTooLong);
        ProductName = FlightOperationsGuard.OptionalText(productName, MaxProductNameLength, FlightOperationsErrors.MissionProductNameTooLong);

        if (productDose.HasValue && productDose.Value < 0m)
        {
            throw new DomainException(FlightOperationsErrors.MissionProductDoseInvalid);
        }
        ProductDose = productDose;

        Notes = FlightOperationsGuard.OptionalText(notes, MaxNotesLength, FlightOperationsErrors.MissionNotesTooLong);
        DroneId = droneId;
        Status = MissionStatus.Planned;
        Progress = Math.Clamp(progress, 0, 100);
        TreatedAreaHectares = 0m;
    }

    public string Code { get; private set; } = string.Empty;
    public long? FarmId { get; private set; }
    public long? ParcelId { get; private set; }
    public DroneId? DroneId { get; private set; }
    public long? ChemicalId { get; private set; }
    public long? OperatorId { get; private set; }
    public string FarmArea { get; private set; } = string.Empty;
    public string CropType { get; private set; } = string.Empty;
    public string? OperatorName { get; private set; }
    public string? TreatmentType { get; private set; }
    public string? ProductName { get; private set; }
    public decimal? ProductDose { get; private set; }
    public string? Notes { get; private set; }
    public MissionStatus Status { get; private set; } = MissionStatus.Planned;
    public DateTimeOffset ScheduledAt { get; private set; }
    public decimal PlannedAreaHectares { get; private set; }
    public decimal TreatedAreaHectares { get; private set; }
    public int Progress { get; private set; }
    public FlightParameters? FlightParameters { get; private set; }
    public GeoCoordinate? LastPausePoint { get; private set; }
    public DateTimeOffset? StartedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public bool IsActive { get; private set; } = true;

    // IAuditableEntity
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public void Deactivate() => IsActive = false;

    public void SetCode(string code)
    {
        Code = FlightOperationsGuard.RequiredText(
            code,
            FlightOperationsErrors.MissionCodeRequired,
            MaxCodeLength,
            FlightOperationsErrors.MissionCodeTooLong);
    }

    public void SetFarmArea(string farmArea)
    {
        FarmArea = FlightOperationsGuard.OptionalText(
            farmArea,
            MaxFarmAreaLength,
            FlightOperationsErrors.MissionFarmAreaTooLong) ?? "Parcela";
    }

    public void SetCropType(string cropType)
    {
        CropType = FlightOperationsGuard.OptionalText(
            cropType,
            MaxCropTypeLength,
            FlightOperationsErrors.MissionCropTypeTooLong) ?? "Cultivo";
    }

    public void SetPlannedArea(decimal plannedArea)
    {
        if (plannedArea <= 0m || plannedArea > MaxAreaHectares)
        {
            throw new DomainException(FlightOperationsErrors.MissionPlannedAreaOutOfRange);
        }

        PlannedAreaHectares = plannedArea;
    }

    public void UpdateProgress(int progress)
    {
        Progress = Math.Clamp(progress, 0, 100);
    }

    public void UpdateGeneralDetails(
        long? farmId,
        long? parcelId,
        string farmArea,
        string cropType,
        DateTimeOffset scheduledAt,
        decimal plannedAreaHectares,
        string? treatmentType,
        string? productName,
        decimal? productDose,
        string? notes,
        string? operatorName)
    {
        if (Status == MissionStatus.Completed || Status == MissionStatus.Cancelled)
        {
            throw new DomainException(FlightOperationsErrors.MissionInvalidStatusTransition(Status.ToString(), "Update"));
        }

        FarmId = farmId;
        ParcelId = parcelId;
        SetFarmArea(farmArea);
        SetCropType(cropType);
        ScheduledAt = scheduledAt;
        SetPlannedArea(plannedAreaHectares);

        TreatmentType = FlightOperationsGuard.OptionalText(treatmentType, MaxTreatmentTypeLength, FlightOperationsErrors.MissionTreatmentTypeTooLong);
        ProductName = FlightOperationsGuard.OptionalText(productName, MaxProductNameLength, FlightOperationsErrors.MissionProductNameTooLong);

        if (productDose.HasValue && productDose.Value < 0m)
        {
            throw new DomainException(FlightOperationsErrors.MissionProductDoseInvalid);
        }
        ProductDose = productDose;

        Notes = FlightOperationsGuard.OptionalText(notes, MaxNotesLength, FlightOperationsErrors.MissionNotesTooLong);

        if (!string.IsNullOrWhiteSpace(operatorName))
        {
            OperatorName = FlightOperationsGuard.OptionalText(operatorName, MaxOperatorNameLength, FlightOperationsErrors.MissionOperatorNameTooLong);
        }
    }

    /// <summary>
    ///     Assigns drone, spraying inputs and dosage to the mission (MI-002).
    /// </summary>
    public void AssignResources(
        DroneId droneId,
        string? treatmentType,
        string? productName,
        decimal? productDose,
        long? chemicalId = null)
    {
        if (Status == MissionStatus.Completed || Status == MissionStatus.Cancelled)
        {
            throw new DomainException(FlightOperationsErrors.MissionInvalidStatusTransition(Status.ToString(), "AssignResources"));
        }

        if (droneId.Value <= 0)
        {
            throw new DomainException(FlightOperationsErrors.DroneNotFound(droneId.Value));
        }

        DroneId = droneId;
        ChemicalId = chemicalId;

        if (!string.IsNullOrWhiteSpace(treatmentType))
        {
            TreatmentType = FlightOperationsGuard.OptionalText(treatmentType, MaxTreatmentTypeLength, FlightOperationsErrors.MissionTreatmentTypeTooLong);
        }

        if (!string.IsNullOrWhiteSpace(productName))
        {
            ProductName = FlightOperationsGuard.OptionalText(productName, MaxProductNameLength, FlightOperationsErrors.MissionProductNameTooLong);
        }

        if (productDose.HasValue)
        {
            if (productDose.Value < 0m)
            {
                throw new DomainException(FlightOperationsErrors.MissionProductDoseInvalid);
            }
            ProductDose = productDose;
        }

        if (Id.Value > 0)
        {
            AddDomainEvent(new MissionResourcesAssignedEvent(
                Id.Value,
                droneId.Value,
                TreatmentType,
                ProductName,
                ProductDose,
                DateTimeOffset.UtcNow));
        }
    }

    /// <summary>
    ///     Assigns designated technical operator to the mission (MI-003).
    /// </summary>
    public void AssignOperator(string operatorName, long? operatorId = null)
    {
        if (Status == MissionStatus.Completed || Status == MissionStatus.Cancelled)
        {
            throw new DomainException(FlightOperationsErrors.MissionInvalidStatusTransition(Status.ToString(), "AssignOperator"));
        }

        OperatorName = FlightOperationsGuard.RequiredText(
            operatorName,
            FlightOperationsErrors.MissionOperatorNameTooLong,
            MaxOperatorNameLength,
            FlightOperationsErrors.MissionOperatorNameTooLong);

        OperatorId = operatorId;

        if (Id.Value > 0)
        {
            AddDomainEvent(new MissionOperatorAssignedEvent(Id.Value, OperatorName, OperatorId, DateTimeOffset.UtcNow));
        }
    }

    /// <summary>
    ///     Configures flight navigation and spraying operational parameters (MI-004).
    /// </summary>
    public void ConfigureFlightParameters(FlightParameters parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        if (Status == MissionStatus.Completed || Status == MissionStatus.Cancelled)
        {
            throw new DomainException(FlightOperationsErrors.MissionInvalidStatusTransition(Status.ToString(), "ConfigureFlightParameters"));
        }

        FlightParameters = parameters;

        if (Id.Value > 0)
        {
            AddDomainEvent(new FlightParametersConfiguredEvent(
                Id.Value,
                parameters.AltitudeMeters,
                parameters.SpeedMetersPerSecond,
                parameters.SprayRateLitersPerHectare,
                parameters.OverlapPercentage,
                DateTimeOffset.UtcNow));
        }
    }

    /// <summary>
    ///     Formally approves the mission for flight deployment (MI-007).
    /// </summary>
    public void Approve()
    {
        if (Status != MissionStatus.Planned)
        {
            throw new DomainException(FlightOperationsErrors.MissionInvalidStatusTransition(Status.ToString(), "Approve"));
        }

        if (!DroneId.HasValue || DroneId.Value.Value <= 0)
        {
            throw new DomainException(FlightOperationsErrors.MissionDroneRequiredForApproval);
        }

        Status = MissionStatus.Authorized;

        if (Id.Value > 0)
        {
            AddDomainEvent(new MissionApprovedEvent(Id.Value, DroneId.Value.Value, DateTimeOffset.UtcNow));
        }
    }

    /// <summary>
    ///     Starts mission execution in the field.
    /// </summary>
    public void Start(DateTimeOffset startedAt)
    {
        if (Status != MissionStatus.Authorized && Status != MissionStatus.Planned)
        {
            throw new DomainException(FlightOperationsErrors.MissionInvalidStatusTransition(Status.ToString(), "Start"));
        }

        StartedAt = startedAt;
        Status = MissionStatus.InProgress;

        if (Id.Value > 0)
        {
            AddDomainEvent(new MissionStartedEvent(Id.Value, startedAt, DateTimeOffset.UtcNow));
        }
    }

    /// <summary>
    ///     Pauses mission execution due to emergency, weather, or maintenance (TR-004).
    /// </summary>
    public void Pause(GeoCoordinate? pausePoint = null)
    {
        if (Status != MissionStatus.InProgress)
        {
            throw new DomainException(FlightOperationsErrors.MissionInvalidStatusTransition(Status.ToString(), "Pause"));
        }

        LastPausePoint = pausePoint;
        Status = MissionStatus.Paused;

        if (Id.Value > 0)
        {
            AddDomainEvent(new MissionPausedEvent(Id.Value, pausePoint?.Latitude, pausePoint?.Longitude, DateTimeOffset.UtcNow));
        }
    }

    /// <summary>
    ///     Resumes a paused mission (TR-004).
    /// </summary>
    public void Resume()
    {
        if (Status != MissionStatus.Paused)
        {
            throw new DomainException(FlightOperationsErrors.MissionInvalidStatusTransition(Status.ToString(), "Resume"));
        }

        Status = MissionStatus.InProgress;

        if (Id.Value > 0)
        {
            AddDomainEvent(new MissionResumedEvent(Id.Value, DateTimeOffset.UtcNow));
        }
    }

    /// <summary>
    ///     Completes mission application, recording final flight log and closing work order (TR-005, TR-006).
    /// </summary>
    public void Complete(
        DateTimeOffset completedAt,
        decimal? treatedAreaHectares = null,
        string? closingNotes = null)
    {
        if (Status != MissionStatus.InProgress && Status != MissionStatus.Paused)
        {
            throw new DomainException(FlightOperationsErrors.MissionInvalidStatusTransition(Status.ToString(), "Complete"));
        }

        CompletedAt = completedAt;
        TreatedAreaHectares = treatedAreaHectares ?? PlannedAreaHectares;

        if (TreatedAreaHectares < 0m || TreatedAreaHectares > MaxAreaHectares)
        {
            throw new DomainException(FlightOperationsErrors.MissionTreatedAreaOutOfRange);
        }

        if (!string.IsNullOrWhiteSpace(closingNotes))
        {
            var combinedNotes = string.IsNullOrWhiteSpace(Notes)
                ? closingNotes.Trim()
                : $"{Notes}\n[Cierre]: {closingNotes.Trim()}";
            Notes = FlightOperationsGuard.OptionalText(combinedNotes, MaxNotesLength, FlightOperationsErrors.MissionNotesTooLong);
        }

        Progress = 100;
        Status = MissionStatus.Completed;

        if (Id.Value > 0)
        {
            AddDomainEvent(new MissionCompletedEvent(
                Id.Value,
                DroneId?.Value,
                TreatedAreaHectares,
                completedAt,
                DateTimeOffset.UtcNow));
        }
    }

    /// <summary>
    ///     Cancels the mission and releases allocated resources (MI-006).
    /// </summary>
    public void Cancel(string? reason = null)
    {
        if (Status == MissionStatus.Completed || Status == MissionStatus.Cancelled)
        {
            throw new DomainException(FlightOperationsErrors.MissionCannotBeCancelled(Status.ToString()));
        }

        if (!string.IsNullOrWhiteSpace(reason))
        {
            var combined = string.IsNullOrWhiteSpace(Notes)
                ? $"[Cancelación]: {reason.Trim()}"
                : $"{Notes}\n[Cancelación]: {reason.Trim()}";
            Notes = FlightOperationsGuard.OptionalText(combined, MaxNotesLength, FlightOperationsErrors.MissionNotesTooLong);
        }

        Status = MissionStatus.Cancelled;

        if (Id.Value > 0)
        {
            AddDomainEvent(new MissionCancelledEvent(Id.Value, DroneId?.Value, reason, DateTimeOffset.UtcNow));
        }
    }

    /// <summary>
    ///     Reschedules mission date and time (MI-006).
    /// </summary>
    public void Reschedule(DateTimeOffset newScheduledAt)
    {
        if (Status == MissionStatus.Completed || Status == MissionStatus.Cancelled)
        {
            throw new DomainException(FlightOperationsErrors.MissionInvalidStatusTransition(Status.ToString(), "Reschedule"));
        }

        ScheduledAt = newScheduledAt;
    }

    public void MarkAsCreated()
    {
        if (_creationEventRaised) return;
        if (Id.Value <= 0)
        {
            throw new InvalidOperationException("The mission must be persisted before raising its creation event.");
        }

        AddDomainEvent(new MissionCreatedEvent(
            Id.Value,
            Code,
            FarmId,
            ParcelId,
            ScheduledAt,
            PlannedAreaHectares,
            DateTimeOffset.UtcNow));

        _creationEventRaised = true;
    }
}
