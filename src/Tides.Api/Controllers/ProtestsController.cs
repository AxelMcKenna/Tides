using Microsoft.AspNetCore.Mvc;
using Tides.Api.Dtos;
using Tides.Api.Services;

namespace Tides.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProtestsController(ICarnivalService service) : ControllerBase
{
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(typeof(ProtestResponse), 200)]
    public async Task<IActionResult> Adjudicate(Guid id, AdjudicateProtestRequest request)
    {
        return Ok(await service.AdjudicateProtestAsync(id, request));
    }
}
