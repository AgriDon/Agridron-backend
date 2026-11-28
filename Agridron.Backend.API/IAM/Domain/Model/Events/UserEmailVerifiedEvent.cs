using Agridron.Backend.API.Shared.Domain.Model.Events;

namespace Agridron.Backend.API.IAM.Domain.Model.Events;

/// <summary>
///     Domain event raised when a user verifies ownership of their email address.
/// </summary>
/// <param name="UserId">The identifier of the verified user.</param>
/// <param name="Email">The verified normalized email address.</param>
/// <param name="OccurredAt">The UTC timestamp when the verification completed.</param>
public sealed record UserEmailVerifiedEvent(
    long UserId,
    string Email,
    DateTimeOffset OccurredAt) : IEvent;
