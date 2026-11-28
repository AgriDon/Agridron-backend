using System.ComponentModel.DataAnnotations;
using Agridron.Backend.API.IAM.Domain.Model.Aggregates;

namespace Agridron.Backend.API.IAM.Interfaces.Rest.Resources;

/// <summary>
///     Request DTO used to authenticate an existing user (AU-003).
/// </summary>
/// <remarks>
///     Design Pattern: DTO / Resource — validates sign-in request fields at the HTTP edge.
/// </remarks>
public sealed class SignInResource
{
    /// <summary>Gets the registered email address.</summary>
    [Required]
    [EmailAddress]
    [MaxLength(User.MaxEmailLength)]
    public string Email { get; init; } = string.Empty;

    /// <summary>Gets the raw password.</summary>
    [Required]
    [MaxLength(User.MaxPasswordLength)]
    public string Password { get; init; } = string.Empty;
}
