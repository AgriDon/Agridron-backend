using System.ComponentModel.DataAnnotations;

namespace Agridron.Backend.API.FieldManagement.Interfaces.Rest.Resources;

/// <summary>
///     Payload used to define a fumigation area.
/// </summary>
public sealed class CreateFumigationAreaResource
{
    /// <summary>The parcel the area belongs to.</summary>
    [Range(1, long.MaxValue)]
    public long ParcelId { get; init; }

    /// <summary>The GeoJSON polygon. The area is derived from it.</summary>
    [Required]
    public string Geometry { get; init; } = string.Empty;
}
