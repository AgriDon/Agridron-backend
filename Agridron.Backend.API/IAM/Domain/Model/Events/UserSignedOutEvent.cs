using Agridron.Backend.API.Shared.Domain.Model.Events;

namespace Agridron.Backend.API.IAM.Domain.Model.Events;

public sealed record UserSignedOutEvent(
    long UserId,
    DateTimeOffset OccurredAt) : IEvent;
