namespace Tides.Core.Events;

public record ResultCorrected(
    Guid CarnivalId,
    Guid EventId,
    Guid HeatId,
    Guid ResultId,
    string Reason,
    DateTime OccurredAt) : IDomainEvent;
