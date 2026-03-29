using Tides.Core.Domain;
using Tides.Core.Domain.Enums;
using Tides.Core.Domain.ValueObjects;

namespace Tides.Core.Services;

public class DrawGeneratorService : IDrawGeneratorService
{
    public DrawResult GenerateHeats(List<Entry> entries, int maxLanes, RoundType roundType)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxLanes, 2);
        if (entries.Count == 0)
            throw new ArgumentException("Cannot generate heats with zero entries.", nameof(entries));

        var activeEntries = entries.Where(e => !e.IsWithdrawn).ToList();
        if (activeEntries.Count == 0)
            throw new ArgumentException("All entries are withdrawn.", nameof(entries));

        var heatCount = (int)Math.Ceiling((double)activeEntries.Count / maxLanes);
        var heats = new List<Heat>();
        for (var i = 0; i < heatCount; i++)
            heats.Add(new Heat(Guid.NewGuid(), i + 1));

        // Round-robin distribution
        for (var i = 0; i < activeEntries.Count; i++)
        {
            var heatIndex = i % heatCount;
            var laneInHeat = (i / heatCount) + 1;
            heats[heatIndex].AssignEntry(activeEntries[i], laneInHeat);
        }

        // For finals, apply centre-lane seeding within each heat
        if (roundType is RoundType.Final or RoundType.Semifinal)
        {
            foreach (var heat in heats)
                ApplyCentreLaneSeeding(heat);
        }

        return new DrawResult(heats, [new AuditEntry(DateTime.UtcNow, "system",
            "HeatsGenerated", $"{heatCount} heats for {activeEntries.Count} entries, max {maxLanes} lanes")]);
    }

    public DrawResult GenerateSeededDraw(
        List<Result> previousRoundResults,
        Dictionary<Guid, Entry> entriesById,
        int maxLanes,
        AdvancementRule rule,
        int topN,
        int fastestN)
    {
        var advancingEntryIds = GetAdvancingEntries(previousRoundResults, rule, topN, fastestN);

        var advancingEntries = advancingEntryIds
            .Select(id => entriesById[id])
            .ToList();

        if (advancingEntries.Count == 0)
            throw new InvalidOperationException("No entries advanced from the previous round.");

        var heatCount = (int)Math.Ceiling((double)advancingEntries.Count / maxLanes);
        var heats = new List<Heat>();
        for (var i = 0; i < heatCount; i++)
            heats.Add(new Heat(Guid.NewGuid(), i + 1));

        if (heatCount == 1)
        {
            // Single heat — assign in seed order, apply centre-lane seeding
            for (var i = 0; i < advancingEntries.Count; i++)
                heats[0].AssignEntry(advancingEntries[i], i + 1);

            ApplyCentreLaneSeeding(heats[0]);
        }
        else
        {
            // Snake seeding across heats: 1→A, 2→B, 3→B, 4→A, 5→A, 6→B...
            SnakeSeed(advancingEntries, heats);

            foreach (var heat in heats)
                ApplyCentreLaneSeeding(heat);
        }

        return new DrawResult(heats, [new AuditEntry(DateTime.UtcNow, "system",
            "SeededDrawGenerated", $"{advancingEntries.Count} qualifiers into {heatCount} heats")]);
    }

    public DrawResult RedrawOnWithdrawal(DrawResult currentDraw, Guid withdrawnEntryId)
    {
        Heat? sourceHeat = null;
        Entry? withdrawnEntry = null;

        foreach (var heat in currentDraw.Heats)
        {
            var entry = heat.Entries.FirstOrDefault(e => e.Id == withdrawnEntryId);
            if (entry != null)
            {
                sourceHeat = heat;
                withdrawnEntry = entry;
                break;
            }
        }

        if (withdrawnEntry == null)
            throw new InvalidOperationException($"Entry {withdrawnEntryId} not found in any heat.");

        withdrawnEntry.Withdraw();
        sourceHeat!.RemoveEntry(withdrawnEntryId);

        // Rebalance if needed — move from largest heat to smallest
        var sortedBySize = currentDraw.Heats.OrderByDescending(h => h.Entries.Count).ToList();
        var largest = sortedBySize.First();
        var smallest = sortedBySize.Last();

        if (largest.Entries.Count - smallest.Entries.Count > 1)
        {
            var lastEntry = largest.Entries.Last();
            largest.RemoveEntry(lastEntry.Id);
            smallest.AssignEntry(lastEntry, smallest.Entries.Count + 1);
        }

        // Reassign lanes sequentially in affected heats
        ReassignLanes(sourceHeat);
        if (largest != sourceHeat) ReassignLanes(largest);
        if (smallest != sourceHeat && smallest != largest) ReassignLanes(smallest);

        var audit = new List<AuditEntry>(currentDraw.AuditTrail)
        {
            new(DateTime.UtcNow, "system", "WithdrawalRedraw",
                $"Entry {withdrawnEntryId} withdrawn, heats rebalanced")
        };

        return new DrawResult(currentDraw.Heats, audit);
    }

    public DrawResult ManuallyAssign(DrawResult currentDraw, Guid entryId, int heatIndex, int lane, string userId)
    {
        if (heatIndex < 0 || heatIndex >= currentDraw.Heats.Count)
            throw new ArgumentOutOfRangeException(nameof(heatIndex));

        // Find and remove from current heat
        foreach (var heat in currentDraw.Heats)
        {
            var entry = heat.Entries.FirstOrDefault(e => e.Id == entryId);
            if (entry != null)
            {
                heat.RemoveEntry(entryId);
                currentDraw.Heats[heatIndex].AssignEntry(entry, lane);

                var audit = new List<AuditEntry>(currentDraw.AuditTrail)
                {
                    new(DateTime.UtcNow, userId, "ManualAssignment",
                        $"Entry {entryId} moved to heat {heatIndex + 1} lane {lane}")
                };

                return new DrawResult(currentDraw.Heats, audit);
            }
        }

        throw new InvalidOperationException($"Entry {entryId} not found in any heat.");
    }

    /// <summary>
    /// Determines which entries advance from a round based on the advancement rule.
    /// Returns entry IDs in seeded order (best first).
    /// </summary>
    private static List<Guid> GetAdvancingEntries(
        List<Result> results, AdvancementRule rule, int topN, int fastestN)
    {
        // Group results by heat (using a simple approach — results carry entry IDs)
        var ranked = results
            .Where(r => r.Status is not ResultStatus.Disqualified
                and not ResultStatus.DidNotStart
                and not ResultStatus.DidNotFinish)
            .OrderBy(r => r.Placing?.Position ?? int.MaxValue)
            .ThenBy(r => r.Time?.Time ?? TimeSpan.MaxValue)
            .ToList();

        return rule switch
        {
            AdvancementRule.TopNPerHeat => ranked.Take(topN * GetHeatCount(ranked)).Select(r => r.EntryId).ToList(),
            AdvancementRule.FastestLoserAcrossHeats => ranked
                .OrderBy(r => r.Time?.Time ?? TimeSpan.MaxValue)
                .Take(fastestN)
                .Select(r => r.EntryId)
                .ToList(),
            AdvancementRule.Combined => ranked
                .Take(topN * GetHeatCount(ranked))
                .Union(ranked.Skip(topN * GetHeatCount(ranked))
                    .OrderBy(r => r.Time?.Time ?? TimeSpan.MaxValue)
                    .Take(fastestN))
                .Select(r => r.EntryId)
                .ToList(),
            _ => throw new ArgumentOutOfRangeException(nameof(rule))
        };
    }

    private static int GetHeatCount(List<Result> results)
    {
        // Estimate heat count from result distribution — at minimum 1
        return Math.Max(1, results.Count > 0 ? (int)Math.Ceiling(results.Count / 4.0) : 1);
    }

    /// <summary>
    /// Centre-lane seeding: fastest/1st gets centre lane, alternating outward.
    /// For n lanes: centre = ceil(n/2), then centre+1, centre-1, centre+2, ...
    /// </summary>
    internal static List<int> GetCentreLaneOrder(int laneCount)
    {
        var order = new List<int>();
        var centre = (laneCount + 1) / 2;
        order.Add(centre);

        for (var offset = 1; order.Count < laneCount; offset++)
        {
            if (centre + offset <= laneCount)
                order.Add(centre + offset);
            if (centre - offset >= 1 && order.Count < laneCount)
                order.Add(centre - offset);
        }

        return order;
    }

    private static void ApplyCentreLaneSeeding(Heat heat)
    {
        var entries = heat.Entries.ToList();
        if (entries.Count == 0) return;

        var laneOrder = GetCentreLaneOrder(entries.Count);

        // Remove all entries and re-add with new lane assignments
        var entryList = entries.ToList();
        foreach (var entry in entryList)
            heat.RemoveEntry(entry.Id);

        for (var i = 0; i < entryList.Count; i++)
            heat.AssignEntry(entryList[i], laneOrder[i]);
    }

    private static void SnakeSeed(List<Entry> seededEntries, List<Heat> heats)
    {
        var heatCount = heats.Count;
        for (var i = 0; i < seededEntries.Count; i++)
        {
            // Snake: 0→A, 1→B, 2→B, 3→A, 4→A, 5→B...
            var cycle = i / heatCount;
            var pos = i % heatCount;
            var heatIndex = cycle % 2 == 0 ? pos : heatCount - 1 - pos;

            heats[heatIndex].AssignEntry(seededEntries[i], heats[heatIndex].Entries.Count + 1);
        }
    }

    private static void ReassignLanes(Heat heat)
    {
        var entries = heat.Entries.ToList();
        foreach (var entry in entries)
            heat.RemoveEntry(entry.Id);

        for (var i = 0; i < entries.Count; i++)
            heat.AssignEntry(entries[i], i + 1);
    }
}
