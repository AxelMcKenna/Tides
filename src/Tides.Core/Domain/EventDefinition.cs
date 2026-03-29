using Tides.Core.Domain.Enums;

namespace Tides.Core.Domain;

public class EventDefinition
{
    public Guid Id { get; private set; }
    public Guid CarnivalId { get; private set; }
    public string Name { get; private set; } = null!;
    public EventCategory Category { get; private set; }
    public AgeGroup AgeGroup { get; private set; }
    public Gender Gender { get; private set; }
    public int MaxLanes { get; private set; }
    public AdvancementRule AdvancementRule { get; private set; }
    public int AdvanceTopN { get; private set; }
    public int AdvanceFastestN { get; private set; }
    private readonly List<Round> _rounds = [];
    public IReadOnlyList<Round> Rounds => _rounds;

    private EventDefinition() { }

    public EventDefinition(Guid id, string name, EventCategory category,
        AgeGroup ageGroup, Gender gender, int maxLanes,
        AdvancementRule advancementRule = AdvancementRule.TopNPerHeat,
        int advanceTopN = 3, int advanceFastestN = 0)
    {
        Id = id;
        Name = name;
        Category = category;
        AgeGroup = ageGroup;
        Gender = gender;
        MaxLanes = maxLanes;
        AdvancementRule = advancementRule;
        AdvanceTopN = advanceTopN;
        AdvanceFastestN = advanceFastestN;
    }

    public Round AddRound(RoundType type)
    {
        var round = new Round(Guid.NewGuid(), type, _rounds.Count + 1);
        _rounds.Add(round);
        return round;
    }

    public Round? GetCurrentRound()
    {
        return _rounds.LastOrDefault(r => !r.IsComplete) ?? _rounds.LastOrDefault();
    }
}
