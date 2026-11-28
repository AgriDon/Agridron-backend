using System.ComponentModel.DataAnnotations;
using Agridron.Backend.API.IAM.Domain.Model.Aggregates;

namespace Agridron.Backend.API.IAM.Interfaces.Rest.Resources;

/// <summary>
///     Request DTO used to register a new user account (AU-001).
/// </summary>
/// <remarks>
///     Design Pattern: DTO / Resource — validates wire-format constraints at the HTTP edge using DataAnnotations.
/// </remarks>
public sealed class SignUpResource
{
    /// <summary>Gets the user's full name.</summary>
    [Required]
    [MaxLength(User.MaxFullNameLength)]
    public string FullName { get; init; } = string.Empty;

    /// <summary>Gets the user's email address.</summary>
    [Required]
    [EmailAddress]
    [MaxLength(User.MaxEmailLength)]
    public string Email { get; init; } = string.Empty;

    /// <summary>Gets the raw password (minimum 8 characters).</summary>
    [Required]
    [MinLength(User.MinPasswordLength)]
    [MaxLength(User.MaxPasswordLength)]
    public string Password { get; init; } = string.Empty;

    /// <summary>Gets the optional contact phone number.</summary>
    [MaxLength(User.MaxPhoneNumberLength)]
    public string? PhoneNumber { get; init; }

    /// <summary>Gets the optional role name (defaults to <c>Farmer</c> when omitted).</summary>
    [MaxLength(30)]
    public string? Role { get; init; }
}
