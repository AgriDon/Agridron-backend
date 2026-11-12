using Agridron.Backend.API.FieldManagement.Domain.Model.Commands;
using Agridron.Backend.API.FieldManagement.Interfaces.Rest.Resources;

namespace Agridron.Backend.API.FieldManagement.Interfaces.Rest.Transform;

/// <summary>
///     Builds <see cref="CreateFumigationAreaCommand" /> from a request resource.
/// </summary>
public static class CreateFumigationAreaCommandFromResourceAssembler
{
    public static CreateFumigationAreaCommand ToCommandFromResource(CreateFumigationAreaResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        return new CreateFumigationAreaCommand(resource.ParcelId, resource.Geometry);
    }
}
