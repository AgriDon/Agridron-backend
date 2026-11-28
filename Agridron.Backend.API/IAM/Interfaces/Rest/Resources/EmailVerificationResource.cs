namespace Agridron.Backend.API.IAM.Interfaces.Rest.Resources;

/// <summary>
///     Response DTO returned after a user's email address is successfully verified.
/// </summary>
/// <remarks>
///     Design Pattern: DTO / Resource — provides a structured confirmation payload for email verification requests.
/// </remarks>
/// <param name="UserId">The verified user's database identifier.</param>
/// <param name="Email">The verified normalized email address.</param>
/// <param name="EmailVerified">Indicates that email verification succeeded (<c>true</c>).</param>
/// <param name="Message">Human-readable confirmation message.</param>
public sealed record EmailVerificationResource(
    long UserId,
    string Email,
    bool EmailVerified,
    string Message);
