using Tides.Core.Domain.Enums;

namespace Tides.Core.Domain;

public class Round
{
    public Guid Id { get; private set; }
    public Guid EventDefinitionId { get; private set; }
    public RoundType Type { get; private set; }
    public int RoundNumber { get; private set; }
    private readonly List<Heat> _heats = [];
    public IReadOnlyList<Heat> Heats => _heats;
    public bool IsComplete { get; private set; }

    private Round() { }

    public Round(Guid id, RoundType type, int roundNumber)
    {
        Id = id;
        Type = type;
        RoundNumber = roundNumber;
    }

    public void AddHeat(Heat heat)
    {
        _heats.Add(heat);
    }

    public void MarkComplete()
    {
        IsComplete = true;
    }

    public List<Result> GetAllResults()
    {
        return _heats.SelectMany(h => h.GetRankedResults()).ToList();
    }
}
