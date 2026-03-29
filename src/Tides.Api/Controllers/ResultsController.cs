using Microsoft.AspNetCore.Mvc;
using Tides.Api.Dtos;
using Tides.Api.Services;

namespace Tides.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResultsController(ICarnivalService service) : ControllerBase
{
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(typeof(ResultResponse), 200)]
    public async Task<IActionResult> Correct(Guid id, CorrectResultRequest request)
    {
        return Ok(await service.CorrectResultAsync(id, request));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await service.DeleteResultAsync(id);
        return NoContent();
    }

    [HttpPost("{id:guid}/protest")]
    [ProducesResponseType(typeof(ProtestResponse), 201)]
    public async Task<IActionResult> LodgeProtest(Guid id, LodgeProtestRequest request)
    {
        var result = await service.LodgeProtestAsync(id, request);
        return Created($"/api/protests/{result.Id}", result);
    }
}
