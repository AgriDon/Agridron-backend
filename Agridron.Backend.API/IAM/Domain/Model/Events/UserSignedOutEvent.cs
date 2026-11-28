using Agridron.Backend.API.Shared.Domain.Model.Events;

namespace Agridron.Backend.API.IAM.Domain.Model.Events;

/// <summary>
///     Domain event raised when an authenticated user explicitly signs out.
/// </summary>
/// <param name="UserId">The signed-out user identifier.</param>
/// <param name="OccurredAt">The UTC timestamp when the sign-out happened.</param>
public sealed record UserSignedOutEvent(
    long UserId,
    DateTimeOffset OccurredAt) : IEvent;
