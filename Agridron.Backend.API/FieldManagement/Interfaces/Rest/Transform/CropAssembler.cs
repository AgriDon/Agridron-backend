using Agridron.Backend.API.FieldManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.FieldManagement.Interfaces.Rest.Resources;

namespace Agridron.Backend.API.FieldManagement.Interfaces.Rest.Transform;

/// <summary>
///     Maps <see cref="Crop" /> aggregates to API resources.
/// </summary>
internal static class CropAssembler
{
    /// <summary>Builds the resource for a crop.</summary>
    public static CropResource ToResourceFrom(Crop crop)
    {
        ArgumentNullException.ThrowIfNull(crop);

        return new CropResource(crop.Id.Value, crop.Name, crop.Variety);
    }
}
