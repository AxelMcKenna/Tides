namespace Tides.Core.Events;

public interface IDomainEvent
{
    DateTime OccurredAt { get; }
}
