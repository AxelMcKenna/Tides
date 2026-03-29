using Tides.Core.Domain;

namespace Tides.Core.Tests;

public class ResultEventTests
{
    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var id = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var carnivalId = Guid.NewGuid();
        var heatId = Guid.NewGuid();
        var entryId = Guid.NewGuid();
        var timestamp = DateTime.UtcNow.AddMinutes(-5);

        var resultEvent = new ResultEvent(id, eventId, carnivalId, heatId, entryId,
            1, TimeSpan.FromSeconds(30), null, "Provisional",
            "device-1", "recorder-1", timestamp, 1);

        Assert.Equal(id, resultEvent.Id);
        Assert.Equal(eventId, resultEvent.EventId);
        Assert.Equal(carnivalId, resultEvent.CarnivalId);
        Assert.Equal(heatId, resultEvent.HeatId);
        Assert.Equal(entryId, resultEvent.EntryId);
        Assert.Equal(1, resultEvent.Placing);
        Assert.Equal(TimeSpan.FromSeconds(30), resultEvent.Time);
        Assert.Null(resultEvent.JudgeScore);
        Assert.Equal("Provisional", resultEvent.Status);
        Assert.Equal("device-1", resultEvent.DeviceId);
        Assert.Equal("recorder-1", resultEvent.RecorderId);
        Assert.Equal(timestamp, resultEvent.DeviceTimestamp);
        Assert.Equal(1, resultEvent.ExpectedHeatVersion);
        Assert.Null(resultEvent.SupersededBy);
    }

    [Fact]
    public void ReceivedAt_IsSetOnConstruction()
    {
        var before = DateTime.UtcNow;
        var resultEvent = CreateResultEvent();
        var after = DateTime.UtcNow;

        Assert.InRange(resultEvent.ReceivedAt, before, after);
    }

    [Fact]
    public void Supersede_SetsSuperseededBy()
    {
        var resultEvent = CreateResultEvent();
        var newEventId = Guid.NewGuid();

        resultEvent.Supersede(newEventId);

        Assert.Equal(newEventId, resultEvent.SupersededBy);
    }

    [Fact]
    public void Supersede_CanBeCalledOnce()
    {
        var resultEvent = CreateResultEvent();
        Assert.Null(resultEvent.SupersededBy);

        var newEventId = Guid.NewGuid();
        resultEvent.Supersede(newEventId);

        Assert.Equal(newEventId, resultEvent.SupersededBy);
    }

    private static ResultEvent CreateResultEvent()
    {
        return new ResultEvent(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), Guid.NewGuid(),
            1, null, null, "Provisional",
            "device-1", "recorder-1", DateTime.UtcNow, 1);
    }
}
