using Tides.Core.Domain.ValueObjects;

namespace Tides.Core.Domain;

public class PointsTable
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    private readonly List<PointsTableEntry> _entries;
    public IReadOnlyList<PointsTableEntry> Entries => _entries;
    public bool FractionalTiesEnabled { get; private set; }

    private PointsTable() { _entries = []; }

    public PointsTable(Guid id, string name, List<PointsTableEntry> entries, bool fractionalTiesEnabled = true)
    {
        Id = id;
        Name = name;
        _entries = entries.OrderBy(e => e.Placing).ToList();
        FractionalTiesEnabled = fractionalTiesEnabled;
    }

    public decimal GetPointsForPlacing(int placing)
    {
        var entry = _entries.FirstOrDefault(e => e.Placing == placing);
        return entry?.Points ?? 0m;
    }

    /// <summary>
    /// For tied placings: averages the points across the positions that the tied athletes span.
    /// E.g., 2 athletes tied for 2nd: average points for 2nd and 3rd.
    /// If fractional ties disabled, all tied athletes get the higher placing's points.
    /// </summary>
    public decimal GetPointsForTiedPlacing(int placing, int tiedCount)
    {
        if (tiedCount <= 1)
            return GetPointsForPlacing(placing);

        if (!FractionalTiesEnabled)
            return GetPointsForPlacing(placing);

        var total = 0m;
        for (var i = 0; i < tiedCount; i++)
            total += GetPointsForPlacing(placing + i);

        return total / tiedCount;
    }
}
