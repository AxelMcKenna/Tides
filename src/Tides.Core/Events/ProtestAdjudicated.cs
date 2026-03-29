using Tides.Core.Domain.Enums;

namespace Tides.Core.Events;

public record ProtestAdjudicated(
    Guid CarnivalId,
    Guid ProtestId,
    ProtestStatus Outcome,
    string Reason,
    DateTime OccurredAt) : IDomainEvent;
