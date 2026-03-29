using Tides.Core.Domain;
using Tides.Core.Domain.Enums;
using Tides.Core.Domain.ValueObjects;
using Tides.Core.Services;

namespace Tides.Core.Tests;

public class PointsCalculatorServiceTests
{
    private readonly PointsCalculatorService _service = new();

    private static PointsTable CreateStandardTable(bool fractionalTies = true)
    {
        return new PointsTable(Guid.NewGuid(), "Standard", [
            new PointsTableEntry(1, 8),
            new PointsTableEntry(2, 6),
            new PointsTableEntry(3, 4),
            new PointsTableEntry(4, 3),
            new PointsTableEntry(5, 2),
            new PointsTableEntry(6, 1)
        ], fractionalTies);
    }

    private static (List<Result> Results, Dictionary<Guid, Guid> EntryToClub) CreateResults(
        params (int placing, Guid? clubId)[] placings)
    {
        var results = new List<Result>();
        var entryToClub = new Dictionary<Guid, Guid>();

        foreach (var (placing, clubId) in placings)
        {
            var entryId = Guid.NewGuid();
            var club = clubId ?? Guid.NewGuid();
            entryToClub[entryId] = club;
            results.Add(new Result(Guid.NewGuid(), entryId, new Placing(placing),
                status: ResultStatus.Confirmed));
        }

        return (results, entryToClub);
    }

    [Fact]
    public void CalculatePoints_StandardPlacings_CorrectPoints()
    {
        var table = CreateStandardTable();
        var (results, entryToClub) = CreateResults((1, null), (2, null), (3, null));

        var allocation = _service.CalculatePoints(Guid.NewGuid(), AgeGroup.Open, results, entryToClub, table);

        Assert.Equal(3, allocation.EntryPoints.Count(ep => ep.Points.Value > 0));
        Assert.Equal(8m, allocation.EntryPoints[0].Points.Value); // 1st
        Assert.Equal(6m, allocation.EntryPoints[1].Points.Value); // 2nd
        Assert.Equal(4m, allocation.EntryPoints[2].Points.Value); // 3rd
    }

    [Fact]
    public void CalculatePoints_TwoWayTie_FractionalEnabled()
    {
        var table = CreateStandardTable(fractionalTies: true);
        var club1 = Guid.NewGuid();
        var club2 = Guid.NewGuid();
        // Two athletes tied for 2nd
        var (results, entryToClub) = CreateResults((1, null), (2, club1), (2, club2));

        var allocation = _service.CalculatePoints(Guid.NewGuid(), AgeGroup.Open, results, entryToClub, table);

        // Tied for 2nd: average of 2nd (6) and 3rd (4) = 5
        Assert.Equal(8m, allocation.EntryPoints[0].Points.Value);
        Assert.Equal(5m, allocation.EntryPoints[1].Points.Value);
        Assert.Equal(5m, allocation.EntryPoints[2].Points.Value);
    }

    [Fact]
    public void CalculatePoints_TwoWayTie_FractionalDisabled()
    {
        var table = CreateStandardTable(fractionalTies: false);
        var (results, entryToClub) = CreateResults((1, null), (2, null), (2, null));

        var allocation = _service.CalculatePoints(Guid.NewGuid(), AgeGroup.Open, results, entryToClub, table);

        // Fractional disabled: both get 2nd-place points (6)
        Assert.Equal(8m, allocation.EntryPoints[0].Points.Value);
        Assert.Equal(6m, allocation.EntryPoints[1].Points.Value);
        Assert.Equal(6m, allocation.EntryPoints[2].Points.Value);
    }

    [Fact]
    public void CalculatePoints_ThreeWayTie_FractionalEnabled()
    {
        var table = CreateStandardTable(fractionalTies: true);
        var (results, entryToClub) = CreateResults((1, null), (1, null), (1, null));

        var allocation = _service.CalculatePoints(Guid.NewGuid(), AgeGroup.Open, results, entryToClub, table);

        // Three-way tie for 1st: average of 1st (8) + 2nd (6) + 3rd (4) = 18/3 = 6
        Assert.Equal(6m, allocation.EntryPoints[0].Points.Value);
        Assert.Equal(6m, allocation.EntryPoints[1].Points.Value);
        Assert.Equal(6m, allocation.EntryPoints[2].Points.Value);
    }

    [Fact]
    public void CalculatePoints_DQEntry_GetsZeroPoints()
    {
        var table = CreateStandardTable();
        var entryId = Guid.NewGuid();
        var entryToClub = new Dictionary<Guid, Guid> { [entryId] = Guid.NewGuid() };
        var results = new List<Result>
        {
            new(Guid.NewGuid(), entryId, new Placing(1), status: ResultStatus.Disqualified)
        };

        var allocation = _service.CalculatePoints(Guid.NewGuid(), AgeGroup.Open, results, entryToClub, table);

        Assert.Single(allocation.EntryPoints);
        Assert.Equal(0m, allocation.EntryPoints[0].Points.Value);
    }

