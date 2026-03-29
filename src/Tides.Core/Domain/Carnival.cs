using Tides.Core.Domain.Enums;
using Tides.Core.Events;

namespace Tides.Core.Domain;

public class Carnival
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public Guid HostingClubId { get; private set; }
    public SanctionLevel Sanction { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }

    private readonly List<EventDefinition> _events = [];
    public IReadOnlyList<EventDefinition> Events => _events;

    public PointsTable? PointsTable { get; private set; }
    public ClubPointsLedger ClubPoints { get; private set; } = new();

    private readonly List<Protest> _protests = [];
    public IReadOnlyList<Protest> Protests => _protests;

    private readonly List<IDomainEvent> _domainEvents = [];
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents;

    private Carnival() { }

    public Carnival(Guid id, string name, Guid hostingClubId,
        SanctionLevel sanction, DateOnly startDate, DateOnly endDate)
    {
        Id = id;
        Name = name;
        HostingClubId = hostingClubId;
        Sanction = sanction;
        StartDate = startDate;
        EndDate = endDate;
    }

    public void SetPointsTable(PointsTable table)
    {
        PointsTable = table;
    }

    public void AddEvent(EventDefinition eventDef)
    {
        _events.Add(eventDef);
    }

    public void RemoveEvent(Guid eventId)
    {
        _events.RemoveAll(e => e.Id == eventId);
    }

    public void RecordResult(Guid eventId, Guid roundId, Guid heatId, Result result)
    {
        var evt = _events.FirstOrDefault(e => e.Id == eventId)
            ?? throw new InvalidOperationException($"Event {eventId} not found.");
        var round = evt.Rounds.FirstOrDefault(r => r.Id == roundId)
            ?? throw new InvalidOperationException($"Round {roundId} not found.");
        var heat = round.Heats.FirstOrDefault(h => h.Id == heatId)
            ?? throw new InvalidOperationException($"Heat {heatId} not found.");

        heat.RecordResult(result);

        _domainEvents.Add(new ResultEntered(Id, eventId, roundId, heatId, result.Id, DateTime.UtcNow));
    }

    public void CorrectResult(Guid eventId, Guid heatId, Result result,
        ValueObjects.Placing? newPlacing, ValueObjects.TimeResult? newTime, string reason, string userId)
    {
        result.Correct(newPlacing, newTime, reason, userId);

        _domainEvents.Add(new ResultCorrected(Id, eventId, heatId, result.Id, reason, DateTime.UtcNow));
    }

    public Protest LodgeProtest(Guid eventId, Guid? heatId, Guid lodgedByClubId, string reason)
    {
        var protest = new Protest(Guid.NewGuid(), eventId, heatId, lodgedByClubId, reason);
        _protests.Add(protest);
        return protest;
    }

    public void AdjudicateProtest(Guid protestId, ProtestStatus outcome, string reason)
    {
        var protest = _protests.FirstOrDefault(p => p.Id == protestId)
            ?? throw new InvalidOperationException($"Protest {protestId} not found.");

        protest.Adjudicate(outcome, reason);

        _domainEvents.Add(new ProtestAdjudicated(Id, protestId, outcome, reason, DateTime.UtcNow));
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
