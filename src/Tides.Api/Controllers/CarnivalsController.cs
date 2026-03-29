using Microsoft.AspNetCore.Mvc;
using Tides.Api.Dtos;
using Tides.Api.Services;

namespace Tides.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CarnivalsController(ICarnivalService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<CarnivalListItemResponse>), 200)]
    public async Task<IActionResult> List()
    {
        return Ok(await service.ListCarnivalsAsync());
    }

    [HttpPost]
    [ProducesResponseType(typeof(CarnivalResponse), 201)]
    public async Task<IActionResult> Create(CreateCarnivalRequest request)
    {
        var result = await service.CreateCarnivalAsync(request);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CarnivalResponse), 200)]
    public async Task<IActionResult> Get(Guid id)
    {
        return Ok(await service.GetCarnivalAsync(id));
    }

    [HttpGet("{id:guid}/draw")]
    [ProducesResponseType(typeof(DrawResponse), 200)]
    public async Task<IActionResult> GetDraw(Guid id)
    {
        return Ok(await service.GetDrawAsync(id));
    }

    [HttpGet("{id:guid}/results")]
    [ProducesResponseType(typeof(CarnivalResultsResponse), 200)]
    public async Task<IActionResult> GetResults(Guid id, [FromQuery] Guid? eventId)
    {
        return Ok(await service.GetResultsAsync(id, eventId));
    }

    [HttpGet("{id:guid}/leaderboard")]
    [ProducesResponseType(typeof(LeaderboardResponse), 200)]
    public async Task<IActionResult> GetLeaderboard(Guid id, [FromQuery] string? ageGroup)
    {
        return Ok(await service.GetLeaderboardAsync(id, ageGroup));
    }

    [HttpGet("{id:guid}/heats/{heatId:guid}")]
    [ProducesResponseType(typeof(HeatDrawResponse), 200)]
    public async Task<IActionResult> GetHeat(Guid id, Guid heatId)
    {
        return Ok(await service.GetHeatAsync(id, heatId));
    }

    [HttpPost("{id:guid}/draws/generate")]
    [ProducesResponseType(typeof(DrawResponse), 200)]
    public async Task<IActionResult> GenerateDraw(Guid id, GenerateDrawRequest request)
    {
        return Ok(await service.GenerateDrawAsync(id, request));
    }

    [HttpPost("{id:guid}/entries")]
    [ProducesResponseType(typeof(EntryResponse), 201)]
    public async Task<IActionResult> CreateEntry(Guid id, CreateEntryRequest request)
    {
        var result = await service.CreateEntryAsync(id, request);
        return Created($"/api/carnivals/{id}/entries/{result.Id}", result);
    }

    [HttpDelete("{id:guid}/entries/{entryId:guid}")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> WithdrawEntry(Guid id, Guid entryId)
    {
        await service.WithdrawEntryAsync(id, entryId);
        return NoContent();
    }

    [HttpPost("{id:guid}/results")]
    [ProducesResponseType(typeof(ResultResponse), 201)]
    public async Task<IActionResult> RecordResult(Guid id, RecordResultRequest request)
    {
        var result = await service.RecordResultAsync(id, request);
        return Created($"/api/results/{result.Id}", result);
    }
}
