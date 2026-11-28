using System.ComponentModel.DataAnnotations;
using Agridron.Backend.API.IAM.Domain.Model.Aggregates;

namespace Agridron.Backend.API.IAM.Interfaces.Rest.Resources;

/// <summary>
///     Request DTO used to update basic profile information for a user (AU-005).
/// </summary>
/// <remarks>
///     Design Pattern: DTO / Resource — exposes only mutable profile fields allowed on <c>PATCH /api/v1/users/{id}</c>.
/// </remarks>
public sealed class UpdateUserResource
{
    /// <summary>Gets the updated full name.</summary>
    [Required]
    [MaxLength(User.MaxFullNameLength)]
    public string FullName { get; init; } = string.Empty;

    /// <summary>Gets the updated optional contact phone number.</summary>
    [MaxLength(User.MaxPhoneNumberLength)]
    public string? PhoneNumber { get; init; }
}
