using System.ComponentModel.DataAnnotations;
using Agridron.Backend.API.IAM.Domain.Model.Aggregates;

namespace Agridron.Backend.API.IAM.Interfaces.Rest.Resources;

/// <summary>Request payload for user registration.</summary>
public sealed class SignUpResource
{
    [Required]
    [MaxLength(User.MaxFullNameLength)]
    public string FullName { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(User.MaxEmailLength)]
    public string Email { get; init; } = string.Empty;

    [Required]
    [MinLength(User.MinPasswordLength)]
    [MaxLength(User.MaxPasswordLength)]
    public string Password { get; init; } = string.Empty;

    [MaxLength(User.MaxPhoneNumberLength)]
    public string? PhoneNumber { get; init; }

    [MaxLength(30)]
    public string? Role { get; init; }
}
