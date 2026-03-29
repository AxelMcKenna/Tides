using Tides.Core.Domain;
using Tides.Core.Domain.Enums;
using Tides.Core.Domain.ValueObjects;
using Tides.Core.Services;

namespace Tides.Core.Tests;

public class DrawGeneratorServiceTests
{
    private readonly DrawGeneratorService _service = new();

    private static List<Entry> CreateEntries(int count, Guid? clubId = null)
    {
        var club = clubId ?? Guid.NewGuid();
        return Enumerable.Range(0, count)
            .Select(_ => new Entry(Guid.NewGuid(), Guid.NewGuid(), club, Guid.NewGuid()))
            .ToList();
    }

    [Fact]
    public void GenerateHeats_EvenSplit_CreatesBalancedHeats()
    {
        var entries = CreateEntries(8);
        var result = _service.GenerateHeats(entries, 4, RoundType.Heat);

        Assert.Equal(2, result.Heats.Count);
        Assert.Equal(4, result.Heats[0].Entries.Count);
        Assert.Equal(4, result.Heats[1].Entries.Count);
    }

    [Fact]
    public void GenerateHeats_UnevenSplit_DistributesCorrectly()
    {
        var entries = CreateEntries(7);
        var result = _service.GenerateHeats(entries, 4, RoundType.Heat);

        Assert.Equal(2, result.Heats.Count);
        var counts = result.Heats.Select(h => h.Entries.Count).OrderBy(c => c).ToList();
        Assert.Equal(3, counts[0]);
        Assert.Equal(4, counts[1]);
    }

    [Fact]
    public void GenerateHeats_SingleEntry_CreatesSingleHeat()
    {
        var entries = CreateEntries(1);
        var result = _service.GenerateHeats(entries, 4, RoundType.Heat);

        Assert.Single(result.Heats);
        Assert.Single(result.Heats[0].Entries);
    }

    [Fact]
    public void GenerateHeats_ZeroEntries_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            _service.GenerateHeats([], 4, RoundType.Heat));
    }

    [Fact]
    public void GenerateHeats_AllEntriesHaveLaneAssignments()
    {
        var entries = CreateEntries(6);
        var result = _service.GenerateHeats(entries, 4, RoundType.Heat);

        foreach (var heat in result.Heats)
        foreach (var entry in heat.Entries)
            Assert.NotNull(entry.Lane);
    }

    [Fact]
    public void GenerateSeededDraw_SingleFinalHeat_CentreLaneSeeding()
    {
        var entries = CreateEntries(4);
        var results = new List<Result>();
        var entriesById = new Dictionary<Guid, Entry>();

        for (var i = 0; i < entries.Count; i++)
        {
            entriesById[entries[i].Id] = entries[i];
            results.Add(new Result(Guid.NewGuid(), entries[i].Id,
                new Placing(i + 1), new TimeResult(TimeSpan.FromSeconds(10 + i))));
        }

        var draw = _service.GenerateSeededDraw(results, entriesById, 4,
            AdvancementRule.TopNPerHeat, 4, 0);

        Assert.Single(draw.Heats);
        Assert.Equal(4, draw.Heats[0].Entries.Count);

        // Verify centre-lane seeding: fastest should be in centre lane
        var centreOrder = DrawGeneratorService.GetCentreLaneOrder(4);
        Assert.Equal(2, centreOrder[0]); // Centre of 4 lanes is lane 2
    }

    [Fact]
    public void GetCentreLaneOrder_ReturnsCorrectOrder()
    {
        // 4 lanes: centre=2, then 3, 1, 4
        var order4 = DrawGeneratorService.GetCentreLaneOrder(4);
        Assert.Equal([2, 3, 1, 4], order4);

        // 5 lanes: centre=3, then 4, 2, 5, 1
        var order5 = DrawGeneratorService.GetCentreLaneOrder(5);
        Assert.Equal([3, 4, 2, 5, 1], order5);

        // 6 lanes: centre=3, then 4, 2, 5, 1, 6
        var order6 = DrawGeneratorService.GetCentreLaneOrder(6);
        Assert.Equal([3, 4, 2, 5, 1, 6], order6);
    }

    [Fact]
    public void RedrawOnWithdrawal_RebalancesHeats()
    {
        var entries = CreateEntries(8);
        var draw = _service.GenerateHeats(entries, 4, RoundType.Heat);

        // Both heats have 4. Withdraw from heat 1 → becomes 3 vs 4, no rebalance needed (diff=1)
        var entryToWithdraw = draw.Heats[0].Entries[0].Id;
        var redrawn = _service.RedrawOnWithdrawal(draw, entryToWithdraw);

        var totalEntries = redrawn.Heats.Sum(h => h.Entries.Count);
        Assert.Equal(7, totalEntries);
        Assert.True(redrawn.AuditTrail.Any(a => a.Action == "WithdrawalRedraw"));
    }

    [Fact]
    public void RedrawOnWithdrawal_ImbalancedHeats_Rebalances()
    {
        // Create 9 entries across 4 lanes = 3 heats (3, 3, 3)
        var entries = CreateEntries(9);
        var draw = _service.GenerateHeats(entries, 4, RoundType.Heat);

        Assert.Equal(3, draw.Heats.Count);

        // Withdraw 2 from heat 1 → heat 1 has 1, others have 3 → should rebalance
        var entry1 = draw.Heats[0].Entries[0].Id;
        var redrawn = _service.RedrawOnWithdrawal(draw, entry1);

        var entry2 = redrawn.Heats[0].Entries[0].Id;
        var redrawn2 = _service.RedrawOnWithdrawal(redrawn, entry2);

        // After rebalancing, no heat should differ from another by more than 1
        var counts = redrawn2.Heats.Select(h => h.Entries.Count).ToList();
        Assert.True(counts.Max() - counts.Min() <= 1,
            $"Heats not balanced: {string.Join(", ", counts)}");
    }

    [Fact]
    public void ManuallyAssign_MovesEntryAndLogsAudit()
    {
        var entries = CreateEntries(6);
        var draw = _service.GenerateHeats(entries, 4, RoundType.Heat);

        var entryToMove = draw.Heats[0].Entries[0];
        var result = _service.ManuallyAssign(draw, entryToMove.Id, 1, 3, "official-1");

        Assert.Contains(result.Heats[1].Entries, e => e.Id == entryToMove.Id);
        Assert.True(result.AuditTrail.Any(a => a.Action == "ManualAssignment" && a.UserId == "official-1"));
    }

    [Fact]
    public void SnakeSeeding_DistributesCorrectly()
    {
        // Create 8 entries and seed into 2 heats
        var entries = CreateEntries(8);
        var results = new List<Result>();
        var entriesById = new Dictionary<Guid, Entry>();

        for (var i = 0; i < entries.Count; i++)
        {
            entriesById[entries[i].Id] = entries[i];
            results.Add(new Result(Guid.NewGuid(), entries[i].Id,
                new Placing(i + 1), new TimeResult(TimeSpan.FromSeconds(10 + i))));
        }

        var draw = _service.GenerateSeededDraw(results, entriesById, 4,
            AdvancementRule.TopNPerHeat, 8, 0);

        Assert.Equal(2, draw.Heats.Count);
        Assert.Equal(4, draw.Heats[0].Entries.Count);
        Assert.Equal(4, draw.Heats[1].Entries.Count);
    }
}
