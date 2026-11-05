using System.ComponentModel.DataAnnotations;
using Agridron.Backend.API.FieldManagement.Domain.Model.Aggregates;

namespace Agridron.Backend.API.FieldManagement.Interfaces.Rest.Resources;

/// <summary>
///     Payload used to register a farm.
/// </summary>
public sealed class CreateFarmResource
{
    /// <summary>The farm name.</summary>
    [Required]
    [MaxLength(Farm.MaxNameLength)]
    public string Name { get; init; } = string.Empty;

    /// <summary>The farm location.</summary>
    [Required]
    [MaxLength(Farm.MaxLocationLength)]
    public string Location { get; init; } = string.Empty;

    /// <summary>The identifier of the owner, from the IAM bounded context.</summary>
    [Range(1, long.MaxValue)]
    public long OwnerId { get; init; }

    /// <summary>An optional absolute http or https image URL.</summary>
    [Url]
    [MaxLength(Farm.MaxImageUrlLength)]
    public string? ImageUrl { get; init; }
}
