using Agridron.Backend.API.FieldManagement.Domain.Model.Commands;
using Agridron.Backend.API.FieldManagement.Interfaces.Rest.Resources;

namespace Agridron.Backend.API.FieldManagement.Interfaces.Rest.Transform;

/// <summary>
///     Builds <see cref="UpdateFumigationAreaCommand" /> from a request resource.
/// </summary>
public static class UpdateFumigationAreaCommandFromResourceAssembler
{
    public static UpdateFumigationAreaCommand ToCommandFromResource(long id, UpdateFumigationAreaResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        return new UpdateFumigationAreaCommand(id, resource.ParcelId, resource.Geometry);
    }
}
