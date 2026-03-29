using Tides.Core.Domain.Enums;
using Tides.Core.Domain.ValueObjects;

namespace Tides.Core.Domain;

public class Result
{
    public Guid Id { get; private set; }
    public Guid HeatId { get; private set; }
    public Guid EntryId { get; private set; }
    public Placing? Placing { get; private set; }
    public TimeResult? Time { get; private set; }
    public decimal? JudgeScore { get; private set; }
    public ResultStatus Status { get; private set; }
    private readonly List<AuditEntry> _auditTrail = [];
    public IReadOnlyList<AuditEntry> AuditTrail => _auditTrail;

    private Result() { }

    public Result(Guid id, Guid entryId, Placing? placing = null,
        TimeResult? time = null, decimal? judgeScore = null,
        ResultStatus status = ResultStatus.Provisional)
    {
        Id = id;
        EntryId = entryId;
        Placing = placing;
        Time = time;
        JudgeScore = judgeScore;
        Status = status;
    }

    public void Correct(Placing? newPlacing, TimeResult? newTime, string reason, string userId)
    {
        _auditTrail.Add(new AuditEntry(
            DateTime.UtcNow, userId, "Corrected",
            $"Placing: {Placing?.Position} -> {newPlacing?.Position}, Time: {Time} -> {newTime}. Reason: {reason}"));

        Placing = newPlacing;
        Time = newTime;
        Status = ResultStatus.Corrected;
    }

    public void Disqualify(string reason, string userId)
    {
        _auditTrail.Add(new AuditEntry(DateTime.UtcNow, userId, "Disqualified", reason));
        Status = ResultStatus.Disqualified;
    }

    public void Confirm(string userId)
    {
        _auditTrail.Add(new AuditEntry(DateTime.UtcNow, userId, "Confirmed", null));
        Status = ResultStatus.Confirmed;
    }
}
