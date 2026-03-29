using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tides.Api.Dtos;
using Tides.Infrastructure.Persistence;

namespace Tides.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClubsController(TidesDbContext db) : ControllerBase
{
    [HttpGet("{id:guid}/members")]
    [ProducesResponseType(typeof(List<MemberResponse>), 200)]
    public async Task<IActionResult> GetMembers(Guid id, [FromQuery] string? ageGroup,
        [FromQuery] string? gender, [FromQuery] DateOnly? referenceDate)
    {
        var refDate = referenceDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var members = await db.Members
            .Where(m => m.ClubId == id)
            .AsNoTracking()
            .ToListAsync();

        var result = members.Select(m => new MemberResponse(
            m.Id, m.FirstName, m.LastName,
            m.Gender.ToString(),
            m.GetAgeGroup(refDate).ToString(),
            m.SurfguardId
        ));

        if (ageGroup != null)
            result = result.Where(m => m.AgeGroup == ageGroup);
        if (gender != null)
            result = result.Where(m => m.Gender == gender);

        return Ok(result.ToList());
    }
}
