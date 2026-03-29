namespace Tides.Core.Events;

public record ResultEntered(
    Guid CarnivalId,
    Guid EventId,
    Guid RoundId,
    Guid HeatId,
    Guid ResultId,
    DateTime OccurredAt) : IDomainEvent;
