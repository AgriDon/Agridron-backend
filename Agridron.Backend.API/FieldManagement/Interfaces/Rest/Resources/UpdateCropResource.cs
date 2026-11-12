using System.ComponentModel.DataAnnotations;
using Agridron.Backend.API.FieldManagement.Domain.Model.Aggregates;

namespace Agridron.Backend.API.FieldManagement.Interfaces.Rest.Resources;

/// <summary>
///     Payload used to update a crop.
/// </summary>
public sealed class UpdateCropResource
{
    /// <summary>The crop name.</summary>
    [Required]
    [MaxLength(Crop.MaxNameLength)]
    public string Name { get; init; } = string.Empty;

    /// <summary>The crop variety.</summary>
    [Required]
    [MaxLength(Crop.MaxVarietyLength)]
    public string Variety { get; init; } = string.Empty;
}
