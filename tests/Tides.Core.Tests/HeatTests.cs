using Tides.Core.Domain;
using Tides.Core.Domain.Enums;
using Tides.Core.Domain.ValueObjects;

namespace Tides.Core.Tests;

public class HeatTests
{
    [Fact]
    public void AssignEntry_AddsEntryToList()
    {
        var heat = new Heat(Guid.NewGuid(), 1);
        var entry = CreateEntry();

        heat.AssignEntry(entry, 1);

        Assert.Single(heat.Entries);
        Assert.Equal(entry.Id, heat.Entries[0].Id);
    }

    [Fact]
    public void AssignEntry_SetsHeatIdAndLane()
    {
        var heat = new Heat(Guid.NewGuid(), 1);
        var entry = CreateEntry();

        heat.AssignEntry(entry, 3);

        Assert.Equal(heat.Id, entry.HeatId);
        Assert.Equal(3, entry.Lane);
    }

    [Fact]
    public void AssignEntry_MultipleEntries()
    {
        var heat = new Heat(Guid.NewGuid(), 1);
        var entry1 = CreateEntry();
        var entry2 = CreateEntry();

        heat.AssignEntry(entry1, 1);
        heat.AssignEntry(entry2, 2);

        Assert.Equal(2, heat.Entries.Count);
    }

    [Fact]
    public void RemoveEntry_RemovesById()
    {
        var heat = new Heat(Guid.NewGuid(), 1);
        var entry1 = CreateEntry();
        var entry2 = CreateEntry();
        heat.AssignEntry(entry1, 1);
        heat.AssignEntry(entry2, 2);

        heat.RemoveEntry(entry1.Id);

        Assert.Single(heat.Entries);
        Assert.Equal(entry2.Id, heat.Entries[0].Id);
    }

    [Fact]
    public void RemoveEntry_NonexistentId_DoesNotThrow()
    {
        var heat = new Heat(Guid.NewGuid(), 1);
        var entry = CreateEntry();
        heat.AssignEntry(entry, 1);

        heat.RemoveEntry(Guid.NewGuid());

        Assert.Single(heat.Entries);
    }

    [Fact]
    public void RecordResult_ValidEntry_AddsToResults()
    {
        var heat = new Heat(Guid.NewGuid(), 1);
        var entry = CreateEntry();
        heat.AssignEntry(entry, 1);
        var result = new Result(Guid.NewGuid(), entry.Id, new Placing(1));

        heat.RecordResult(result);

        Assert.Single(heat.Results);
    }

    [Fact]
    public void RecordResult_EntryNotInHeat_Throws()
    {
        var heat = new Heat(Guid.NewGuid(), 1);
        var result = new Result(Guid.NewGuid(), Guid.NewGuid(), new Placing(1));

        var ex = Assert.Throws<InvalidOperationException>(() => heat.RecordResult(result));
        Assert.Contains("not in this heat", ex.Message);
    }

    [Fact]
    public void RecordResult_WithdrawnEntry_Throws()
    {
        var heat = new Heat(Guid.NewGuid(), 1);
        var entry = CreateEntry();
        heat.AssignEntry(entry, 1);
        entry.Withdraw();
        var result = new Result(Guid.NewGuid(), entry.Id, new Placing(1));

        var ex = Assert.Throws<InvalidOperationException>(() => heat.RecordResult(result));
        Assert.Contains("withdrawn", ex.Message);
    }

    [Fact]
    public void GetRankedResults_ExcludesDisqualified()
    {
        var heat = new Heat(Guid.NewGuid(), 1);
        var entry1 = CreateEntry();
        var entry2 = CreateEntry();
        heat.AssignEntry(entry1, 1);
        heat.AssignEntry(entry2, 2);

        heat.RecordResult(new Result(Guid.NewGuid(), entry1.Id, new Placing(1)));
        heat.RecordResult(new Result(Guid.NewGuid(), entry2.Id,
            status: ResultStatus.Disqualified));

        var ranked = heat.GetRankedResults();
        Assert.Single(ranked);
        Assert.Equal(entry1.Id, ranked[0].EntryId);
    }

