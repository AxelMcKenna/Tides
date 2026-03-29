using Tides.Core.Domain;
using Tides.Core.Domain.Enums;
using Tides.Core.Domain.ValueObjects;
using Tides.Core.Events;

namespace Tides.Core.Tests;

public class DomainTests
{
    [Fact]
    public void Member_GetAgeGroup_14YearOld_ReturnsU15()
    {
        var member = new Member(Guid.NewGuid(), Guid.NewGuid(), "Test", "Member",
            new DateOnly(2012, 3, 29), Gender.Male);

        var ageGroup = member.GetAgeGroup(new DateOnly(2026, 3, 29));

        Assert.Equal(AgeGroup.U15, ageGroup);
    }

    [Fact]
    public void Member_GetAgeGroup_16YearOld_ReturnsU17()
    {
        var member = new Member(Guid.NewGuid(), Guid.NewGuid(), "Test", "Member",
            new DateOnly(2010, 1, 15), Gender.Female);

        var ageGroup = member.GetAgeGroup(new DateOnly(2026, 3, 29));

        Assert.Equal(AgeGroup.U17, ageGroup);
    }

    [Fact]
    public void Member_GetAgeGroup_18YearOld_ReturnsU19()
    {
        var member = new Member(Guid.NewGuid(), Guid.NewGuid(), "Test", "Member",
            new DateOnly(2008, 1, 15), Gender.Male);

        var ageGroup = member.GetAgeGroup(new DateOnly(2026, 3, 29));

        Assert.Equal(AgeGroup.U19, ageGroup);
    }

    [Fact]
    public void Member_GetAgeGroup_OpenAge()
    {
        var member = new Member(Guid.NewGuid(), Guid.NewGuid(), "Test", "Member",
            new DateOnly(2000, 6, 1), Gender.Male);

        var ageGroup = member.GetAgeGroup(new DateOnly(2026, 3, 29));

        Assert.Equal(AgeGroup.Open, ageGroup);
    }

    [Fact]
    public void PointsTable_OutOfRangePlacing_ReturnsZero()
    {
        var table = new PointsTable(Guid.NewGuid(), "Test", [
            new PointsTableEntry(1, 8),
            new PointsTableEntry(2, 6),
            new PointsTableEntry(3, 4)
        ]);

        Assert.Equal(0m, table.GetPointsForPlacing(10));
    }

    [Fact]
    public void Carnival_RecordResult_RaisesResultEnteredEvent()
    {
        var carnival = CreateCarnivalWithHeat(out var eventId, out var roundId, out var heatId, out var entryId);
        var result = new Result(Guid.NewGuid(), entryId, new Placing(1));

        carnival.RecordResult(eventId, roundId, heatId, result);

        Assert.Single(carnival.DomainEvents);
        Assert.IsType<ResultEntered>(carnival.DomainEvents[0]);
    }

    [Fact]
    public void Carnival_CorrectResult_RaisesResultCorrectedEvent()
    {
        var carnival = CreateCarnivalWithHeat(out var eventId, out var roundId, out var heatId, out var entryId);
        var result = new Result(Guid.NewGuid(), entryId, new Placing(1));
        carnival.RecordResult(eventId, roundId, heatId, result);
        carnival.ClearDomainEvents();

        carnival.CorrectResult(eventId, heatId, result, new Placing(2), null, "Timing error", "official-1");

        Assert.Single(carnival.DomainEvents);
        Assert.IsType<ResultCorrected>(carnival.DomainEvents[0]);
        Assert.Equal(ResultStatus.Corrected, result.Status);
    }

    [Fact]
    public void Carnival_AdjudicateProtest_RaisesProtestAdjudicatedEvent()
    {
        var carnival = new Carnival(Guid.NewGuid(), "Test", Guid.NewGuid(),
            SanctionLevel.Regional, new DateOnly(2026, 3, 29), new DateOnly(2026, 3, 29));

        var protest = carnival.LodgeProtest(Guid.NewGuid(), null, Guid.NewGuid(), "Wrong placing");
        carnival.AdjudicateProtest(protest.Id, ProtestStatus.Upheld, "Review confirmed error");

        Assert.Single(carnival.DomainEvents);
        Assert.IsType<ProtestAdjudicated>(carnival.DomainEvents[0]);
        Assert.Equal(ProtestStatus.Upheld, protest.Status);
    }

    [Fact]
    public void Entry_Withdraw_SetsIsWithdrawn()
    {
        var entry = new Entry(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        entry.AssignLane(3);

        entry.Withdraw();

        Assert.True(entry.IsWithdrawn);
        Assert.Null(entry.Lane);
    }

    [Fact]
    public void Heat_RecordResult_ForWithdrawnEntry_Throws()
    {
        var heat = new Heat(Guid.NewGuid(), 1);
        var entry = new Entry(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        heat.AssignEntry(entry, 1);
        entry.Withdraw();

        var result = new Result(Guid.NewGuid(), entry.Id, new Placing(1));

        Assert.Throws<InvalidOperationException>(() => heat.RecordResult(result));
    }

    [Fact]
    public void Placing_InvalidPosition_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Placing(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Placing(-1));
    }

    private static Carnival CreateCarnivalWithHeat(
        out Guid eventId, out Guid roundId, out Guid heatId, out Guid entryId)
    {
        var carnival = new Carnival(Guid.NewGuid(), "Test Carnival", Guid.NewGuid(),
            SanctionLevel.Regional, new DateOnly(2026, 3, 29), new DateOnly(2026, 3, 29));

        var eventDef = new EventDefinition(Guid.NewGuid(), "U15 Sprint", EventCategory.Sprint,
            AgeGroup.U15, Gender.Male, 4);
        carnival.AddEvent(eventDef);
        eventId = eventDef.Id;

        var round = eventDef.AddRound(RoundType.Heat);
        roundId = round.Id;

        var heat = new Heat(Guid.NewGuid(), 1);
        round.AddHeat(heat);
        heatId = heat.Id;

        var entry = new Entry(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        heat.AssignEntry(entry, 1);
        entryId = entry.Id;

        return carnival;
    }
}
