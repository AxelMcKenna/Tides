namespace Tides.Api.Dtos;

public record RecordResultRequest(
    Guid EventId,
    Guid RoundId,
    Guid HeatId,
    Guid EntryId,
    int? Placing,
    TimeSpan? Time,
    decimal? JudgeScore,
    string Status = "Provisional");

public record CorrectResultRequest(
    int? NewPlacing,
    TimeSpan? NewTime,
    string Reason);

public record ResultResponse(
    Guid Id,
    Guid HeatId,
    Guid EntryId,
    int? Placing,
    TimeSpan? Time,
    decimal? JudgeScore,
    string Status,
    Guid ClubId,
    string ClubName,
    List<MemberBriefResponse> Members,
    decimal? Points = null);

public record CarnivalResultsResponse(
    Guid CarnivalId,
    List<EventResultsResponse> Events);

public record EventResultsResponse(
    Guid EventId,
    string EventName,
    string AgeGroup,
    string Gender,
    List<HeatResultsResponse> Heats);

public record HeatResultsResponse(
    Guid HeatId,
    int HeatNumber,
    string RoundType,
    bool IsComplete,
    DateTime? CompletedAt,
    List<ResultResponse> Results);
