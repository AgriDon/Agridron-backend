using Agridron.Backend.API.FieldManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.FieldManagement.Interfaces.Rest.Resources;

namespace Agridron.Backend.API.FieldManagement.Interfaces.Rest.Transform;

/// <summary>
///     Maps <see cref="Farm" /> aggregates to API resources.
/// </summary>
internal static class FarmAssembler
{
    /// <summary>Builds the resource for a farm.</summary>
    public static FarmResource ToResourceFrom(Farm farm)
    {
        ArgumentNullException.ThrowIfNull(farm);

        return new FarmResource(
            farm.Id.Value,
            farm.Name,
            farm.Location,
            farm.OwnerId,
            farm.ImageUrl);
    }
}
