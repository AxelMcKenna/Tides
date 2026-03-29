using Tides.Core.Domain;
using Tides.Core.Domain.Enums;

namespace Tides.Core.Tests;

public class EventDefinitionTests
{
    [Fact]
    public void AddRound_FirstRound_RoundNumberIs1()
    {
        var eventDef = CreateEventDef();

        var round = eventDef.AddRound(RoundType.Heat);

        Assert.Equal(1, round.RoundNumber);
    }

    [Fact]
    public void AddRound_SecondRound_RoundNumberIs2()
    {
        var eventDef = CreateEventDef();
        eventDef.AddRound(RoundType.Heat);

        var round2 = eventDef.AddRound(RoundType.Final);

        Assert.Equal(2, round2.RoundNumber);
    }

    [Fact]
    public void AddRound_ReturnsRoundWithCorrectType()
    {
        var eventDef = CreateEventDef();

        var round = eventDef.AddRound(RoundType.Semifinal);

        Assert.Equal(RoundType.Semifinal, round.Type);
    }

    [Fact]
    public void AddRound_AppearsInRoundsList()
    {
        var eventDef = CreateEventDef();

        var round = eventDef.AddRound(RoundType.Heat);

        Assert.Single(eventDef.Rounds);
        Assert.Equal(round.Id, eventDef.Rounds[0].Id);
    }

    [Fact]
    public void GetCurrentRound_NoRounds_ReturnsNull()
    {
        var eventDef = CreateEventDef();

        Assert.Null(eventDef.GetCurrentRound());
    }

    [Fact]
    public void GetCurrentRound_OneIncomplete_ReturnsThatOne()
    {
        var eventDef = CreateEventDef();
        var round1 = eventDef.AddRound(RoundType.Heat);
        round1.MarkComplete();
        var round2 = eventDef.AddRound(RoundType.Final);

        var current = eventDef.GetCurrentRound();

        Assert.Equal(round2.Id, current!.Id);
    }

    [Fact]
    public void GetCurrentRound_AllComplete_ReturnsLast()
    {
        var eventDef = CreateEventDef();
        var round1 = eventDef.AddRound(RoundType.Heat);
        round1.MarkComplete();
        var round2 = eventDef.AddRound(RoundType.Final);
        round2.MarkComplete();

        var current = eventDef.GetCurrentRound();

        Assert.Equal(round2.Id, current!.Id);
    }

    [Fact]
    public void Constructor_StoresAdvancementDefaults()
    {
        var eventDef = CreateEventDef();

        Assert.Equal(AdvancementRule.TopNPerHeat, eventDef.AdvancementRule);
        Assert.Equal(3, eventDef.AdvanceTopN);
        Assert.Equal(0, eventDef.AdvanceFastestN);
    }

    private static EventDefinition CreateEventDef()
    {
        return new EventDefinition(Guid.NewGuid(), "U15 Sprint",
            EventCategory.Sprint, AgeGroup.U15, Gender.Male, 4);
    }
}
