using Agridron.Backend.API.FieldManagement.Domain.Model.Commands;
using Agridron.Backend.API.FieldManagement.Interfaces.Rest.Resources;

namespace Agridron.Backend.API.FieldManagement.Interfaces.Rest.Transform;

/// <summary>
///     Builds <see cref="CreateParcelCommand" /> from a request resource.
/// </summary>
public static class CreateParcelCommandFromResourceAssembler
{
    public static CreateParcelCommand ToCommandFromResource(CreateParcelResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        return new CreateParcelCommand(
            resource.FarmId,
            resource.CropId,
            resource.Name,
            resource.Geometry,
            resource.PhenologicalStage,
            resource.EffectiveImageUrl);
    }
}
