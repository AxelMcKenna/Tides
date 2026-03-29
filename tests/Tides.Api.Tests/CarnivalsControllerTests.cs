using Microsoft.AspNetCore.Mvc;
using Tides.Api.Controllers;
using Tides.Api.Dtos;
using Tides.Api.Tests.Stubs;

namespace Tides.Api.Tests;

public class CarnivalsControllerTests
{
    private readonly StubCarnivalService _stub = new();
    private CarnivalsController Controller => new(_stub);

    [Fact]
    public async Task List_Returns200WithList()
    {
        var items = new List<CarnivalListItemResponse>
        {
            new(Guid.NewGuid(), "Northern Champs", "Regional",
                new DateOnly(2026, 3, 29), new DateOnly(2026, 3, 30), 8, true)
        };
        _stub.ListCarnivalsAsyncFunc = () => Task.FromResult(items);

        var result = await Controller.List();

        var ok = Assert.IsType<OkObjectResult>(result);
        var data = Assert.IsType<List<CarnivalListItemResponse>>(ok.Value);
        Assert.Single(data);
    }

    [Fact]
    public async Task Create_Returns201WithLocation()
    {
        var id = Guid.NewGuid();
        var response = new CarnivalResponse(id, "Test", Guid.NewGuid(), "Regional",
            new DateOnly(2026, 4, 1), new DateOnly(2026, 4, 1), []);
        _stub.CreateCarnivalAsyncFunc = _ => Task.FromResult(response);

        var request = new CreateCarnivalRequest("Test", Guid.NewGuid(), "Regional",
            new DateOnly(2026, 4, 1), new DateOnly(2026, 4, 1), null);
        var result = await Controller.Create(request);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(id, ((CarnivalResponse)created.Value!).Id);
    }

    [Fact]
    public async Task Get_Returns200()
    {
        var id = Guid.NewGuid();
        var response = new CarnivalResponse(id, "Test", Guid.NewGuid(), "Regional",
            new DateOnly(2026, 4, 1), new DateOnly(2026, 4, 1), []);
        _stub.GetCarnivalAsyncFunc = _ => Task.FromResult(response);

        var result = await Controller.Get(id);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(id, ((CarnivalResponse)ok.Value!).Id);
    }

    [Fact]
    public async Task GetDraw_Returns200()
    {
        var id = Guid.NewGuid();
        var response = new DrawResponse(id, []);
        _stub.GetDrawAsyncFunc = _ => Task.FromResult(response);

        var result = await Controller.GetDraw(id);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetResults_Returns200()
    {
        var id = Guid.NewGuid();
        var response = new CarnivalResultsResponse(id, []);
        _stub.GetResultsAsyncFunc = (_, _) => Task.FromResult(response);

        var result = await Controller.GetResults(id, null);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetResults_PassesEventIdToService()
    {
        var carnivalId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        Guid? receivedEventId = null;
        _stub.GetResultsAsyncFunc = (_, eid) =>
        {
            receivedEventId = eid;
            return Task.FromResult(new CarnivalResultsResponse(carnivalId, []));
        };

        await Controller.GetResults(carnivalId, eventId);

        Assert.Equal(eventId, receivedEventId);
    }

    [Fact]
    public async Task GetLeaderboard_Returns200()
    {
        var id = Guid.NewGuid();
        _stub.GetLeaderboardAsyncFunc = (_, _) =>
            Task.FromResult(new LeaderboardResponse(id, []));

        var result = await Controller.GetLeaderboard(id, null);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task RecordResult_Returns201()
    {
        var resultId = Guid.NewGuid();
        var response = new ResultResponse(resultId, Guid.NewGuid(), Guid.NewGuid(),
            1, null, null, "Provisional", Guid.NewGuid(), "Piha", []);
        _stub.RecordResultAsyncFunc = (_, _) => Task.FromResult(response);

        var request = new RecordResultRequest(Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), Guid.NewGuid(), 1, null, null);
        var result = await Controller.RecordResult(Guid.NewGuid(), request);

        var created = Assert.IsType<CreatedResult>(result);
        Assert.Equal(resultId, ((ResultResponse)created.Value!).Id);
    }

    [Fact]
    public async Task WithdrawEntry_Returns204()
    {
        _stub.WithdrawEntryAsyncFunc = (_, _) => Task.CompletedTask;

        var result = await Controller.WithdrawEntry(Guid.NewGuid(), Guid.NewGuid());

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task CreateEntry_Returns201()
    {
        var entryId = Guid.NewGuid();
        var response = new EntryResponse(entryId, Guid.NewGuid(), Guid.NewGuid(),
            "Piha", [], null, false);
        _stub.CreateEntryAsyncFunc = (_, _) => Task.FromResult(response);

        var request = new CreateEntryRequest(Guid.NewGuid(), Guid.NewGuid(), [Guid.NewGuid()]);
        var result = await Controller.CreateEntry(Guid.NewGuid(), request);

        var created = Assert.IsType<CreatedResult>(result);
        Assert.Equal(entryId, ((EntryResponse)created.Value!).Id);
    }
}
