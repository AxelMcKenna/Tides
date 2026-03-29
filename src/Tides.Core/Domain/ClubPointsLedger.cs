using Tides.Core.Domain.Enums;
using Tides.Core.Domain.ValueObjects;

namespace Tides.Core.Domain;

public class ClubPointsLedger
{
    private readonly Dictionary<Guid, Points> _overallTotals = new();
    private readonly Dictionary<(Guid ClubId, AgeGroup AgeGroup), Points> _byAgeGroup = new();

    public IReadOnlyDictionary<Guid, Points> OverallTotals => _overallTotals;
    public IReadOnlyDictionary<(Guid ClubId, AgeGroup AgeGroup), Points> ByAgeGroup => _byAgeGroup;

    public void Credit(Guid clubId, AgeGroup ageGroup, Points points)
    {
        _overallTotals[clubId] = _overallTotals.GetValueOrDefault(clubId) + points;
        var key = (clubId, ageGroup);
        _byAgeGroup[key] = _byAgeGroup.GetValueOrDefault(key) + points;
    }

    public void Debit(Guid clubId, AgeGroup ageGroup, Points points)
    {
        _overallTotals[clubId] = _overallTotals.GetValueOrDefault(clubId) - points;
        var key = (clubId, ageGroup);
        _byAgeGroup[key] = _byAgeGroup.GetValueOrDefault(key) - points;
    }

    public List<(Guid ClubId, Points Total)> GetStandings()
    {
        return _overallTotals
            .OrderByDescending(kv => kv.Value.Value)
            .Select(kv => (kv.Key, kv.Value))
            .ToList();
    }

    public List<(Guid ClubId, Points Total)> GetStandingsByAgeGroup(AgeGroup ageGroup)
    {
        return _byAgeGroup
            .Where(kv => kv.Key.AgeGroup == ageGroup)
            .OrderByDescending(kv => kv.Value.Value)
            .Select(kv => (kv.Key.ClubId, kv.Value))
            .ToList();
    }
}
