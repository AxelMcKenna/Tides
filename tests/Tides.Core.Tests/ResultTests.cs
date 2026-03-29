using Tides.Core.Domain;
using Tides.Core.Domain.Enums;
using Tides.Core.Domain.ValueObjects;

namespace Tides.Core.Tests;

public class ResultTests
{
    [Fact]
    public void Constructor_DefaultsToProvisional()
    {
        var result = new Result(Guid.NewGuid(), Guid.NewGuid(), new Placing(1));

        Assert.Equal(ResultStatus.Provisional, result.Status);
    }

    [Fact]
    public void Constructor_AcceptsExplicitStatus()
    {
        var result = new Result(Guid.NewGuid(), Guid.NewGuid(),
            status: ResultStatus.DidNotStart);

        Assert.Equal(ResultStatus.DidNotStart, result.Status);
    }

    [Fact]
    public void Constructor_StoresAllFields()
    {
        var id = Guid.NewGuid();
        var entryId = Guid.NewGuid();
        var placing = new Placing(2);
        var time = new TimeResult(TimeSpan.FromSeconds(32.5));

        var result = new Result(id, entryId, placing, time, 8.5m);

        Assert.Equal(id, result.Id);
        Assert.Equal(entryId, result.EntryId);
        Assert.Equal(2, result.Placing!.Value.Position);
        Assert.Equal(TimeSpan.FromSeconds(32.5), result.Time!.Value.Time);
        Assert.Equal(8.5m, result.JudgeScore);
    }

    [Fact]
    public void Correct_UpdatesPlacingAndTime()
    {
        var result = new Result(Guid.NewGuid(), Guid.NewGuid(), new Placing(3),
            new TimeResult(TimeSpan.FromSeconds(30)));

        result.Correct(new Placing(2), new TimeResult(TimeSpan.FromSeconds(29)), "Timing error", "official-1");

        Assert.Equal(2, result.Placing!.Value.Position);
        Assert.Equal(TimeSpan.FromSeconds(29), result.Time!.Value.Time);
        Assert.Equal(ResultStatus.Corrected, result.Status);
    }

    [Fact]
    public void Correct_AppendsAuditEntry()
    {
        var result = new Result(Guid.NewGuid(), Guid.NewGuid(), new Placing(1));

        result.Correct(new Placing(2), null, "Wrong lane", "official-1");

        Assert.Single(result.AuditTrail);
        var audit = result.AuditTrail[0];
        Assert.Equal("official-1", audit.UserId);
        Assert.Equal("Corrected", audit.Action);
        Assert.Contains("Wrong lane", audit.Detail!);
    }

    [Fact]
    public void Correct_MultipleTimes_AccumulatesAuditTrail()
    {
        var result = new Result(Guid.NewGuid(), Guid.NewGuid(), new Placing(1));

        result.Correct(new Placing(2), null, "First correction", "official-1");
        result.Correct(new Placing(3), null, "Second correction", "official-2");

        Assert.Equal(2, result.AuditTrail.Count);
        Assert.Contains("First correction", result.AuditTrail[0].Detail!);
        Assert.Contains("Second correction", result.AuditTrail[1].Detail!);
    }

    [Fact]
    public void Disqualify_SetsStatusAndAddsAudit()
    {
        var result = new Result(Guid.NewGuid(), Guid.NewGuid(), new Placing(1));

        result.Disqualify("False start", "official-1");

        Assert.Equal(ResultStatus.Disqualified, result.Status);
        Assert.Single(result.AuditTrail);
        Assert.Equal("Disqualified", result.AuditTrail[0].Action);
        Assert.Equal("False start", result.AuditTrail[0].Detail);
    }

    [Fact]
    public void Confirm_SetsStatusAndAddsAudit()
    {
        var result = new Result(Guid.NewGuid(), Guid.NewGuid(), new Placing(1));

        result.Confirm("official-1");

        Assert.Equal(ResultStatus.Confirmed, result.Status);
        Assert.Single(result.AuditTrail);
        Assert.Equal("Confirmed", result.AuditTrail[0].Action);
        Assert.Equal("official-1", result.AuditTrail[0].UserId);
    }

    [Fact]
    public void StatusTransition_Provisional_Correct_Confirm()
    {
        var result = new Result(Guid.NewGuid(), Guid.NewGuid(), new Placing(1));
        Assert.Equal(ResultStatus.Provisional, result.Status);

        result.Correct(new Placing(2), null, "Correction", "official-1");
        Assert.Equal(ResultStatus.Corrected, result.Status);

        result.Confirm("official-2");
        Assert.Equal(ResultStatus.Confirmed, result.Status);

        Assert.Equal(2, result.AuditTrail.Count);
    }
}
