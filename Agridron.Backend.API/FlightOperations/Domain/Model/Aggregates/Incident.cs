using Agridron.Backend.API.FlightOperations.Domain.Model.Errors;
using Agridron.Backend.API.FlightOperations.Domain.Model.Events;
using Agridron.Backend.API.FlightOperations.Domain.Model.Validation;
using Agridron.Backend.API.FlightOperations.Domain.Model.ValueObjects;
using Agridron.Backend.API.Shared.Domain.Model.Aggregates;
using Agridron.Backend.API.Shared.Domain.Model.Entities;

namespace Agridron.Backend.API.FlightOperations.Domain.Model.Aggregates;

/// <summary>
///     Aggregate representing a flight or equipment anomaly/incident.
/// </summary>
public sealed class Incident : AggregateRoot<IncidentId>, IAuditableEntity
{
    public const int MaxTypeLength = 60;
    public const int MaxDescriptionLength = 500;

    private bool _creationEventRaised;

    private Incident()
    {
    }

    public Incident(
        string type,
        string description,
        DateTimeOffset occurredAt,
        MissionId? missionId = null,
        DroneId? droneId = null,
        IncidentSeverity severity = IncidentSeverity.Medium) : base(IncidentId.Empty)
    {
        SetType(type);
        SetDescription(description);
        OccurredAt = occurredAt;
        MissionId = missionId;
        DroneId = droneId;
        Severity = severity;
    }

    public string Type { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; private set; }
    public MissionId? MissionId { get; private set; }
    public DroneId? DroneId { get; private set; }
    public IncidentSeverity Severity { get; private set; } = IncidentSeverity.Medium;

    // IAuditableEntity
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public void SetType(string type)
    {
        Type = FlightOperationsGuard.RequiredText(
            type,
            FlightOperationsErrors.IncidentTypeRequired,
            MaxTypeLength,
            FlightOperationsErrors.IncidentTypeTooLong);
    }

    public void SetDescription(string description)
    {
        Description = FlightOperationsGuard.RequiredText(
            description,
            FlightOperationsErrors.IncidentDescriptionRequired,
            MaxDescriptionLength,
            FlightOperationsErrors.IncidentDescriptionTooLong);
    }

    public void Update(string type, string description, IncidentSeverity severity, DateTimeOffset occurredAt)
    {
        SetType(type);
        SetDescription(description);
        Severity = severity;
        OccurredAt = occurredAt;
    }

    public void MarkAsCreated()
    {
        if (_creationEventRaised) return;
        if (Id.Value <= 0)
        {
            throw new InvalidOperationException("The incident must be persisted before raising its creation event.");
        }

        AddDomainEvent(new IncidentReportedEvent(
            Id.Value,
            MissionId?.Value,
            DroneId?.Value,
            Type,
            Severity.ToString(),
            Description,
            OccurredAt));

        _creationEventRaised = true;
    }
}
