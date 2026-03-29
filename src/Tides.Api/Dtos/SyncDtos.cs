namespace Tides.Api.Dtos;

public record SyncBatchRequest(
    Guid CarnivalId,
    string DeviceId,
    string RecorderId,
    List<SyncEventRequest> Events);

public record SyncEventRequest(
    Guid EventId,
    Guid HeatId,
    Guid EntryId,
    int? Placing,
    TimeSpan? Time,
    decimal? JudgeScore,
    string Status,
    DateTime DeviceTimestamp,
    int ExpectedHeatVersion,
    Guid? SupersedesEventId = null);

public record SyncBatchResponse(
    List<SyncEventAck> Acknowledged,
    List<SyncEventConflict> Conflicts,
    List<SyncEventError> Errors);

public record SyncEventAck(
    Guid EventId,
    int NewHeatVersion);

public record SyncEventConflict(
    Guid EventId,
    int ExpectedVersion,
    int ActualVersion,
    List<ResultResponse> CurrentResults);

public record SyncEventError(
    Guid EventId,
    string ErrorCode,
    string Message);
