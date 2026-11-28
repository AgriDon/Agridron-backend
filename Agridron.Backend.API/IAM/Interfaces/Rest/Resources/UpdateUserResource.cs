using System.ComponentModel.DataAnnotations;
using Agridron.Backend.API.IAM.Domain.Model.Aggregates;

namespace Agridron.Backend.API.IAM.Interfaces.Rest.Resources;

/// <summary>Request payload for updating user profile fields.</summary>
public sealed class UpdateUserResource
{
    [Required]
    [MaxLength(User.MaxFullNameLength)]
    public string FullName { get; init; } = string.Empty;

    [MaxLength(User.MaxPhoneNumberLength)]
    public string? PhoneNumber { get; init; }
}
