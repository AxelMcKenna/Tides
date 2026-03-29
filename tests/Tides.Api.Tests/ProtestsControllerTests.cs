using Microsoft.AspNetCore.Mvc;
using Tides.Api.Controllers;
using Tides.Api.Dtos;
using Tides.Api.Tests.Stubs;

namespace Tides.Api.Tests;

public class ProtestsControllerTests
{
    private readonly StubCarnivalService _stub = new();
    private ProtestsController Controller => new(_stub);

    [Fact]
    public async Task Adjudicate_Returns200()
    {
        var id = Guid.NewGuid();
        var response = new ProtestResponse(id, Guid.NewGuid(), Guid.NewGuid(),
            null, Guid.NewGuid(), "Wrong placing", "Upheld", "Review confirmed",
            DateTime.UtcNow, DateTime.UtcNow);
        _stub.AdjudicateProtestAsyncFunc = (_, _) => Task.FromResult(response);

        var request = new AdjudicateProtestRequest("Upheld", "Review confirmed");
        var result = await Controller.Adjudicate(id, request);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(id, ((ProtestResponse)ok.Value!).Id);
    }
}
