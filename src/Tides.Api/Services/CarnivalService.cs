using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Tides.Api.Dtos;
using Tides.Api.Exceptions;
using Tides.Api.Hubs;
using Tides.Core.Domain;
using Tides.Core.Domain.Enums;
using Tides.Core.Domain.ValueObjects;
using Tides.Core.Services;
using Tides.Infrastructure.Persistence;

namespace Tides.Api.Services;

public class CarnivalService(
    TidesDbContext db,
    IDrawGeneratorService drawService,
    IPointsCalculatorService pointsService,
    IHubContext<ResultsHub> hubContext) : ICarnivalService
{
    public async Task<List<CarnivalListItemResponse>> ListCarnivalsAsync()
    {
        var carnivals = await db.Carnivals
            .Include(c => c.Events)
                .ThenInclude(e => e.Rounds)
                    .ThenInclude(r => r.Heats)
                        .ThenInclude(h => h.Results)
            .AsNoTracking()
            .OrderByDescending(c => c.StartDate)
            .ToListAsync();

        return carnivals.Select(c => new CarnivalListItemResponse(
            c.Id, c.Name, c.Sanction.ToString(),
            c.StartDate, c.EndDate,
            c.Events.Count,
            c.Events.Any(e => e.Rounds.Any(r => r.Heats.Any(h => h.Results.Count > 0)))
        )).ToList();
    }

    public async Task<CarnivalResponse> CreateCarnivalAsync(CreateCarnivalRequest request)
    {
        var sanction = Enum.Parse<SanctionLevel>(request.Sanction, true);
        var carnival = new Carnival(Guid.NewGuid(), request.Name, request.HostingClubId,
            sanction, request.StartDate, request.EndDate);

        if (request.Events != null)
        {
            foreach (var e in request.Events)
            {
                var eventDef = new EventDefinition(Guid.NewGuid(), e.Name,
                    Enum.Parse<EventCategory>(e.Category, true),
                    Enum.Parse<AgeGroup>(e.AgeGroup, true),
                    Enum.Parse<Gender>(e.Gender, true),
                    e.MaxLanes,
                    Enum.Parse<AdvancementRule>(e.AdvancementRule, true),
                    e.AdvanceTopN, e.AdvanceFastestN);
                carnival.AddEvent(eventDef);
            }
        }

        db.Carnivals.Add(carnival);
        await db.SaveChangesAsync();

        return MapCarnival(carnival);
    }

    public async Task<CarnivalResponse> GetCarnivalAsync(Guid id)
    {
        var carnival = await db.Carnivals
            .Include(c => c.Events)
                .ThenInclude(e => e.Rounds)
                    .ThenInclude(r => r.Heats)
                        .ThenInclude(h => h.Results)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new NotFoundException($"Carnival {id} not found.");

        return MapCarnival(carnival);
    }

    public async Task<DrawResponse> GetDrawAsync(Guid carnivalId)
    {
        var carnival = await LoadCarnivalWithDrawAsync(carnivalId);
        return await MapDrawAsync(carnival);
    }

    public async Task<CarnivalResultsResponse> GetResultsAsync(Guid carnivalId, Guid? eventId = null)
    {
        var query = db.Carnivals
            .Include(c => c.Events)
                .ThenInclude(e => e.Rounds)
                    .ThenInclude(r => r.Heats)
                        .ThenInclude(h => h.Results)
            .Include(c => c.Events)
                .ThenInclude(e => e.Rounds)
                    .ThenInclude(r => r.Heats)
                        .ThenInclude(h => h.Entries)
            .AsNoTracking();

        var carnival = await query.FirstOrDefaultAsync(c => c.Id == carnivalId)
            ?? throw new NotFoundException($"Carnival {carnivalId} not found.");

        var events = carnival.Events.AsEnumerable();
        if (eventId.HasValue)
            events = events.Where(e => e.Id == eventId.Value);

        var memberIds = events.SelectMany(e => e.Rounds)
            .SelectMany(r => r.Heats)
            .SelectMany(h => h.Entries)
            .SelectMany(e => e.MemberIds)
            .Distinct().ToList();

        var members = await db.Members
            .Where(m => memberIds.Contains(m.Id))
            .AsNoTracking()
            .ToDictionaryAsync(m => m.Id);

        var clubIds = events.SelectMany(e => e.Rounds)
            .SelectMany(r => r.Heats)
            .SelectMany(h => h.Entries)
            .Select(e => e.ClubId)
            .Distinct().ToList();

        var clubs = await db.Clubs
            .Where(c => clubIds.Contains(c.Id))
            .AsNoTracking()
            .ToDictionaryAsync(c => c.Id);

        return new CarnivalResultsResponse(carnivalId, events.Select(e =>
            new EventResultsResponse(e.Id, e.Name, e.AgeGroup.ToString(), e.Gender.ToString(),
                e.Rounds.SelectMany(r => r.Heats.Select(h =>
                    new HeatResultsResponse(h.Id, h.HeatNumber, r.Type.ToString(), h.IsComplete,
                        h.Results.Select(res =>
                        {
                            var entry = h.Entries.FirstOrDefault(en => en.Id == res.EntryId);
                            return MapResult(res, entry, members, clubs);
                        }).ToList())
                )).ToList())
        ).ToList());
    }

    public async Task<LeaderboardResponse> GetLeaderboardAsync(Guid carnivalId, string? ageGroup = null)
    {
        var carnival = await db.Carnivals
            .Include(c => c.Events)
                .ThenInclude(e => e.Rounds)
                    .ThenInclude(r => r.Heats)
                        .ThenInclude(h => h.Results)
            .Include(c => c.Events)
                .ThenInclude(e => e.Rounds)
                    .ThenInclude(r => r.Heats)
                        .ThenInclude(h => h.Entries)
            .Include(c => c.PointsTable)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == carnivalId)
            ?? throw new NotFoundException($"Carnival {carnivalId} not found.");

        if (carnival.PointsTable == null)
            return new LeaderboardResponse(carnivalId, []);

        var allocations = new List<PointsAllocation>();
        foreach (var evt in carnival.Events)
        {
            if (ageGroup != null && evt.AgeGroup.ToString() != ageGroup)
                continue;

            var finalRound = evt.Rounds.OrderByDescending(r => r.RoundNumber).FirstOrDefault();
            if (finalRound == null) continue;

            var results = finalRound.Heats.SelectMany(h => h.Results).ToList();
            if (results.Count == 0) continue;

            var entryToClub = finalRound.Heats
                .SelectMany(h => h.Entries)
                .ToDictionary(e => e.Id, e => e.ClubId);

            var allocation = pointsService.CalculatePoints(
                evt.Id, evt.AgeGroup, results, entryToClub, carnival.PointsTable);
            allocations.Add(allocation);
        }

        var ledger = pointsService.AggregateClubPoints(allocations);
        var standings = ledger.GetStandings();

        var clubIds = standings.Select(s => s.ClubId).ToList();
        var clubs = await db.Clubs
            .Where(c => clubIds.Contains(c.Id))
            .AsNoTracking()
            .ToDictionaryAsync(c => c.Id);

        var rank = 0;
        return new LeaderboardResponse(carnivalId, standings.Select(s =>
        {
            rank++;
            clubs.TryGetValue(s.ClubId, out var club);
            return new ClubStandingResponse(rank, s.ClubId,
                club?.Name ?? "Unknown", club?.Abbreviation ?? "?", s.Total.Value);
        }).ToList());
    }

    public async Task<HeatDrawResponse> GetHeatAsync(Guid carnivalId, Guid heatId)
    {
        var heat = await db.Heats
            .Include(h => h.Entries)
            .Include(h => h.Results)
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.Id == heatId)
            ?? throw new NotFoundException($"Heat {heatId} not found.");

        var memberIds = heat.Entries.SelectMany(e => e.MemberIds).Distinct().ToList();
        var members = await db.Members.Where(m => memberIds.Contains(m.Id))
            .AsNoTracking().ToDictionaryAsync(m => m.Id);
        var clubIds = heat.Entries.Select(e => e.ClubId).Distinct().ToList();
        var clubs = await db.Clubs.Where(c => clubIds.Contains(c.Id))
            .AsNoTracking().ToDictionaryAsync(c => c.Id);

        return MapHeat(heat, members, clubs);
    }

    public async Task<DrawResponse> GenerateDrawAsync(Guid carnivalId, GenerateDrawRequest request)
    {
        var carnival = await db.Carnivals
            .Include(c => c.Events)
                .ThenInclude(e => e.Rounds)
                    .ThenInclude(r => r.Heats)
            .FirstOrDefaultAsync(c => c.Id == carnivalId)
            ?? throw new NotFoundException($"Carnival {carnivalId} not found.");

        var eventDef = carnival.Events.FirstOrDefault(e => e.Id == request.EventId)
            ?? throw new NotFoundException($"Event {request.EventId} not found.");

        var roundType = Enum.Parse<RoundType>(request.RoundType, true);

        // Get unassigned entries for this event
        var entries = await db.Entries
            .Where(e => e.EventDefinitionId == request.EventId && !e.IsWithdrawn)
            .ToListAsync();

        if (entries.Count == 0)
            throw new InvalidOperationException("No entries to draw.");

        // Create untracked copies for the draw service (it mutates entries)
        var entryCopies = entries.Select(e =>
            new Entry(e.Id, e.EventDefinitionId, e.ClubId, e.MemberIds.ToList())).ToList();

        var drawResult = drawService.GenerateHeats(entryCopies, eventDef.MaxLanes, roundType);
        var round = eventDef.AddRound(roundType);

        // Capture assignments from the draw result
        var assignments = drawResult.Heats.SelectMany(h =>
            h.Entries.Select(e => (EntryId: e.Id, HeatId: h.Id, Lane: e.Lane!.Value))).ToList();

        // Create heats on the round — explicitly add to context as new entities
        foreach (var drawHeat in drawResult.Heats)
        {
            var heat = new Heat(drawHeat.Id, drawHeat.HeatNumber);
            round.AddHeat(heat);
            db.Entry(heat).State = Microsoft.EntityFrameworkCore.EntityState.Added;
        }
        db.Entry(round).State = Microsoft.EntityFrameworkCore.EntityState.Added;

        await db.SaveChangesAsync();

        // Now assign tracked entries to the persisted heats
        var entriesById = entries.ToDictionary(e => e.Id);
        foreach (var (entryId, heatId, lane) in assignments)
        {
            if (entriesById.TryGetValue(entryId, out var entry))
            {
                entry.AssignToHeat(heatId);
                entry.AssignLane(lane);
            }
        }

        await db.SaveChangesAsync();

        return await MapDrawAsync(carnival);
    }

    public async Task<EntryResponse> CreateEntryAsync(Guid carnivalId, CreateEntryRequest request)
    {
        var carnival = await db.Carnivals
            .Include(c => c.Events)
            .FirstOrDefaultAsync(c => c.Id == carnivalId)
            ?? throw new NotFoundException($"Carnival {carnivalId} not found.");

        var eventDef = carnival.Events.FirstOrDefault(e => e.Id == request.EventId)
            ?? throw new NotFoundException($"Event {request.EventId} not found.");

        var entry = new Entry(Guid.NewGuid(), request.EventId, request.ClubId, request.MemberIds);
        db.Entries.Add(entry);
        await db.SaveChangesAsync();

        var members = await db.Members.Where(m => request.MemberIds.Contains(m.Id))
            .AsNoTracking().ToDictionaryAsync(m => m.Id);
        var club = await db.Clubs.AsNoTracking().FirstOrDefaultAsync(c => c.Id == request.ClubId);

        return new EntryResponse(entry.Id, entry.EventDefinitionId, entry.ClubId,
            club?.Name ?? "Unknown",
            request.MemberIds.Select(id => members.TryGetValue(id, out var m)
                ? new MemberBriefResponse(m.Id, m.FirstName, m.LastName)
                : new MemberBriefResponse(id, "Unknown", "")).ToList(),
            entry.Lane, entry.IsWithdrawn);
    }

    public async Task WithdrawEntryAsync(Guid carnivalId, Guid entryId)
    {
        var entry = await db.Entries.FirstOrDefaultAsync(e => e.Id == entryId)
            ?? throw new NotFoundException($"Entry {entryId} not found.");
        entry.Withdraw();
        await db.SaveChangesAsync();
    }

    public async Task<ResultResponse> RecordResultAsync(Guid carnivalId, RecordResultRequest request)
    {
        var carnival = await LoadCarnivalAggregateAsync(carnivalId);

        var placing = request.Placing.HasValue ? new Placing(request.Placing.Value) : (Placing?)null;
        var time = request.Time.HasValue ? new TimeResult(request.Time.Value) : (TimeResult?)null;
        var status = Enum.Parse<ResultStatus>(request.Status, true);

        var result = new Result(Guid.NewGuid(), request.EntryId, placing, time,
            request.JudgeScore, status);

        carnival.RecordResult(request.EventId, request.RoundId, request.HeatId, result);
        await db.SaveChangesAsync();

        // Load entry for response
        var entry = await db.Entries.AsNoTracking().FirstOrDefaultAsync(e => e.Id == request.EntryId);
        var members = entry != null
            ? await db.Members.Where(m => entry.MemberIds.Contains(m.Id)).AsNoTracking().ToDictionaryAsync(m => m.Id)
            : new Dictionary<Guid, Member>();
        var clubs = entry != null
            ? await db.Clubs.Where(c => c.Id == entry.ClubId).AsNoTracking().ToDictionaryAsync(c => c.Id)
            : new Dictionary<Guid, Club>();

        var response = MapResult(result, entry, members, clubs);

        // Broadcast via SignalR
        await hubContext.Clients.Group($"carnival-{carnivalId}")
            .SendAsync("ResultRecorded", response);

        return response;
    }

    public async Task<ResultResponse> CorrectResultAsync(Guid resultId, CorrectResultRequest request)
    {
        var result = await db.Results.FirstOrDefaultAsync(r => r.Id == resultId)
            ?? throw new NotFoundException($"Result {resultId} not found.");

        var placing = request.NewPlacing.HasValue ? new Placing(request.NewPlacing.Value) : (Placing?)null;
        var time = request.NewTime.HasValue ? new TimeResult(request.NewTime.Value) : (TimeResult?)null;

        result.Correct(placing, time, request.Reason, "system");
        await db.SaveChangesAsync();

        var entry = await db.Entries.AsNoTracking().FirstOrDefaultAsync(e => e.Id == result.EntryId);
        var members = entry != null
            ? await db.Members.Where(m => entry.MemberIds.Contains(m.Id)).AsNoTracking().ToDictionaryAsync(m => m.Id)
            : new Dictionary<Guid, Member>();
        var clubs = entry != null
            ? await db.Clubs.Where(c => c.Id == entry.ClubId).AsNoTracking().ToDictionaryAsync(c => c.Id)
            : new Dictionary<Guid, Club>();

        var response = MapResult(result, entry, members, clubs);

        // Find carnival ID for SignalR broadcast
        var heat = await db.Heats
            .Include(h => h.Entries)
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.Id == result.HeatId);

        if (heat != null)
        {
            var round = await db.Rounds.AsNoTracking().FirstOrDefaultAsync(r => r.Id == heat.RoundId);
            if (round != null)
            {
                var evt = await db.Events.AsNoTracking().FirstOrDefaultAsync(e => e.Id == round.EventDefinitionId);
                if (evt != null)
                {
                    await hubContext.Clients.Group($"carnival-{evt.CarnivalId}")
                        .SendAsync("ResultCorrected", response);
                }
            }
        }

        return response;
    }

    public async Task DeleteResultAsync(Guid resultId)
    {
        var result = await db.Results.FirstOrDefaultAsync(r => r.Id == resultId)
            ?? throw new NotFoundException($"Result {resultId} not found.");

        db.Results.Remove(result);
        await db.SaveChangesAsync();
    }

    public async Task<ProtestResponse> LodgeProtestAsync(Guid resultId, LodgeProtestRequest request)
    {
        var result = await db.Results.FirstOrDefaultAsync(r => r.Id == resultId)
            ?? throw new NotFoundException($"Result {resultId} not found.");

        // Find the carnival through the result chain
        var heat = await db.Heats.AsNoTracking().FirstOrDefaultAsync(h => h.Id == result.HeatId);
        var round = heat != null ? await db.Rounds.AsNoTracking().FirstOrDefaultAsync(r => r.Id == heat.RoundId) : null;
        var evt = round != null ? await db.Events.AsNoTracking().FirstOrDefaultAsync(e => e.Id == round.EventDefinitionId) : null;

        if (evt == null) throw new InvalidOperationException("Cannot find carnival for this result.");

        var carnival = await LoadCarnivalAggregateAsync(evt.CarnivalId);
        var protest = carnival.LodgeProtest(request.EventId, request.HeatId, request.LodgedByClubId, request.Reason);
        await db.SaveChangesAsync();

        return MapProtest(protest);
    }

    public async Task<ProtestResponse> AdjudicateProtestAsync(Guid protestId, AdjudicateProtestRequest request)
    {
        var protest = await db.Protests.FirstOrDefaultAsync(p => p.Id == protestId)
            ?? throw new NotFoundException($"Protest {protestId} not found.");

        var carnival = await LoadCarnivalAggregateAsync(protest.CarnivalId);
        var outcome = Enum.Parse<ProtestStatus>(request.Outcome, true);
        carnival.AdjudicateProtest(protestId, outcome, request.Reason);
        await db.SaveChangesAsync();

        return MapProtest(protest);
    }

    // --- Private helpers ---

    private async Task<Carnival> LoadCarnivalAggregateAsync(Guid id)
    {
        return await db.Carnivals
            .Include(c => c.Events)
                .ThenInclude(e => e.Rounds)
                    .ThenInclude(r => r.Heats)
                        .ThenInclude(h => h.Entries)
            .Include(c => c.Events)
                .ThenInclude(e => e.Rounds)
                    .ThenInclude(r => r.Heats)
                        .ThenInclude(h => h.Results)
            .Include(c => c.Protests)
            .Include(c => c.PointsTable)
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new NotFoundException($"Carnival {id} not found.");
    }

    private async Task<Carnival> LoadCarnivalWithDrawAsync(Guid id)
    {
        return await db.Carnivals
            .Include(c => c.Events)
                .ThenInclude(e => e.Rounds)
                    .ThenInclude(r => r.Heats)
                        .ThenInclude(h => h.Entries)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new NotFoundException($"Carnival {id} not found.");
    }

    private static CarnivalResponse MapCarnival(Carnival c) => new(
        c.Id, c.Name, c.HostingClubId, c.Sanction.ToString(),
        c.StartDate, c.EndDate,
        c.Events.Select(e => new EventSummaryResponse(
            e.Id, e.Name, e.Category.ToString(), e.AgeGroup.ToString(),
            e.Gender.ToString(), e.MaxLanes, e.Rounds.Count,
            e.Rounds.Any(r => r.Heats.Any(h => h.Results.Count > 0))
        )).ToList());

    private async Task<DrawResponse> MapDrawAsync(Carnival carnival)
    {
        var allMemberIds = carnival.Events
            .SelectMany(e => e.Rounds)
            .SelectMany(r => r.Heats)
            .SelectMany(h => h.Entries)
            .SelectMany(e => e.MemberIds)
            .Distinct().ToList();

        var members = await db.Members.Where(m => allMemberIds.Contains(m.Id))
            .AsNoTracking().ToDictionaryAsync(m => m.Id);

        var allClubIds = carnival.Events
            .SelectMany(e => e.Rounds)
            .SelectMany(r => r.Heats)
            .SelectMany(h => h.Entries)
            .Select(e => e.ClubId)
            .Distinct().ToList();

        var clubs = await db.Clubs.Where(c => allClubIds.Contains(c.Id))
            .AsNoTracking().ToDictionaryAsync(c => c.Id);

        return new DrawResponse(carnival.Id, carnival.Events.Select(e =>
            new EventDrawResponse(e.Id, e.Name,
                e.Rounds.Select(r => new RoundDrawResponse(
                    r.Id, r.Type.ToString(), r.RoundNumber,
                    r.Heats.Select(h => MapHeat(h, members, clubs)).ToList()
                )).ToList())
        ).ToList());
    }

    private static HeatDrawResponse MapHeat(Heat h,
        Dictionary<Guid, Member> members, Dictionary<Guid, Club> clubs) =>
        new(h.Id, h.HeatNumber, h.IsComplete,
            h.Entries.Select(e => new LaneEntryResponse(
                e.Id, e.Lane, e.ClubId,
                clubs.TryGetValue(e.ClubId, out var club) ? club.Name : "Unknown",
                e.MemberIds.Select(id => members.TryGetValue(id, out var m)
                    ? new MemberBriefResponse(m.Id, m.FirstName, m.LastName)
                    : new MemberBriefResponse(id, "Unknown", "")).ToList(),
                e.IsWithdrawn
            )).ToList());

    private static ResultResponse MapResult(Result r, Entry? entry,
        Dictionary<Guid, Member> members, Dictionary<Guid, Club> clubs) =>
        new(r.Id, r.HeatId, r.EntryId, r.Placing?.Position, r.Time?.Time,
            r.JudgeScore, r.Status.ToString(),
            entry?.ClubId ?? Guid.Empty,
            entry != null && clubs.TryGetValue(entry.ClubId, out var club) ? club.Name : "Unknown",
            entry?.MemberIds.Select(id => members.TryGetValue(id, out var m)
                ? new MemberBriefResponse(m.Id, m.FirstName, m.LastName)
                : new MemberBriefResponse(id, "Unknown", "")).ToList() ?? []);

    private static ProtestResponse MapProtest(Protest p) =>
        new(p.Id, p.CarnivalId, p.EventId, p.HeatId, p.LodgedByClubId,
            p.Reason, p.Status.ToString(), p.AdjudicationReason,
            p.LodgedAt, p.AdjudicatedAt);
}
