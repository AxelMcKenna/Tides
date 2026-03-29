namespace Tides.Api.Dtos;

public record MemberResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Gender,
    string AgeGroup,
    string? SurfguardId);
