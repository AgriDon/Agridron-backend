namespace Agridron.Backend.API.IAM.Interfaces.Rest.Resources;

/// <summary>Public user profile response payload.</summary>
public sealed record UserResource(
    long Id,
    string FullName,
    string Email,
    string Role,
    string? PhoneNumber,
    string Status,
    bool EmailVerified,
    DateTimeOffset? CreatedAt);
