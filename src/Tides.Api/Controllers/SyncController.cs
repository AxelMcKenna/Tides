using Microsoft.AspNetCore.Mvc;
using Tides.Api.Dtos;
using Tides.Api.Services;

namespace Tides.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SyncController(ISyncService syncService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(SyncBatchResponse), 200)]
    [ProducesResponseType(typeof(SyncBatchResponse), 207)]
    public async Task<IActionResult> Sync(SyncBatchRequest request)
    {
        var response = await syncService.ProcessBatchAsync(request);

        if (response.Conflicts.Count == 0 && response.Errors.Count == 0)
            return Ok(response);

        return StatusCode(207, response);
    }
}
