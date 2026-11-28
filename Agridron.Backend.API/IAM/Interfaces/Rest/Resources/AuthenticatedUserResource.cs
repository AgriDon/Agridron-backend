namespace Agridron.Backend.API.IAM.Interfaces.Rest.Resources;

/// <summary>Response payload returned after successful sign-in.</summary>
public sealed record AuthenticatedUserResource(
    long Id,
    string FullName,
    string Email,
    string Role,
    bool EmailVerified,
    string Token);
