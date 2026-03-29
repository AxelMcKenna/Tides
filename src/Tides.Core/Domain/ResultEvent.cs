namespace Tides.Core.Domain;

public class ResultEvent
{
    public Guid Id { get; private set; }
    public Guid EventId { get; private set; }
    public Guid CarnivalId { get; private set; }
    public Guid HeatId { get; private set; }
    public Guid EntryId { get; private set; }
    public int? Placing { get; private set; }
    public TimeSpan? Time { get; private set; }
    public decimal? JudgeScore { get; private set; }
    public string Status { get; private set; } = null!;
    public string DeviceId { get; private set; } = null!;
    public string RecorderId { get; private set; } = null!;
    public DateTime DeviceTimestamp { get; private set; }
    public DateTime ReceivedAt { get; private set; }
    public int ExpectedHeatVersion { get; private set; }
    public Guid? SupersededBy { get; private set; }

    private ResultEvent() { }

    public ResultEvent(Guid id, Guid eventId, Guid carnivalId, Guid heatId, Guid entryId,
        int? placing, TimeSpan? time, decimal? judgeScore, string status,
        string deviceId, string recorderId, DateTime deviceTimestamp,
        int expectedHeatVersion)
    {
        Id = id;
        EventId = eventId;
        CarnivalId = carnivalId;
        HeatId = heatId;
        EntryId = entryId;
        Placing = placing;
        Time = time;
        JudgeScore = judgeScore;
        Status = status;
        DeviceId = deviceId;
        RecorderId = recorderId;
        DeviceTimestamp = deviceTimestamp;
        ReceivedAt = DateTime.UtcNow;
        ExpectedHeatVersion = expectedHeatVersion;
    }

    public void Supersede(Guid newEventId)
    {
        SupersededBy = newEventId;
    }
}
