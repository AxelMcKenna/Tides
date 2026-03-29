using Tides.Core.Domain;
using Tides.Core.Domain.Enums;
using Tides.Core.Domain.ValueObjects;

namespace Tides.Core.Services;

public class PointsCalculatorService : IPointsCalculatorService
{
    public PointsAllocation CalculatePoints(
        Guid eventId,
        AgeGroup ageGroup,
        List<Result> results,
        Dictionary<Guid, Guid> entryToClub,
        PointsTable table)
    {
        var eligible = results
            .Where(r => r.Status is not ResultStatus.Disqualified
                and not ResultStatus.DidNotStart
                and not ResultStatus.DidNotFinish)
            .OrderBy(r => r.Placing?.Position ?? int.MaxValue)
            .ThenBy(r => r.Time?.Time ?? TimeSpan.MaxValue)
            .ToList();

        var entryPoints = new List<EntryPoints>();
        var i = 0;

        while (i < eligible.Count)
        {
            var currentPlacing = eligible[i].Placing?.Position ?? (i + 1);

            // Count ties at this placing
            var tiedCount = 1;
            while (i + tiedCount < eligible.Count &&
                   (eligible[i + tiedCount].Placing?.Position ?? (i + tiedCount + 1)) == currentPlacing)
            {
                tiedCount++;
            }

            var points = table.GetPointsForTiedPlacing(currentPlacing, tiedCount);

            for (var j = 0; j < tiedCount; j++)
            {
                var result = eligible[i + j];
                var clubId = entryToClub.GetValueOrDefault(result.EntryId);
                entryPoints.Add(new EntryPoints(result.EntryId, clubId, new Points(points)));
            }

            i += tiedCount;
        }

        // DQ/DNS/DNF entries get zero points
        foreach (var result in results.Where(r =>
            r.Status is ResultStatus.Disqualified or ResultStatus.DidNotStart or ResultStatus.DidNotFinish))
        {
            var clubId = entryToClub.GetValueOrDefault(result.EntryId);
            entryPoints.Add(new EntryPoints(result.EntryId, clubId, Points.Zero));
        }

        return new PointsAllocation(eventId, ageGroup, entryPoints);
    }

    public ClubPointsLedger AggregateClubPoints(List<PointsAllocation> allocations)
    {
        var ledger = new ClubPointsLedger();

        foreach (var allocation in allocations)
        {
            foreach (var ep in allocation.EntryPoints)
            {
                if (ep.Points.Value > 0)
                    ledger.Credit(ep.ClubId, allocation.AgeGroup, ep.Points);
            }
        }

        return ledger;
    }

    public ClubPointsLedger RecalculateOnCorrection(
        ClubPointsLedger current,
        PointsAllocation oldAllocation,
        PointsAllocation newAllocation)
    {
        // Debit old points
        foreach (var ep in oldAllocation.EntryPoints)
        {
            if (ep.Points.Value > 0)
                current.Debit(ep.ClubId, oldAllocation.AgeGroup, ep.Points);
        }

        // Credit new points
        foreach (var ep in newAllocation.EntryPoints)
        {
            if (ep.Points.Value > 0)
                current.Credit(ep.ClubId, newAllocation.AgeGroup, ep.Points);
        }

        return current;
    }
}
