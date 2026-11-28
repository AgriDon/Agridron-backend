using System.ComponentModel.DataAnnotations;
using Agridron.Backend.API.IAM.Domain.Model.Aggregates;

namespace Agridron.Backend.API.IAM.Interfaces.Rest.Resources;

/// <summary>Request payload for user sign-in.</summary>
public sealed class SignInResource
{
    [Required]
    [EmailAddress]
    [MaxLength(User.MaxEmailLength)]
    public string Email { get; init; } = string.Empty;

    [Required]
    [MaxLength(User.MaxPasswordLength)]
    public string Password { get; init; } = string.Empty;
}
