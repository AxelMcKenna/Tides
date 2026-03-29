# Shore Sync — Failure Modes & Server Behaviour

How the `POST /api/sync` endpoint handles every failure scenario from the beach.

---

## Endpoint Contract

```
POST /api/sync
Content-Type: application/json

{
  "carnivalId": "uuid",
  "deviceId": "device-1",
  "recorderId": "recorder-1",
  "events": [ ... ]
}
```

**Response codes:**
- `200` — every event in the batch was acknowledged
- `207` — partial success. Some events acknowledged, some conflicted or errored. The response body contains per-event status.

The response always contains three arrays:

| Array | Meaning | Client action |
|---|---|---|
| `acknowledged` | Event processed (or already existed). Includes `newHeatVersion`. | Remove from sync queue |
| `conflicts` | Heat was modified by another recorder since this device last synced. Includes current server results. | Surface for human review |
| `errors` | Validation failure (heat not found, entry not in heat, entry withdrawn). | Move to failed queue, surface to secretary |

---

## Failure Scenarios

### 1. Network drops mid-sync (response lost)

**What happens:** The server receives and processes the batch. The device never gets the response. The device retries the full batch.

**Server behaviour:** Each event carries a device-assigned `eventId` (UUID). On retry, the server finds the `eventId` already exists in `result_events` (unique index) and returns it in `acknowledged` without re-processing. The `results` table is untouched.

**Guarantee:** No duplicate results. No duplicate event log entries. The unique index on `result_events.EventId` enforces this at the database level.

**Relevant code:** `SyncService.ProcessBatchAsync` — dedup check is the first step for each event.

---

### 2. Two recorders submit results for the same heat

**What happens:** iPad A and iPad B both record results for Heat 12 while offline. iPad A syncs first (heat version goes from 1 → 2). iPad B syncs with `expectedHeatVersion: 1`.

