using Microsoft.AspNetCore.Mvc;
using Tides.Api.Controllers;
using Tides.Api.Dtos;
using Tides.Api.Services;

namespace Tides.Api.Tests;

public class SyncControllerTests
{
    [Fact]
    public async Task Sync_AllAcknowledged_Returns200()
    {
        var response = new SyncBatchResponse(
            [new SyncEventAck(Guid.NewGuid(), 2)],
            [],
            []);
        var stub = new StubSyncService(response);
        var controller = new SyncController(stub);

        var request = CreateBatchRequest();
        var result = await controller.Sync(request);

        var ok = Assert.IsType<OkObjectResult>(result);
        var body = Assert.IsType<SyncBatchResponse>(ok.Value);
        Assert.Single(body.Acknowledged);
        Assert.Empty(body.Conflicts);
        Assert.Empty(body.Errors);
    }

    [Fact]
    public async Task Sync_WithConflicts_Returns207()
    {
        var response = new SyncBatchResponse(
            [new SyncEventAck(Guid.NewGuid(), 2)],
            [new SyncEventConflict(Guid.NewGuid(), 1, 3, [])],
            []);
        var stub = new StubSyncService(response);
        var controller = new SyncController(stub);

        var request = CreateBatchRequest();
        var result = await controller.Sync(request);

        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(207, statusResult.StatusCode);
    }

    [Fact]
    public async Task Sync_WithErrors_Returns207()
    {
        var response = new SyncBatchResponse(
            [],
            [],
            [new SyncEventError(Guid.NewGuid(), "HEAT_NOT_FOUND", "Heat not found.")]);
        var stub = new StubSyncService(response);
        var controller = new SyncController(stub);

        var request = CreateBatchRequest();
        var result = await controller.Sync(request);

        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(207, statusResult.StatusCode);
    }

    [Fact]
    public async Task Sync_EmptyBatch_Returns200()
    {
        var response = new SyncBatchResponse([], [], []);
        var stub = new StubSyncService(response);
        var controller = new SyncController(stub);

        var request = new SyncBatchRequest(Guid.NewGuid(), "device-1", "recorder-1", []);
        var result = await controller.Sync(request);

        Assert.IsType<OkObjectResult>(result);
    }

    private static SyncBatchRequest CreateBatchRequest()
    {
        return new SyncBatchRequest(
            Guid.NewGuid(),
            "device-1",
            "recorder-1",
            [
                new SyncEventRequest(
                    Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                    1, null, null, "Provisional", DateTime.UtcNow, 1)
            ]);
    }

    private class StubSyncService(SyncBatchResponse response) : ISyncService
    {
        public Task<SyncBatchResponse> ProcessBatchAsync(SyncBatchRequest request)
            => Task.FromResult(response);
    }
}
