using Agridron.Backend.API.FlightOperations.Domain.Model.Errors;
using Agridron.Backend.API.FlightOperations.Domain.Model.Validation;
using Agridron.Backend.API.Shared.Domain.Model.Aggregates;
using Agridron.Backend.API.Shared.Domain.Model.Entities;
using Agridron.Backend.API.Shared.Domain.Model.Exceptions;

namespace Agridron.Backend.API.FlightOperations.Domain.Model.Aggregates;
using Agridron.Backend.API.FlightOperations.Domain.Model.ValueObjects;


/// <summary>
///  Aggregate representing one maintenance or repair performed on a drone
/// It is the drone's maintenance history: who did what, when and of which type of maintenance
/// </summary>
public class MaintenanceRecord : AggregateRoot<MaintenanceRecordId>, IAuditableEntity
{

    public const int MaxDescriptionLength = 500;
    public const int MaxTechnicianLength = 120;

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public MaintenanceRecord(){}

    public MaintenanceRecord(
        DroneId droneId,
        DateTimeOffset performedAt,
        MaintenanceType type,
        string technician,
        string? description = null) : base(MaintenanceRecordId.Empty)
    {
        if (!Enum.IsDefined(type))
        {
            throw new DomainException(FlightOperationsErrors.MaintenanceTypeInvalid);
        }

        DroneId = droneId;
        PerformedAt = performedAt;
        Type = type;
        Technician = FlightOperationsGuard.RequiredText(
            technician,
            FlightOperationsErrors.MaintenanceTechnicianRequired,
            MaxTechnicianLength,
            FlightOperationsErrors.MaintenanceTechnicianTooLong);
        Description = FlightOperationsGuard.OptionalText(
            description,
            MaxDescriptionLength,
            FlightOperationsErrors.MaintenanceDescriptionTooLong);
    }

    public DroneId DroneId { get; private set; }
    public DateTimeOffset PerformedAt { get; private set; }
    public MaintenanceType Type { get; private set; } = MaintenanceType.Preventive;
    public string Technician { get; private set; } = string.Empty;
    public string? Description { get; private set; }



}
