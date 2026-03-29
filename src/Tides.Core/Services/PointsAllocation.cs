using Tides.Core.Domain.Enums;
using Tides.Core.Domain.ValueObjects;

namespace Tides.Core.Services;

public record PointsAllocation(
    Guid EventId,
    AgeGroup AgeGroup,
    List<EntryPoints> EntryPoints);

public record EntryPoints(Guid EntryId, Guid ClubId, Points Points);
