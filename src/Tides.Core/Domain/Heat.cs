namespace Tides.Core.Domain;

public class Heat
{
    public Guid Id { get; private set; }
    public Guid RoundId { get; private set; }
    public int HeatNumber { get; private set; }
    private readonly List<Entry> _entries = [];
    public IReadOnlyList<Entry> Entries => _entries;
    private readonly List<Result> _results = [];
    public IReadOnlyList<Result> Results => _results;
    public int Version { get; private set; } = 1;
    public bool IsComplete { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private Heat() { }

    public Heat(Guid id, int heatNumber)
    {
        Id = id;
        HeatNumber = heatNumber;
    }

    public void AssignEntry(Entry entry, int lane)
    {
        entry.AssignToHeat(Id);
        entry.AssignLane(lane);
        _entries.Add(entry);
    }

    public void RemoveEntry(Guid entryId)
    {
        _entries.RemoveAll(e => e.Id == entryId);
    }

    public void RecordResult(Result result)
    {
        if (_entries.All(e => e.Id != result.EntryId))
            throw new InvalidOperationException($"Entry {result.EntryId} is not in this heat.");

        var entry = _entries.First(e => e.Id == result.EntryId);
        if (entry.IsWithdrawn)
            throw new InvalidOperationException($"Cannot record result for withdrawn entry {result.EntryId}.");

        _results.Add(result);
        IncrementVersion();
    }

    public void IncrementVersion()
    {
        Version++;
    }

    public List<Result> GetRankedResults()
    {
        return _results
            .Where(r => r.Status is not Enums.ResultStatus.Disqualified
                and not Enums.ResultStatus.DidNotStart
                and not Enums.ResultStatus.DidNotFinish)
            .OrderBy(r => r.Placing?.Position ?? int.MaxValue)
            .ThenBy(r => r.Time?.Time ?? TimeSpan.MaxValue)
            .ToList();
    }

    public void MarkComplete()
    {
        IsComplete = true;
        CompletedAt = DateTime.UtcNow;
    }
}
