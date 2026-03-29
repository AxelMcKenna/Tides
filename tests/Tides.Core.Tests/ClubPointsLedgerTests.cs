using Tides.Core.Domain;
using Tides.Core.Domain.Enums;
using Tides.Core.Domain.ValueObjects;

namespace Tides.Core.Tests;

public class ClubPointsLedgerTests
{
    [Fact]
    public void Credit_SingleClub_IncreasesTotal()
    {
        var ledger = new ClubPointsLedger();
        var clubId = Guid.NewGuid();

        ledger.Credit(clubId, AgeGroup.Open, new Points(8));

        Assert.Equal(8m, ledger.OverallTotals[clubId].Value);
    }

    [Fact]
    public void Credit_MultipleCredits_Accumulates()
    {
        var ledger = new ClubPointsLedger();
        var clubId = Guid.NewGuid();

        ledger.Credit(clubId, AgeGroup.Open, new Points(8));
        ledger.Credit(clubId, AgeGroup.Open, new Points(6));

        Assert.Equal(14m, ledger.OverallTotals[clubId].Value);
    }

    [Fact]
    public void Debit_ReducesTotal()
    {
        var ledger = new ClubPointsLedger();
        var clubId = Guid.NewGuid();

        ledger.Credit(clubId, AgeGroup.Open, new Points(8));
        ledger.Debit(clubId, AgeGroup.Open, new Points(3));

        Assert.Equal(5m, ledger.OverallTotals[clubId].Value);
    }

    [Fact]
    public void Credit_TracksAgeGroup()
    {
        var ledger = new ClubPointsLedger();
        var clubId = Guid.NewGuid();

        ledger.Credit(clubId, AgeGroup.U15, new Points(8));
        ledger.Credit(clubId, AgeGroup.U17, new Points(6));

        Assert.Equal(8m, ledger.ByAgeGroup[(clubId, AgeGroup.U15)].Value);
        Assert.Equal(6m, ledger.ByAgeGroup[(clubId, AgeGroup.U17)].Value);
        Assert.Equal(14m, ledger.OverallTotals[clubId].Value);
    }

    [Fact]
    public void GetStandings_OrderedDescending()
    {
        var ledger = new ClubPointsLedger();
        var clubA = Guid.NewGuid();
        var clubB = Guid.NewGuid();

        ledger.Credit(clubA, AgeGroup.Open, new Points(10));
        ledger.Credit(clubB, AgeGroup.Open, new Points(20));

        var standings = ledger.GetStandings();
        Assert.Equal(clubB, standings[0].ClubId);
        Assert.Equal(clubA, standings[1].ClubId);
    }

    [Fact]
    public void GetStandings_EmptyLedger_ReturnsEmpty()
    {
        var ledger = new ClubPointsLedger();

        Assert.Empty(ledger.GetStandings());
    }

    [Fact]
    public void GetStandingsByAgeGroup_FiltersCorrectly()
    {
        var ledger = new ClubPointsLedger();
        var clubA = Guid.NewGuid();

        ledger.Credit(clubA, AgeGroup.U15, new Points(8));
        ledger.Credit(clubA, AgeGroup.U17, new Points(6));

        var u15Standings = ledger.GetStandingsByAgeGroup(AgeGroup.U15);
        Assert.Single(u15Standings);
        Assert.Equal(8m, u15Standings[0].Total.Value);
    }

    [Fact]
    public void GetStandingsByAgeGroup_NoMatchingEntries_ReturnsEmpty()
    {
        var ledger = new ClubPointsLedger();
        ledger.Credit(Guid.NewGuid(), AgeGroup.U15, new Points(8));

        Assert.Empty(ledger.GetStandingsByAgeGroup(AgeGroup.Open));
    }

    [Fact]
    public void Debit_CanGoNegative()
    {
        var ledger = new ClubPointsLedger();
        var clubId = Guid.NewGuid();

        ledger.Debit(clubId, AgeGroup.Open, new Points(5));

        Assert.Equal(-5m, ledger.OverallTotals[clubId].Value);
    }
}