**Server behaviour:** The version check fails for iPad B's events. The server returns a `conflict` response containing:
- The expected version (1) and actual version (2)
- The current results on the server (iPad A's results)

The device surfaces both sets of results for a human to choose between.

**Guarantee:** No silent overwrites. A human always decides which results to keep.

**Relevant code:** `SyncService.ProcessBatchAsync` — version check is step 3. `Heat.Version` is configured as an EF Core concurrency token (`HeatConfiguration.cs`).

---

### 3. Two sync requests hit the server simultaneously for the same heat

**What happens:** Two HTTP requests arrive at the same time, both targeting the same heat with matching `expectedHeatVersion`.

**Server behaviour:** EF Core's concurrency token on `Heat.Version` causes a `DbUpdateConcurrencyException` on `SaveChangesAsync()` for the second request. The service catches this, reloads the heats from the database, and returns all events as `conflicts` with the updated heat state.

**Guarantee:** Database-level optimistic locking prevents both writes from succeeding. Exactly one wins.

**Relevant code:** `SyncService.HandleConcurrencyConflict` — reloads fresh state and converts to conflict responses.

---

### 4. Heat not found

**What happens:** Device sends a sync event referencing a `heatId` that doesn't exist on the server. Possible causes: stale carnival data on device, heat deleted and regenerated.

**Server behaviour:** Returns an `error` with code `HEAT_NOT_FOUND`. Other events in the same batch are unaffected and process normally.

**Guarantee:** Partial batch success. One bad event doesn't poison the batch.

---

### 5. Entry not in heat / entry withdrawn

**What happens:** Device submits a result for an entry that either isn't assigned to this heat or has been withdrawn since the device last synced.

**Server behaviour:** `Heat.RecordResult()` throws `InvalidOperationException`. The service catches it and returns an `error` with code `INVALID_OPERATION` and the exception message. Other events in the batch continue processing.

**Guarantee:** Domain invariants enforced. No results recorded for invalid entries.

---

### 6. Correction (supersession)

**What happens:** A secretary corrects a previously synced result. The device sends a new event with `supersedesEventId` pointing to the original event's `eventId`.

**Server behaviour:**
1. Finds the original `ResultEvent` and sets `supersededBy` to the new event's ID
2. Finds the existing `Result` for that entry and calls `Result.Correct()` — which adds an audit trail entry and sets status to `Corrected`
3. Increments the heat version
4. Appends the new event to the event log

**Guarantee:** Original event is preserved (append-only log). The correction is a new event. Full audit trail maintained on both `ResultEvent` and `Result`.

**Relevant code:** `SyncService.ProcessBatchAsync` — supersession is step 4. `Result.Correct()` in `Result.cs`.

---

### 7. Correction for an event that hasn't synced yet

**What happens:** Device sends a correction (`supersedesEventId`) but the original event hasn't been received by the server (e.g., it was in a failed batch).

**Server behaviour:** The `supersedesEventId` lookup returns null. The service falls through to step 5 (new result) and processes it as a fresh result. No error.

**Edge case:** If the original event later syncs, it will be deduplicated (its `eventId` might already have a result for this entry). The version check will catch this — the heat version will have advanced, so the late-arriving original event returns as a conflict.

---

### 8. Server crash mid-transaction

**What happens:** The server crashes after processing some events but before `SaveChangesAsync()` commits.

**Server behaviour:** The database transaction is rolled back. Nothing is persisted — no results, no event log entries.

**Client behaviour:** The device never receives a response, so it retries the full batch. All events process from scratch.

**Guarantee:** Atomicity. Either all events in the batch are committed or none are. There is no partial write to the database.

---

### 9. Duplicate result for same entry in same heat

**What happens:** A device sends two events for the same entry in the same heat within the same batch (bug or race condition).

**Server behaviour:** The first event processes normally via `Heat.RecordResult()`. The second event also calls `Heat.RecordResult()` — the result is added (the domain doesn't currently prevent multiple results per entry). Both are acknowledged.

**Note:** This is a valid scenario for corrections within the same batch. If it's unintentional, the conflict will be visible in the results and the secretary can correct it.

---

### 10. Invalid status string

**What happens:** Device sends `"status": "invalid_value"` that doesn't parse to a `ResultStatus` enum.

**Server behaviour:** `Enum.Parse<ResultStatus>()` throws `ArgumentException`, which propagates up. The global exception handler returns a 400 Bad Request.

**Note:** This fails the entire batch because it's a malformed request, not a per-event failure. The client should retry after fixing the payload.

---

### 11. Points become stale after sync

**What happens:** New results arrive via sync. The cached leaderboard points no longer reflect the current results.

**Server behaviour:** `PointsChecksumService` computes a deterministic SHA256 hash of all active (non-superseded) `ResultEvent` records. When the leaderboard is requested, the checksum of current events can be compared against the last computed checksum. If different, points are recalculated from the event log using the existing `PointsCalculatorService` (pure function).

**Guarantee:** Points are always derivable from the event log. A bug in the points calculator can be fixed and all points replayed from the immutable event history.

---

## Event Log Guarantees

The `result_events` table is append-only:

- **Never deleted.** Corrections are new events with `supersedesEventId` set.
- **Never mutated** except for `supersededBy` (set once, when a correction arrives).
- **Idempotent.** Unique index on `eventId` prevents duplicate entries.
- **Auditable.** Every event records `deviceId`, `recorderId`, `deviceTimestamp`, and `receivedAt`.

To reconstruct the current state of any heat, filter `result_events` for that heat where `supersededBy IS NULL`. These are the active events.

---

## Quick Reference: Error Codes

| Code | Meaning | Retryable? |
|---|---|---|
| `HEAT_NOT_FOUND` | Heat ID doesn't exist on server | No — device needs to re-sync carnival data |
| `INVALID_OPERATION` | Entry not in heat, or entry withdrawn | No — device needs to re-sync heat data |
| Version conflict (in `conflicts` array) | Another recorder modified this heat | No — human must review and resubmit |
