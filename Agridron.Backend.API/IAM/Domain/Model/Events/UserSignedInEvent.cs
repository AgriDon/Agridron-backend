using Agridron.Backend.API.Shared.Domain.Model.Events;

namespace Agridron.Backend.API.IAM.Domain.Model.Events;

/// <summary>
///     Domain event raised when a user successfully authenticates.
/// </summary>
/// <param name="UserId">The authenticated user identifier.</param>
/// <param name="Email">The authenticated user's email address.</param>
/// <param name="OccurredAt">The UTC timestamp when the sign-in happened.</param>
public sealed record UserSignedInEvent(
    long UserId,
    string Email,
    DateTimeOffset OccurredAt) : IEvent;
