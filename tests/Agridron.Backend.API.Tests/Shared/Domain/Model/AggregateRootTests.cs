using Agridron.Backend.API.Shared.Domain.Model.Aggregates;
using Agridron.Backend.API.Shared.Domain.Model.Events;

namespace Agridron.Backend.API.Tests.Shared.Domain.Model;

public class AggregateRootTests
{
    [Fact]
    public void DomainEvents_ShouldBeEmpty_WhenNothingHappened()
    {
        // Arrange
        var aggregate = new SampleAggregate(Guid.NewGuid());

        // Act & Assert
        aggregate.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void AddDomainEvent_ShouldRecordTheEvent()
    {
        // Arrange
        var aggregate = new SampleAggregate(Guid.NewGuid());

        // Act
        aggregate.DoSomething();

        // Assert
        aggregate.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<SomethingHappened>();
    }

    [Fact]
    public void ClearDomainEvents_ShouldRemoveEveryRecordedEvent()
    {
        // Arrange
        var aggregate = new SampleAggregate(Guid.NewGuid());
        aggregate.DoSomething();
        aggregate.DoSomething();

        // Act
        aggregate.ClearDomainEvents();

        // Assert
        aggregate.DomainEvents.Should().BeEmpty();
    }

    private sealed class SampleAggregate(Guid id) : AggregateRoot<Guid>(id)
    {
        public void DoSomething()
        {
            AddDomainEvent(new SomethingHappened(Id));
        }
    }

    private sealed record SomethingHappened(Guid AggregateId) : IEvent;
}
