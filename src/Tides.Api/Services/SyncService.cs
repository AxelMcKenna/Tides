using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Tides.Api.Dtos;
using Tides.Api.Hubs;
using Tides.Core.Domain;
using Tides.Core.Domain.Enums;
using Tides.Core.Domain.ValueObjects;
using Tides.Infrastructure.Persistence;

namespace Tides.Api.Services;

public class SyncService(TidesDbContext db, IHubContext<ResultsHub> hubContext) : ISyncService
{
    public async Task<SyncBatchResponse> ProcessBatchAsync(SyncBatchRequest request)
    {
        var acknowledged = new List<SyncEventAck>();
        var conflicts = new List<SyncEventConflict>();
        var errors = new List<SyncEventError>();
        var broadcastResults = new List<ResultResponse>();

        // Pre-load all heats referenced in the batch (with entries and results)
        var heatIds = request.Events.Select(e => e.HeatId).Distinct().ToList();
        var heats = await db.Heats
            .Include(h => h.Entries)
            .Include(h => h.Results)
            .Where(h => heatIds.Contains(h.Id))
            .ToDictionaryAsync(h => h.Id);

        // Pre-load existing event IDs for dedup check
        var incomingEventIds = request.Events.Select(e => e.EventId).ToList();
        var existingEventIds = await db.ResultEvents
            .Where(e => incomingEventIds.Contains(e.EventId))
            .Select(e => e.EventId)
            .ToHashSetAsync();

        foreach (var syncEvent in request.Events)
        {
            // 1. Dedup — already processed
            if (existingEventIds.Contains(syncEvent.EventId))
            {
                var heatVersion = heats.TryGetValue(syncEvent.HeatId, out var h) ? h.Version : 0;
                acknowledged.Add(new SyncEventAck(syncEvent.EventId, heatVersion));
                continue;
            }

            // 2. Load heat
            if (!heats.TryGetValue(syncEvent.HeatId, out var heat))
            {
                errors.Add(new SyncEventError(syncEvent.EventId, "HEAT_NOT_FOUND",
                    $"Heat {syncEvent.HeatId} not found."));
                continue;
            }

            // 3. Version check
            if (heat.Version != syncEvent.ExpectedHeatVersion)
            {
                var currentResults = MapHeatResults(heat);
                conflicts.Add(new SyncEventConflict(
                    syncEvent.EventId,
                    syncEvent.ExpectedHeatVersion,
                    heat.Version,
                    currentResults));
                continue;
            }

            // 4. Supersession — handle corrections
            if (syncEvent.SupersedesEventId.HasValue)
            {
                var originalEvent = await db.ResultEvents
                    .FirstOrDefaultAsync(e => e.EventId == syncEvent.SupersedesEventId.Value);

                if (originalEvent != null)
                {
                    originalEvent.Supersede(syncEvent.EventId);

                    // Find and correct the existing result
                    var existingResult = heat.Results
                        .FirstOrDefault(r => r.EntryId == syncEvent.EntryId);

                    if (existingResult != null)
                    {
                        var newPlacing = syncEvent.Placing.HasValue
                            ? new Placing(syncEvent.Placing.Value) : (Placing?)null;
                        var newTime = syncEvent.Time.HasValue
                            ? new TimeResult(syncEvent.Time.Value) : (TimeResult?)null;

                        existingResult.Correct(newPlacing, newTime,
                            $"Superseded by sync event {syncEvent.EventId}", request.RecorderId);

                        heat.IncrementVersion();
                        AppendEventLog(syncEvent, request);
                        existingEventIds.Add(syncEvent.EventId);
                        acknowledged.Add(new SyncEventAck(syncEvent.EventId, heat.Version));
                        continue;
                    }
                }
            }

            // 5. Apply — new result
            try
            {
                var placing = syncEvent.Placing.HasValue
                    ? new Placing(syncEvent.Placing.Value) : (Placing?)null;
                var time = syncEvent.Time.HasValue
                    ? new TimeResult(syncEvent.Time.Value) : (TimeResult?)null;
                var status = Enum.Parse<ResultStatus>(syncEvent.Status, true);

                var result = new Result(Guid.NewGuid(), syncEvent.EntryId,
                    placing, time, syncEvent.JudgeScore, status);

                heat.RecordResult(result);

                // Build response for SignalR broadcast
                var entry = heat.Entries.FirstOrDefault(e => e.Id == syncEvent.EntryId);
                broadcastResults.Add(new ResultResponse(
                    result.Id, result.HeatId, result.EntryId,
                    result.Placing?.Position, result.Time?.Time,
                    result.JudgeScore, result.Status.ToString(),
                    entry?.ClubId ?? Guid.Empty, "", [], null));
            }
            catch (InvalidOperationException ex)
            {
                errors.Add(new SyncEventError(syncEvent.EventId, "INVALID_OPERATION", ex.Message));
                continue;
            }

            // 6. Append to event log
            AppendEventLog(syncEvent, request);
            existingEventIds.Add(syncEvent.EventId);

            // 7. Acknowledge
            acknowledged.Add(new SyncEventAck(syncEvent.EventId, heat.Version));
        }

        // Save all changes in a single transaction
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            // Another request modified a heat concurrently — convert affected events to conflicts
            return await HandleConcurrencyConflict(request);
        }

        // Broadcast via SignalR after successful save
        foreach (var result in broadcastResults)
        {
            await hubContext.Clients.Group($"carnival-{request.CarnivalId}")
                .SendAsync("ResultRecorded", result);
        }

        return new SyncBatchResponse(acknowledged, conflicts, errors);
    }

    private void AppendEventLog(SyncEventRequest syncEvent, SyncBatchRequest batch)
    {
        var resultEvent = new ResultEvent(
            Guid.NewGuid(),
            syncEvent.EventId,
            batch.CarnivalId,
            syncEvent.HeatId,
            syncEvent.EntryId,
            syncEvent.Placing,
            syncEvent.Time,
            syncEvent.JudgeScore,
            syncEvent.Status,
            batch.DeviceId,
            batch.RecorderId,
            syncEvent.DeviceTimestamp,
            syncEvent.ExpectedHeatVersion);

        db.ResultEvents.Add(resultEvent);
    }

    private async Task<SyncBatchResponse> HandleConcurrencyConflict(SyncBatchRequest request)
    {
        // Reload heats and return all events as conflicts
        var conflicts = new List<SyncEventConflict>();

        foreach (var syncEvent in request.Events)
        {
            var heat = await db.Heats
                .Include(h => h.Results)
                .Include(h => h.Entries)
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.Id == syncEvent.HeatId);

            if (heat != null)
            {
                conflicts.Add(new SyncEventConflict(
                    syncEvent.EventId,
                    syncEvent.ExpectedHeatVersion,
                    heat.Version,
                    MapHeatResults(heat)));
            }
        }

        return new SyncBatchResponse([], conflicts, []);
    }

    private static List<ResultResponse> MapHeatResults(Heat heat)
    {
        return heat.Results.Select(r =>
        {
            var entry = heat.Entries.FirstOrDefault(e => e.Id == r.EntryId);
            return new ResultResponse(
                r.Id, r.HeatId, r.EntryId,
                r.Placing?.Position, r.Time?.Time,
                r.JudgeScore, r.Status.ToString(),
                entry?.ClubId ?? Guid.Empty, "", [], null);
        }).ToList();
    }
}
