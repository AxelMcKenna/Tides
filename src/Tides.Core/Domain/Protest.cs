using Tides.Core.Domain.Enums;

namespace Tides.Core.Domain;

public class Protest
{
    public Guid Id { get; private set; }
    public Guid CarnivalId { get; private set; }
    public Guid EventId { get; private set; }
    public Guid? HeatId { get; private set; }
    public Guid LodgedByClubId { get; private set; }
    public string Reason { get; private set; } = null!;
    public ProtestStatus Status { get; private set; }
    public string? AdjudicationReason { get; private set; }
    public DateTime LodgedAt { get; private set; }
    public DateTime? AdjudicatedAt { get; private set; }

    private Protest() { }

    public Protest(Guid id, Guid eventId, Guid? heatId, Guid lodgedByClubId, string reason)
    {
        Id = id;
        EventId = eventId;
        HeatId = heatId;
        LodgedByClubId = lodgedByClubId;
        Reason = reason;
        Status = ProtestStatus.Lodged;
        LodgedAt = DateTime.UtcNow;
    }

    public void Adjudicate(ProtestStatus outcome, string reason)
    {
        if (outcome is not (ProtestStatus.Upheld or ProtestStatus.Dismissed))
            throw new ArgumentException("Outcome must be Upheld or Dismissed.", nameof(outcome));

        Status = outcome;
        AdjudicationReason = reason;
        AdjudicatedAt = DateTime.UtcNow;
    }
}
