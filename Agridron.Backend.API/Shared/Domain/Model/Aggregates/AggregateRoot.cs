using Agridron.Backend.API.Shared.Domain.Model.Entities;
using Agridron.Backend.API.Shared.Domain.Model.Events;

namespace Agridron.Backend.API.Shared.Domain.Model.Aggregates;

/// <summary>
///     Base class for aggregate roots.
/// </summary>
/// <remarks>
///     An aggregate root is the only entry point to a consistency boundary. It records the domain
///     events that happened inside the boundary so the application layer can publish them once the
///     transaction has been committed.
/// </remarks>
/// <typeparam name="TId">The type of the aggregate identifier.</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    private readonly List<IEvent> _domainEvents = [];

    /// <summary>
    ///     Creates an aggregate root with the given identity.
    /// </summary>
    /// <param name="id">The aggregate identifier.</param>
    protected AggregateRoot(TId id) : base(id)
    {
    }

    /// <summary>
    ///     Parameterless constructor required by Entity Framework Core materialization.
    /// </summary>
    protected AggregateRoot()
    {
    }

    /// <summary>
    ///     Domain events recorded by this aggregate and not published yet.
    /// </summary>
    public IReadOnlyCollection<IEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    ///     Records a domain event inside the aggregate boundary.
    /// </summary>
    /// <param name="domainEvent">The event to record.</param>
    protected void AddDomainEvent(IEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    ///     Clears the recorded domain events. Called after they have been published.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