    [Fact]
    public void GetRankedResults_ExcludesDNS()
    {
        var heat = new Heat(Guid.NewGuid(), 1);
        var entry = CreateEntry();
        heat.AssignEntry(entry, 1);
        heat.RecordResult(new Result(Guid.NewGuid(), entry.Id,
            status: ResultStatus.DidNotStart));

        Assert.Empty(heat.GetRankedResults());
    }

    [Fact]
    public void GetRankedResults_ExcludesDNF()
    {
        var heat = new Heat(Guid.NewGuid(), 1);
        var entry = CreateEntry();
        heat.AssignEntry(entry, 1);
        heat.RecordResult(new Result(Guid.NewGuid(), entry.Id,
            status: ResultStatus.DidNotFinish));

        Assert.Empty(heat.GetRankedResults());
    }

    [Fact]
    public void GetRankedResults_OrdersByPlacingThenTime()
    {
        var heat = new Heat(Guid.NewGuid(), 1);
        var entry1 = CreateEntry();
        var entry2 = CreateEntry();
        var entry3 = CreateEntry();
        heat.AssignEntry(entry1, 1);
        heat.AssignEntry(entry2, 2);
        heat.AssignEntry(entry3, 3);

        // Record in non-sequential order
        heat.RecordResult(new Result(Guid.NewGuid(), entry3.Id, new Placing(3)));
        heat.RecordResult(new Result(Guid.NewGuid(), entry1.Id, new Placing(1)));
        heat.RecordResult(new Result(Guid.NewGuid(), entry2.Id, new Placing(2)));

        var ranked = heat.GetRankedResults();
        Assert.Equal(entry1.Id, ranked[0].EntryId);
        Assert.Equal(entry2.Id, ranked[1].EntryId);
        Assert.Equal(entry3.Id, ranked[2].EntryId);
    }

    [Fact]
    public void GetRankedResults_SamePlacing_OrdersByTime()
    {
        var heat = new Heat(Guid.NewGuid(), 1);
        var entry1 = CreateEntry();
        var entry2 = CreateEntry();
        heat.AssignEntry(entry1, 1);
        heat.AssignEntry(entry2, 2);

        heat.RecordResult(new Result(Guid.NewGuid(), entry1.Id, new Placing(1),
            new TimeResult(TimeSpan.FromSeconds(35))));
        heat.RecordResult(new Result(Guid.NewGuid(), entry2.Id, new Placing(1),
            new TimeResult(TimeSpan.FromSeconds(30))));

        var ranked = heat.GetRankedResults();
        Assert.Equal(entry2.Id, ranked[0].EntryId); // faster time first
        Assert.Equal(entry1.Id, ranked[1].EntryId);
    }

    [Fact]
    public void GetRankedResults_NullPlacing_SortsLast()
    {
        var heat = new Heat(Guid.NewGuid(), 1);
        var entry1 = CreateEntry();
        var entry2 = CreateEntry();
        heat.AssignEntry(entry1, 1);
        heat.AssignEntry(entry2, 2);

        heat.RecordResult(new Result(Guid.NewGuid(), entry1.Id)); // null placing
        heat.RecordResult(new Result(Guid.NewGuid(), entry2.Id, new Placing(1)));

        var ranked = heat.GetRankedResults();
        Assert.Equal(entry2.Id, ranked[0].EntryId);
        Assert.Equal(entry1.Id, ranked[1].EntryId);
    }

    [Fact]
    public void MarkComplete_SetsIsCompleteAndTimestamp()
    {
        var heat = new Heat(Guid.NewGuid(), 1);
        Assert.False(heat.IsComplete);
        Assert.Null(heat.CompletedAt);

        heat.MarkComplete();

        Assert.True(heat.IsComplete);
        Assert.NotNull(heat.CompletedAt);
        Assert.True(heat.CompletedAt.Value <= DateTime.UtcNow);
    }

    private static Entry CreateEntry()
    {
        return new Entry(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
    }
}
