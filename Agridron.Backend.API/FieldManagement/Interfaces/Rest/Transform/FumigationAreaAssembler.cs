using Agridron.Backend.API.FieldManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.FieldManagement.Interfaces.Rest.Resources;

namespace Agridron.Backend.API.FieldManagement.Interfaces.Rest.Transform;

/// <summary>
///     Maps <see cref="FumigationArea" /> aggregates to API resources.
/// </summary>
internal static class FumigationAreaAssembler
{
    /// <summary>Builds the resource for a fumigation area.</summary>
    public static FumigationAreaResource ToResourceFrom(FumigationArea area)
    {
        ArgumentNullException.ThrowIfNull(area);

        return new FumigationAreaResource(
            area.Id.Value,
            area.ParcelId.Value,
            area.AreaHectares,
            area.Geometry.GeoJson);
    }
}
