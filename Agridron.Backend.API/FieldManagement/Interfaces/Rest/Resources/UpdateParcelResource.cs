using System.ComponentModel.DataAnnotations;
using Agridron.Backend.API.FieldManagement.Domain.Model.Aggregates;

namespace Agridron.Backend.API.FieldManagement.Interfaces.Rest.Resources;

/// <summary>
///     Payload used to replace a parcel.
/// </summary>
public sealed class UpdateParcelResource
{
    /// <summary>The farm the parcel belongs to.</summary>
    [Range(1, long.MaxValue)]
    public long FarmId { get; init; }

    /// <summary>The crop grown in the parcel.</summary>
    [Range(1, long.MaxValue)]
    public long CropId { get; init; }

    /// <summary>The parcel name.</summary>
    [Required]
    [MaxLength(Parcel.MaxNameLength)]
    public string Name { get; init; } = string.Empty;

    /// <summary>The GeoJSON polygon. The area is derived from it.</summary>
    [Required]
    public string Geometry { get; init; } = string.Empty;

    /// <summary>The optional phenological stage of the crop.</summary>
    [MaxLength(Parcel.MaxPhenologicalStageLength)]
    public string? PhenologicalStage { get; init; }

    /// <summary>An optional absolute http or https image URL.</summary>
    [Url]
    [MaxLength(Parcel.MaxImageUrlLength)]
    public string? ImageUrl { get; init; }

    /// <summary>Alias for ImageUrl matching frontend conventions.</summary>
    [Url]
    [MaxLength(Parcel.MaxImageUrlLength)]
    public string? Image { get; init; }

    public string? EffectiveImageUrl => !string.IsNullOrWhiteSpace(ImageUrl) ? ImageUrl : Image;
}
