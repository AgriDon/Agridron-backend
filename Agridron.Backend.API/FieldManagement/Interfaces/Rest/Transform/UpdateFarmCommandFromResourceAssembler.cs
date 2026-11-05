using Agridron.Backend.API.FieldManagement.Domain.Model.Commands;
using Agridron.Backend.API.FieldManagement.Interfaces.Rest.Resources;

namespace Agridron.Backend.API.FieldManagement.Interfaces.Rest.Transform;

/// <summary>
///     Builds <see cref="UpdateFarmCommand" /> from a request resource.
/// </summary>
public static class UpdateFarmCommandFromResourceAssembler
{
    public static UpdateFarmCommand ToCommandFromResource(long id, UpdateFarmResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        return new UpdateFarmCommand(id, resource.Name, resource.Location, resource.OwnerId, resource.ImageUrl);
    }
}
