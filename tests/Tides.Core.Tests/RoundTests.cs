using Tides.Core.Domain;
using Tides.Core.Domain.Enums;
using Tides.Core.Domain.ValueObjects;

namespace Tides.Core.Tests;

public class RoundTests
{
    [Fact]
    public void AddHeat_AddsToHeatsList()
    {
        var round = new Round(Guid.NewGuid(), RoundType.Heat, 1);
        var heat = new Heat(Guid.NewGuid(), 1);

        round.AddHeat(heat);

        Assert.Single(round.Heats);
        Assert.Equal(heat.Id, round.Heats[0].Id);
    }

    [Fact]
    public void MarkComplete_SetsIsComplete()
    {
        var round = new Round(Guid.NewGuid(), RoundType.Heat, 1);
        Assert.False(round.IsComplete);

        round.MarkComplete();

        Assert.True(round.IsComplete);
    }

    [Fact]
    public void GetAllResults_AggregatesAcrossHeats()
    {
        var round = new Round(Guid.NewGuid(), RoundType.Heat, 1);
        var heat1 = new Heat(Guid.NewGuid(), 1);
        var heat2 = new Heat(Guid.NewGuid(), 2);

        var entry1 = new Entry(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        var entry2 = new Entry(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        heat1.AssignEntry(entry1, 1);
        heat2.AssignEntry(entry2, 1);

        heat1.RecordResult(new Result(Guid.NewGuid(), entry1.Id, new Placing(1)));
        heat2.RecordResult(new Result(Guid.NewGuid(), entry2.Id, new Placing(1)));

        round.AddHeat(heat1);
        round.AddHeat(heat2);

        var results = round.GetAllResults();
        Assert.Equal(2, results.Count);
    }

    [Fact]
    public void GetAllResults_ExcludesDisqualified()
    {
        var round = new Round(Guid.NewGuid(), RoundType.Heat, 1);
        var heat = new Heat(Guid.NewGuid(), 1);

        var entry1 = new Entry(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        var entry2 = new Entry(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        heat.AssignEntry(entry1, 1);
        heat.AssignEntry(entry2, 2);

        heat.RecordResult(new Result(Guid.NewGuid(), entry1.Id, new Placing(1)));
        heat.RecordResult(new Result(Guid.NewGuid(), entry2.Id, status: ResultStatus.Disqualified));

        round.AddHeat(heat);

        var results = round.GetAllResults();
        Assert.Single(results);
    }

    [Fact]
    public void GetAllResults_EmptyRound_ReturnsEmpty()
    {
        var round = new Round(Guid.NewGuid(), RoundType.Heat, 1);

        Assert.Empty(round.GetAllResults());
    }
}
