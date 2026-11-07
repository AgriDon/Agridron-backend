using Agridron.Backend.API.FlightOperations.Domain.Model.Errors;
using Agridron.Backend.API.FlightOperations.Domain.Model.Events;
using Agridron.Backend.API.FlightOperations.Domain.Model.Validation;
using Agridron.Backend.API.FlightOperations.Domain.Model.ValueObjects;
using Agridron.Backend.API.Shared.Domain.Model.Aggregates;
using Agridron.Backend.API.Shared.Domain.Model.Entities;

namespace Agridron.Backend.API.FlightOperations.Domain.Model.Aggregates;

/// <summary>
///     Aggregate representing a drone telemetry reading and operation state snapshot (TR-001, TR-002).
/// </summary>
public sealed class OperationStatus : AggregateRoot<OperationStatusId>, IAuditableEntity
{
    public const int MaxStateDescriptionLength = 60;

    private bool _creationEventRaised;

    private OperationStatus()
    {
    }

    public OperationStatus(
        GeoCoordinate location,
        string status,
        DateTimeOffset timestamp,
        MissionId? missionId = null,
        DroneId? droneId = null,
        decimal? altitudeMeters = null,
        decimal? speedMetersPerSecond = null,
        decimal? batteryLevel = null,
        decimal? tankVolumeLiters = null) : base(OperationStatusId.Empty)
    {
        ArgumentNullException.ThrowIfNull(location);
        Location = location;
        SetStatus(status);
        Timestamp = timestamp;
        MissionId = missionId;
        DroneId = droneId;
        AltitudeMeters = altitudeMeters;
        SpeedMetersPerSecond = speedMetersPerSecond;
        BatteryLevel = batteryLevel.HasValue ? Math.Clamp(batteryLevel.Value, 0m, 100m) : null;
        TankVolumeLiters = tankVolumeLiters.HasValue ? Math.Max(0m, tankVolumeLiters.Value) : null;
    }

    public MissionId? MissionId { get; private set; }
    public DroneId? DroneId { get; private set; }
    public GeoCoordinate Location { get; private set; } = null!;
    public decimal? AltitudeMeters { get; private set; }
    public decimal? SpeedMetersPerSecond { get; private set; }
    public decimal? BatteryLevel { get; private set; }
    public decimal? TankVolumeLiters { get; private set; }
    public string Status { get; private set; } = string.Empty;
    public DateTimeOffset Timestamp { get; private set; }

    // IAuditableEntity
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public void SetStatus(string status)
    {
        Status = FlightOperationsGuard.RequiredText(
            status,
            FlightOperationsErrors.OperationStatusStateRequired,
            MaxStateDescriptionLength,
            FlightOperationsErrors.OperationStatusStateTooLong);
    }

    public void UpdateLocation(GeoCoordinate location, string status)
    {
        ArgumentNullException.ThrowIfNull(location);
        Location = location;
        SetStatus(status);
        Timestamp = DateTimeOffset.UtcNow;
    }

    public void MarkAsCreated()
    {
        if (_creationEventRaised) return;
        if (Id.Value <= 0)
        {
            throw new InvalidOperationException("The operation status must be persisted before raising its creation event.");
        }

        AddDomainEvent(new OperationStatusRecordedEvent(
            Id.Value,
            MissionId?.Value,
            DroneId?.Value,
            Location.Latitude,
            Location.Longitude,
            Status,
            Timestamp));

        _creationEventRaised = true;
    }
}
