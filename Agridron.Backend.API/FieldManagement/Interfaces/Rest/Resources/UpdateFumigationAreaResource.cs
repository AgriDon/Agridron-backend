using System.ComponentModel.DataAnnotations;

namespace Agridron.Backend.API.FieldManagement.Interfaces.Rest.Resources;

/// <summary>
///     Payload used to redefine a fumigation area.
/// </summary>
public sealed class UpdateFumigationAreaResource
{
    /// <summary>The parcel the area belongs to.</summary>
    [Range(1, long.MaxValue)]
    public long ParcelId { get; init; }

    /// <summary>The new GeoJSON polygon. The area is derived from it.</summary>
    [Required]
    public string Geometry { get; init; } = string.Empty;
}
