namespace Agridron.Backend.API.IAM.Domain.Model.Commands;

/// <summary>
///     Command to authenticate a user and issue a signed JWT token (AU-003).
/// </summary>
/// <param name="Email">The registered email address.</param>
/// <param name="Password">The raw password to verify.</param>
public sealed record SignInCommand(
    string Email,
    string Password);
