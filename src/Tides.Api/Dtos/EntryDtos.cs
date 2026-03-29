namespace Tides.Api.Dtos;

public record CreateEntryRequest(
    Guid EventId,
    Guid ClubId,
    List<Guid> MemberIds);

public record EntryResponse(
    Guid Id,
    Guid EventDefinitionId,
    Guid ClubId,
    string ClubName,
    List<MemberBriefResponse> Members,
    int? Lane,
    bool IsWithdrawn);
