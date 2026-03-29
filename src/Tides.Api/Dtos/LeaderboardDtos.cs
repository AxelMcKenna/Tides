namespace Tides.Api.Dtos;

public record LeaderboardResponse(
    Guid CarnivalId,
    List<ClubStandingResponse> Standings);

public record ClubStandingResponse(
    int Rank,
    Guid ClubId,
    string ClubName,
    string ClubAbbreviation,
    decimal TotalPoints);
