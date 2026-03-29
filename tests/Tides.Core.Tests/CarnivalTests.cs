using Tides.Core.Domain;
using Tides.Core.Domain.Enums;
using Tides.Core.Domain.ValueObjects;
using Tides.Core.Events;

namespace Tides.Core.Tests;

public class CarnivalTests
{
    [Fact]
    public void AddEvent_EventAppearsInList()
    {
        var carnival = CreateCarnival();
        var eventDef = CreateEventDef();

        carnival.AddEvent(eventDef);

        Assert.Single(carnival.Events);
        Assert.Equal(eventDef.Id, carnival.Events[0].Id);
    }

    [Fact]
    public void RemoveEvent_RemovesById()
    {
        var carnival = CreateCarnival();
        var eventDef = CreateEventDef();
        carnival.AddEvent(eventDef);

        carnival.RemoveEvent(eventDef.Id);

        Assert.Empty(carnival.Events);
    }

    [Fact]
    public void RemoveEvent_NonexistentId_DoesNotThrow()
    {
        var carnival = CreateCarnival();
        var eventDef = CreateEventDef();
        carnival.AddEvent(eventDef);

        carnival.RemoveEvent(Guid.NewGuid());

        Assert.Single(carnival.Events);
    }

    [Fact]
    public void SetPointsTable_StoresTable()
    {
        var carnival = CreateCarnival();
        var table = new PointsTable(Guid.NewGuid(), "Standard", [
            new PointsTableEntry(1, 8),
            new PointsTableEntry(2, 6)
        ]);

        carnival.SetPointsTable(table);

        Assert.NotNull(carnival.PointsTable);
        Assert.Equal(table.Id, carnival.PointsTable.Id);
    }

    [Fact]
    public void RecordResult_EventNotFound_Throws()
    {
        var carnival = CreateCarnival();

        var ex = Assert.Throws<InvalidOperationException>(() =>
            carnival.RecordResult(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                new Result(Guid.NewGuid(), Guid.NewGuid())));

        Assert.Contains("Event", ex.Message);
    }

    [Fact]
    public void RecordResult_RoundNotFound_Throws()
    {
        var carnival = CreateCarnival();
        var eventDef = CreateEventDef();
        carnival.AddEvent(eventDef);

        var ex = Assert.Throws<InvalidOperationException>(() =>
            carnival.RecordResult(eventDef.Id, Guid.NewGuid(), Guid.NewGuid(),
                new Result(Guid.NewGuid(), Guid.NewGuid())));

        Assert.Contains("Round", ex.Message);
    }

    [Fact]
    public void RecordResult_HeatNotFound_Throws()
    {
        var carnival = CreateCarnival();
        var eventDef = CreateEventDef();
        carnival.AddEvent(eventDef);
        var round = eventDef.AddRound(RoundType.Heat);

        var ex = Assert.Throws<InvalidOperationException>(() =>
            carnival.RecordResult(eventDef.Id, round.Id, Guid.NewGuid(),
                new Result(Guid.NewGuid(), Guid.NewGuid())));

        Assert.Contains("Heat", ex.Message);
    }

    [Fact]
    public void RecordResult_ValidPath_RaisesResultEnteredEvent()
    {
        var carnival = CreateCarnivalWithHeat(out var eventId, out var roundId, out var heatId, out var entryId);
        var result = new Result(Guid.NewGuid(), entryId, new Placing(1));

        carnival.RecordResult(eventId, roundId, heatId, result);

        Assert.Single(carnival.DomainEvents);
        Assert.IsType<ResultEntered>(carnival.DomainEvents[0]);
    }

    [Fact]
    public void LodgeProtest_AddsToProtestsList()
    {
        var carnival = CreateCarnival();

        carnival.LodgeProtest(Guid.NewGuid(), null, Guid.NewGuid(), "Wrong placing");

        Assert.Single(carnival.Protests);
    }

    [Fact]
    public void LodgeProtest_MultipleProtests_AllStored()
    {
        var carnival = CreateCarnival();

        carnival.LodgeProtest(Guid.NewGuid(), null, Guid.NewGuid(), "Reason 1");
        carnival.LodgeProtest(Guid.NewGuid(), null, Guid.NewGuid(), "Reason 2");
        carnival.LodgeProtest(Guid.NewGuid(), null, Guid.NewGuid(), "Reason 3");

        Assert.Equal(3, carnival.Protests.Count);
    }

    [Fact]
    public void LodgeProtest_ReturnsTheCreatedProtest()
    {
        var carnival = CreateCarnival();
        var clubId = Guid.NewGuid();

        var protest = carnival.LodgeProtest(Guid.NewGuid(), null, clubId, "Reason");

        Assert.Equal(clubId, protest.LodgedByClubId);
        Assert.Equal("Reason", protest.Reason);
        Assert.Equal(ProtestStatus.Lodged, protest.Status);
    }

    [Fact]
    public void AdjudicateProtest_NotFound_Throws()
    {
        var carnival = CreateCarnival();

        var ex = Assert.Throws<InvalidOperationException>(() =>
            carnival.AdjudicateProtest(Guid.NewGuid(), ProtestStatus.Upheld, "Reason"));

        Assert.Contains("Protest", ex.Message);
    }

    [Fact]
    public void AdjudicateProtest_RaisesProtestAdjudicatedEvent()
    {
        var carnival = CreateCarnival();
        var protest = carnival.LodgeProtest(Guid.NewGuid(), null, Guid.NewGuid(), "Reason");

        carnival.AdjudicateProtest(protest.Id, ProtestStatus.Dismissed, "No evidence");

        Assert.Single(carnival.DomainEvents);
        Assert.IsType<ProtestAdjudicated>(carnival.DomainEvents[0]);
    }

    [Fact]
    public void ClearDomainEvents_RemovesAll()
    {
        var carnival = CreateCarnivalWithHeat(out var eventId, out var roundId, out var heatId, out var entryId);
        carnival.RecordResult(eventId, roundId, heatId,
            new Result(Guid.NewGuid(), entryId, new Placing(1)));
        Assert.NotEmpty(carnival.DomainEvents);

        carnival.ClearDomainEvents();

        Assert.Empty(carnival.DomainEvents);
    }

    private static Carnival CreateCarnival()
    {
        return new Carnival(Guid.NewGuid(), "Test Carnival", Guid.NewGuid(),
            SanctionLevel.Regional, new DateOnly(2026, 3, 29), new DateOnly(2026, 3, 29));
    }

    private static EventDefinition CreateEventDef()
    {
        return new EventDefinition(Guid.NewGuid(), "U15 Sprint",
            EventCategory.Sprint, AgeGroup.U15, Gender.Male, 4);
    }

    private static Carnival CreateCarnivalWithHeat(
        out Guid eventId, out Guid roundId, out Guid heatId, out Guid entryId)
    {
        var carnival = CreateCarnival();
        var eventDef = CreateEventDef();
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
