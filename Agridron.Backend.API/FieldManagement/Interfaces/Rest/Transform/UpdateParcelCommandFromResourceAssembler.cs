using Agridron.Backend.API.FieldManagement.Domain.Model.Commands;
using Agridron.Backend.API.FieldManagement.Interfaces.Rest.Resources;

namespace Agridron.Backend.API.FieldManagement.Interfaces.Rest.Transform;

/// <summary>
///     Builds <see cref="UpdateParcelCommand" /> from a request resource.
/// </summary>
public static class UpdateParcelCommandFromResourceAssembler
{
    public static UpdateParcelCommand ToCommandFromResource(long id, UpdateParcelResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        return new UpdateParcelCommand(
            id,
            resource.FarmId,
            resource.CropId,
            resource.Name,
            resource.Geometry,
            resource.PhenologicalStage,
            resource.EffectiveImageUrl);
    }
}
