namespace Agridron.Backend.API.IAM.Application.Internal.OutboundServices;

/// <summary>Contract for sending IAM transactional emails.</summary>
public interface IBrevoEmailService
{
    /// <summary>Sends a welcome and email-verification message to a newly registered user.</summary>
    Task SendWelcomeEmailAsync(
        string toEmail,
        string fullName,
        string role,
        string verificationToken,
        CancellationToken cancellationToken = default);
}
