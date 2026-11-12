using Agridron.Backend.API.Shared.Domain.Model.Events;

namespace Agridron.Backend.API.Shared.Domain.Model.Aggregates;

/// <summary>
///     Exposes the domain events recorded by an aggregate root, without depending on its
///     identifier type.
/// </summary>
/// <remarks>
///     The persistence interceptor uses this contract to collect events from every aggregate.
///     A generic base class such as <c>AggregateRoot&lt;TId&gt;</c> cannot be matched directly
///     because generic types are invariant.
/// </remarks>
public interface IHasDomainEvents
{
    /// <summary>Domain events recorded by the aggregate and not published yet.</summary>
    IReadOnlyCollection<IEvent> DomainEvents { get; }

    /// <summary>Clears the recorded domain events. Called after they have been published.</summary>
    void ClearDomainEvents();
}
