namespace Agridron.Backend.API.IAM.Application.Internal.OutboundServices;

/// <summary>
///     Outbound service contract for sending IAM transactional emails.
/// </summary>
/// <remarks>
///     Design Pattern: Strategy — abstracts the transactional email delivery mechanism (Brevo REST API v3)
///     behind an application port so providers can be swapped or mocked cleanly in unit tests.
/// </remarks>
public interface IBrevoEmailService
{
    /// <summary>
    ///     Sends a welcome and email-verification message to a newly registered user.
    /// </summary>
    /// <param name="toEmail">The recipient's normalized email address.</param>
    /// <param name="fullName">The recipient's full name.</param>
    /// <param name="role">The user's assigned role name.</param>
    /// <param name="verificationToken">The single-use URL-safe email verification token.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous send operation.</returns>
    Task SendWelcomeEmailAsync(
        string toEmail,
        string fullName,
        string role,
        string verificationToken,
        CancellationToken cancellationToken = default);
}
