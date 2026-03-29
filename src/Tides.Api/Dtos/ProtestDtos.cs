namespace Tides.Api.Dtos;

public record LodgeProtestRequest(
    Guid EventId,
    Guid? HeatId,
    Guid LodgedByClubId,
    string Reason);

public record AdjudicateProtestRequest(
    string Outcome,
    string Reason);

public record ProtestResponse(
    Guid Id,
    Guid CarnivalId,
    Guid EventId,
    Guid? HeatId,
    Guid LodgedByClubId,
    string Reason,
    string Status,
    string? AdjudicationReason,
    DateTime LodgedAt,
    DateTime? AdjudicatedAt);
