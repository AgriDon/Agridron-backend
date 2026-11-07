using Agridron.Backend.API.FieldManagement.Domain.Model.Commands;
using Agridron.Backend.API.FieldManagement.Interfaces.Rest.Resources;

namespace Agridron.Backend.API.FieldManagement.Interfaces.Rest.Transform;

/// <summary>
///     Builds <see cref="CreateFarmCommand" /> from a request resource.
/// </summary>
public static class CreateFarmCommandFromResourceAssembler
{
    public static CreateFarmCommand ToCommandFromResource(CreateFarmResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        return new CreateFarmCommand(resource.Name, resource.Location, resource.OwnerId, resource.EffectiveImageUrl);
    }
}
