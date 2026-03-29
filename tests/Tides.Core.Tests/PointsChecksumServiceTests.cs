using Tides.Core.Domain;
using Tides.Core.Services;

namespace Tides.Core.Tests;

public class PointsChecksumServiceTests
{
    private readonly PointsChecksumService _service = new();

    [Fact]
    public void ComputeChecksum_SameInput_SameOutput()
    {
        var events = CreateEvents();

        var hash1 = _service.ComputeChecksum(events);
        var hash2 = _service.ComputeChecksum(events);

        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void ComputeChecksum_DifferentInput_DifferentOutput()
    {
        var events1 = CreateEvents();
        var events2 = new List<ResultEvent>
        {
            new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                Guid.NewGuid(), Guid.NewGuid(),
                2, null, null, "Provisional",
                "device-1", "recorder-1", DateTime.UtcNow, 1)
        };

        var hash1 = _service.ComputeChecksum(events1);
        var hash2 = _service.ComputeChecksum(events2);

        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void ComputeChecksum_OrderIndependent_SortedInternally()
    {
        var heatId = Guid.NewGuid();
        var entryId1 = Guid.NewGuid();
        var entryId2 = Guid.NewGuid();
        var eventId1 = Guid.NewGuid();
        var eventId2 = Guid.NewGuid();

        var event1 = new ResultEvent(Guid.NewGuid(), eventId1, Guid.NewGuid(),
            heatId, entryId1, 1, null, null, "Provisional",
            "device-1", "recorder-1", DateTime.UtcNow, 1);
        var event2 = new ResultEvent(Guid.NewGuid(), eventId2, Guid.NewGuid(),
            heatId, entryId2, 2, null, null, "Provisional",
            "device-1", "recorder-1", DateTime.UtcNow, 1);

        var hashForward = _service.ComputeChecksum([event1, event2]);
        var hashReverse = _service.ComputeChecksum([event2, event1]);

        Assert.Equal(hashForward, hashReverse);
    }

    [Fact]
    public void ComputeChecksum_EmptyList_ReturnsConsistentHash()
    {
        var hash1 = _service.ComputeChecksum([]);
        var hash2 = _service.ComputeChecksum([]);

        Assert.Equal(hash1, hash2);
        Assert.NotEmpty(hash1);
    }

    private static List<ResultEvent> CreateEvents()
    {
        return
        [
            new ResultEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                Guid.NewGuid(), Guid.NewGuid(),
                1, TimeSpan.FromSeconds(30), null, "Provisional",
                "device-1", "recorder-1", DateTime.UtcNow, 1)
        ];
    }
}
