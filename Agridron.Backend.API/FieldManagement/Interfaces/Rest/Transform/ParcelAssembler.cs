using Agridron.Backend.API.FieldManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.FieldManagement.Interfaces.Rest.Resources;

namespace Agridron.Backend.API.FieldManagement.Interfaces.Rest.Transform;

/// <summary>
///     Maps <see cref="Parcel" /> aggregates to API resources.
/// </summary>
internal static class ParcelAssembler
{
    /// <summary>Builds the resource for a parcel.</summary>
    public static ParcelResource ToResourceFrom(Parcel parcel)
    {
        ArgumentNullException.ThrowIfNull(parcel);

        return new ParcelResource(
            parcel.Id.Value,
            parcel.FarmId.Value,
            parcel.CropId.Value,
            parcel.Name,
            parcel.AreaHectares,
            parcel.Geometry.GeoJson,
            parcel.ImageUrl);
    }
}
