namespace Agridron.Backend.API.IAM.Interfaces.Rest.Resources;

/// <summary>Response payload confirming email verification.</summary>
public sealed record EmailVerificationResource(
    long UserId,
    string Email,
    bool EmailVerified,
    string Message);
