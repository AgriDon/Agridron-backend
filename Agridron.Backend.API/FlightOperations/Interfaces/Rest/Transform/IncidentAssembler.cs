using Agridron.Backend.API.FlightOperations.Domain.Model.Aggregates;
using Agridron.Backend.API.FlightOperations.Domain.Model.Commands;
using Agridron.Backend.API.FlightOperations.Interfaces.Rest.Resources;

namespace Agridron.Backend.API.FlightOperations.Interfaces.Rest.Transform;

public static class IncidentAssembler
{
    public static IncidentResource ToResourceFrom(Incident entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new IncidentResource(
            entity.Id.Value,
            entity.Type,
            entity.Description,
            entity.OccurredAt,
            entity.MissionId?.Value,
            entity.DroneId?.Value,
            entity.Severity.ToString(),
            entity.CreatedAt,
            entity.UpdatedAt);
    }

    public static CreateIncidentCommand ToCommandFrom(CreateIncidentResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        return new CreateIncidentCommand(
            resource.Type,
            resource.Description,
            resource.OccurredAt,
            resource.MissionId,
            resource.DroneId,
            resource.Severity);
    }

    public static UpdateIncidentCommand ToCommandFrom(long id, UpdateIncidentResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        return new UpdateIncidentCommand(
            id,
            resource.Type,
            resource.Description,
            resource.Severity,
            resource.OccurredAt);
    }
}
