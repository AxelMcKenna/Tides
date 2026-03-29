namespace Tides.Api.Dtos;

public record LeaderboardResponse(
    Guid CarnivalId,
    List<ClubStandingResponse> Standings,
    string? Checksum = null);

public record ClubStandingResponse(
    int Rank,
    Guid ClubId,
    string ClubName,
    string ClubAbbreviation,
    decimal TotalPoints);