    [Fact]
    public void CalculatePoints_DNSAndDNF_Filtered()
    {
        var table = CreateStandardTable();
        var entry1 = Guid.NewGuid();
        var entry2 = Guid.NewGuid();
        var entry3 = Guid.NewGuid();
        var entryToClub = new Dictionary<Guid, Guid>
        {
            [entry1] = Guid.NewGuid(),
            [entry2] = Guid.NewGuid(),
            [entry3] = Guid.NewGuid()
        };
        var results = new List<Result>
        {
            new(Guid.NewGuid(), entry1, new Placing(1), status: ResultStatus.Confirmed),
            new(Guid.NewGuid(), entry2, status: ResultStatus.DidNotStart),
            new(Guid.NewGuid(), entry3, status: ResultStatus.DidNotFinish)
        };

        var allocation = _service.CalculatePoints(Guid.NewGuid(), AgeGroup.Open, results, entryToClub, table);

        var scoringEntries = allocation.EntryPoints.Where(ep => ep.Points.Value > 0).ToList();
        Assert.Single(scoringEntries);
        Assert.Equal(8m, scoringEntries[0].Points.Value);
    }

    [Fact]
    public void AggregateClubPoints_MultipleEvents()
    {
        var club1 = Guid.NewGuid();
        var club2 = Guid.NewGuid();

        var alloc1 = new PointsAllocation(Guid.NewGuid(), AgeGroup.Open, [
            new EntryPoints(Guid.NewGuid(), club1, new Points(8)),
            new EntryPoints(Guid.NewGuid(), club2, new Points(6))
        ]);
        var alloc2 = new PointsAllocation(Guid.NewGuid(), AgeGroup.Open, [
            new EntryPoints(Guid.NewGuid(), club1, new Points(4)),
            new EntryPoints(Guid.NewGuid(), club2, new Points(8))
        ]);
        var alloc3 = new PointsAllocation(Guid.NewGuid(), AgeGroup.U14, [
            new EntryPoints(Guid.NewGuid(), club1, new Points(6)),
            new EntryPoints(Guid.NewGuid(), club2, new Points(4))
        ]);

        var ledger = _service.AggregateClubPoints([alloc1, alloc2, alloc3]);

        Assert.Equal(18m, ledger.OverallTotals[club1].Value); // 8+4+6
        Assert.Equal(18m, ledger.OverallTotals[club2].Value); // 6+8+4
    }

    [Fact]
    public void AggregateClubPoints_AgeGroupBreakdown()
    {
        var club1 = Guid.NewGuid();

        var allocOpen = new PointsAllocation(Guid.NewGuid(), AgeGroup.Open, [
            new EntryPoints(Guid.NewGuid(), club1, new Points(8))
        ]);
        var allocU14 = new PointsAllocation(Guid.NewGuid(), AgeGroup.U14, [
            new EntryPoints(Guid.NewGuid(), club1, new Points(6))
        ]);

        var ledger = _service.AggregateClubPoints([allocOpen, allocU14]);

        var openStandings = ledger.GetStandingsByAgeGroup(AgeGroup.Open);
        var u14Standings = ledger.GetStandingsByAgeGroup(AgeGroup.U14);

        Assert.Equal(8m, openStandings.First(s => s.ClubId == club1).Total.Value);
        Assert.Equal(6m, u14Standings.First(s => s.ClubId == club1).Total.Value);
        Assert.Equal(14m, ledger.OverallTotals[club1].Value);
    }

    [Fact]
    public void RecalculateOnCorrection_DebitsAndCredits()
    {
        var club1 = Guid.NewGuid();
        var club2 = Guid.NewGuid();

        var oldAlloc = new PointsAllocation(Guid.NewGuid(), AgeGroup.Open, [
            new EntryPoints(Guid.NewGuid(), club1, new Points(8)),
            new EntryPoints(Guid.NewGuid(), club2, new Points(6))
        ]);
        var newAlloc = new PointsAllocation(Guid.NewGuid(), AgeGroup.Open, [
            new EntryPoints(Guid.NewGuid(), club1, new Points(6)),
            new EntryPoints(Guid.NewGuid(), club2, new Points(8))
        ]);

        var ledger = _service.AggregateClubPoints([oldAlloc]);
        Assert.Equal(8m, ledger.OverallTotals[club1].Value);
        Assert.Equal(6m, ledger.OverallTotals[club2].Value);

        _service.RecalculateOnCorrection(ledger, oldAlloc, newAlloc);

        Assert.Equal(6m, ledger.OverallTotals[club1].Value);
        Assert.Equal(8m, ledger.OverallTotals[club2].Value);
    }

    [Fact]
    public void CalculatePoints_EmptyResults_ReturnsEmptyAllocation()
    {
        var table = CreateStandardTable();
        var allocation = _service.CalculatePoints(Guid.NewGuid(), AgeGroup.Open, [], new(), table);

        Assert.Empty(allocation.EntryPoints);
    }

    [Fact]
    public void CalculatePoints_SingleEntry_GetsFirstPlacePoints()
    {
        var table = CreateStandardTable();
        var (results, entryToClub) = CreateResults((1, null));

        var allocation = _service.CalculatePoints(Guid.NewGuid(), AgeGroup.Open, results, entryToClub, table);

        Assert.Single(allocation.EntryPoints);
        Assert.Equal(8m, allocation.EntryPoints[0].Points.Value);
    }

    [Fact]
    public void GetStandings_TiedClubs_BothAppear()
    {
        var club1 = Guid.NewGuid();
        var club2 = Guid.NewGuid();

        var alloc = new PointsAllocation(Guid.NewGuid(), AgeGroup.Open, [
            new EntryPoints(Guid.NewGuid(), club1, new Points(8)),
            new EntryPoints(Guid.NewGuid(), club2, new Points(8))
        ]);

        var ledger = _service.AggregateClubPoints([alloc]);
        var standings = ledger.GetStandings();

        Assert.Equal(2, standings.Count);
        Assert.Equal(standings[0].Total.Value, standings[1].Total.Value);
    }
}
