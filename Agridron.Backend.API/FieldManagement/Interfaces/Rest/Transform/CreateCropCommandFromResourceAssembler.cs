using Agridron.Backend.API.FieldManagement.Domain.Model.Commands;
using Agridron.Backend.API.FieldManagement.Interfaces.Rest.Resources;

namespace Agridron.Backend.API.FieldManagement.Interfaces.Rest.Transform;

/// <summary>
///     Builds <see cref="CreateCropCommand" /> from a request resource.
/// </summary>
public static class CreateCropCommandFromResourceAssembler
{
    public static CreateCropCommand ToCommandFromResource(CreateCropResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        return new CreateCropCommand(resource.Name, resource.Variety);
    }
}
