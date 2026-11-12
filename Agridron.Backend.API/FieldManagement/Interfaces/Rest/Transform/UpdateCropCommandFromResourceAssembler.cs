using Agridron.Backend.API.FieldManagement.Domain.Model.Commands;
using Agridron.Backend.API.FieldManagement.Interfaces.Rest.Resources;

namespace Agridron.Backend.API.FieldManagement.Interfaces.Rest.Transform;

/// <summary>
///     Builds <see cref="UpdateCropCommand" /> from a request resource.
/// </summary>
public static class UpdateCropCommandFromResourceAssembler
{
    public static UpdateCropCommand ToCommandFromResource(long id, UpdateCropResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        return new UpdateCropCommand(id, resource.Name, resource.Variety);
    }
}
