using Tides.Core.Domain;
using Tides.Core.Domain.Enums;

namespace Tides.Core.Services;

public interface IPointsCalculatorService
{
    PointsAllocation CalculatePoints(
        Guid eventId,
        AgeGroup ageGroup,
        List<Result> results,
        Dictionary<Guid, Guid> entryToClub,
        PointsTable table);

    ClubPointsLedger AggregateClubPoints(List<PointsAllocation> allocations);

    ClubPointsLedger RecalculateOnCorrection(
        ClubPointsLedger current,
        PointsAllocation oldAllocation,
        PointsAllocation newAllocation);
}
