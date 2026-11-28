namespace Agridron.Backend.API.IAM.Domain.Model.Commands;

/// <summary>
///     Command to verify a user's email address using the single-use verification token sent during registration.
/// </summary>
/// <param name="Token">The URL-safe email verification token.</param>
public sealed record VerifyEmailCommand(string Token);
