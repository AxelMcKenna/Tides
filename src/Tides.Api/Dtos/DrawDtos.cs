namespace Tides.Api.Dtos;

public record GenerateDrawRequest(Guid EventId, string RoundType);

public record DrawResponse(
    Guid CarnivalId,
    List<EventDrawResponse> Events);

public record EventDrawResponse(
    Guid EventId,
    string EventName,
    List<RoundDrawResponse> Rounds);

public record RoundDrawResponse(
    Guid RoundId,
    string RoundType,
    int RoundNumber,
    List<HeatDrawResponse> Heats);

public record HeatDrawResponse(
    Guid HeatId,
    int HeatNumber,
    bool IsComplete,
    List<LaneEntryResponse> Entries);

public record LaneEntryResponse(
    Guid EntryId,
    int? Lane,
    Guid ClubId,
    string ClubName,
    List<MemberBriefResponse> Members,
    bool IsWithdrawn);

public record MemberBriefResponse(Guid Id, string FirstName, string LastName);
