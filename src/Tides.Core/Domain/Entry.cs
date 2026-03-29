namespace Tides.Core.Domain;

public class Entry
{
    public Guid Id { get; private set; }
    public Guid? HeatId { get; private set; }
    public Guid EventDefinitionId { get; private set; }
    public Guid ClubId { get; private set; }
    public List<Guid> MemberIds { get; private set; } = [];
    public int? Lane { get; private set; }
    public bool IsWithdrawn { get; private set; }

    private Entry() { }

    public Entry(Guid id, Guid eventDefinitionId, Guid clubId, List<Guid> memberIds)
    {
        Id = id;
        EventDefinitionId = eventDefinitionId;
        ClubId = clubId;
        MemberIds = memberIds;
    }

    /// <summary>Convenience constructor for individual (non-relay) entries.</summary>
    public Entry(Guid id, Guid eventDefinitionId, Guid clubId, Guid memberId)
        : this(id, eventDefinitionId, clubId, [memberId])
    {
    }

    public void AssignToHeat(Guid heatId)
    {
        HeatId = heatId;
    }

    public void AssignLane(int lane)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(lane, 1);
        Lane = lane;
    }

    public void Withdraw()
    {
        IsWithdrawn = true;
        Lane = null;
    }
}
