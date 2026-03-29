using Microsoft.AspNetCore.Mvc;
using Tides.Api.Controllers;
using Tides.Api.Dtos;
using Tides.Api.Tests.Stubs;

namespace Tides.Api.Tests;

public class ResultsControllerTests
{
    private readonly StubCarnivalService _stub = new();
    private ResultsController Controller => new(_stub);

    [Fact]
    public async Task Correct_Returns200()
    {
        var id = Guid.NewGuid();
        var response = new ResultResponse(id, Guid.NewGuid(), Guid.NewGuid(),
            2, null, null, "Corrected", Guid.NewGuid(), "Piha", []);
        _stub.CorrectResultAsyncFunc = (_, _) => Task.FromResult(response);

        var request = new CorrectResultRequest(2, null, "Timing error");
        var result = await Controller.Correct(id, request);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(id, ((ResultResponse)ok.Value!).Id);
    }

    [Fact]
    public async Task Delete_Returns204()
    {
        _stub.DeleteResultAsyncFunc = _ => Task.CompletedTask;

        var result = await Controller.Delete(Guid.NewGuid());

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task LodgeProtest_Returns201()
    {
        var protestId = Guid.NewGuid();
        var response = new ProtestResponse(protestId, Guid.NewGuid(), Guid.NewGuid(),
            null, Guid.NewGuid(), "Wrong placing", "Lodged", null,
            DateTime.UtcNow, null);
        _stub.LodgeProtestAsyncFunc = (_, _) => Task.FromResult(response);

        var request = new LodgeProtestRequest(Guid.NewGuid(), null, Guid.NewGuid(), "Wrong placing");
        var result = await Controller.LodgeProtest(Guid.NewGuid(), request);

        var created = Assert.IsType<CreatedResult>(result);
        Assert.Equal(protestId, ((ProtestResponse)created.Value!).Id);
    }
}
