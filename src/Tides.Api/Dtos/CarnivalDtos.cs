namespace Tides.Api.Dtos;

public record CreateCarnivalRequest(
    string Name,
    Guid HostingClubId,
    string Sanction,
    DateOnly StartDate,
    DateOnly EndDate,
    List<CreateEventRequest>? Events);

public record CreateEventRequest(
    string Name,
    string Category,
    string AgeGroup,
    string Gender,
    int MaxLanes,
    string AdvancementRule = "TopNPerHeat",
    int AdvanceTopN = 3,
    int AdvanceFastestN = 0);

public record CarnivalResponse(
    Guid Id,
    string Name,
    Guid HostingClubId,
    string Sanction,
    DateOnly StartDate,
    DateOnly EndDate,
    List<EventSummaryResponse> Events);

public record CarnivalListItemResponse(
    Guid Id,
    string Name,
    string Sanction,
    DateOnly StartDate,
    DateOnly EndDate,
    int EventCount,
    bool HasResults);

public record EventSummaryResponse(
    Guid Id,
    string Name,
    string Category,
    string AgeGroup,
    string Gender,
    int MaxLanes,
    int RoundCount,
    bool HasResults);
