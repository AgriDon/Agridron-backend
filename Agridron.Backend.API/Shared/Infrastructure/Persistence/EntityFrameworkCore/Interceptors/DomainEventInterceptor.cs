using System.Diagnostics;
using Agridron.Backend.API.Shared.Domain.Model.Aggregates;
using Agridron.Backend.API.Shared.Domain.Model.Events;
using Cortex.Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Agridron.Backend.API.Shared.Infrastructure.Persistence.EntityFrameworkCore.Interceptors;

/// <summary>
///     Intercepts <see cref="DbContext.SaveChangesAsync" /> to publish domain events after the
///     transaction commits successfully.
/// </summary>
internal sealed class DomainEventInterceptor : SaveChangesInterceptor
{
    private readonly IMediator _mediator;
    private readonly ILogger<DomainEventInterceptor> _logger;

    public DomainEventInterceptor(IMediator mediator, ILogger<DomainEventInterceptor> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context is null)
        {
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        var aggregatesWithEvents = context.ChangeTracker
            .Entries()
            .Select(entry => entry.Entity)
            .OfType<IHasDomainEvents>()
            .Where(aggregate => aggregate.DomainEvents.Count > 0)
            .ToList();

        if (aggregatesWithEvents.Count == 0)
        {
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        var eventsToPublish = aggregatesWithEvents
            .SelectMany(a => a.DomainEvents)
            .ToList();

        _logger.LogDebug("Collected {Count} domain events from {AggregateCount} aggregates before commit",
            eventsToPublish.Count, aggregatesWithEvents.Count);

        var saveResult = await base.SavingChangesAsync(eventData, result, cancellationToken);

        // Creation events are raised after the insert, when the database identity is known, so a
        // second SaveChanges may have nothing to persist but still has events to publish.
        foreach (var domainEvent in eventsToPublish)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                await _mediator.PublishAsync(domainEvent, cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception,
                    "Failed to publish domain event {EventType}. The transaction has already committed.",
                    domainEvent.GetType().Name);
            }
            finally
            {
                stopwatch.Stop();
                _logger.LogDebug("Published domain event {EventType} in {ElapsedMs} ms",
                    domainEvent.GetType().Name, stopwatch.ElapsedMilliseconds);
            }
        }

        foreach (var aggregate in aggregatesWithEvents)
        {
            aggregate.ClearDomainEvents();
        }

        return saveResult;
    }
}
