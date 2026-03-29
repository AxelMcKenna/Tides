using Tides.Core.Domain;
using Tides.Core.Domain.Enums;

namespace Tides.Core.Tests;

public class ProtestTests
{
    [Fact]
    public void Constructor_DefaultsToLodgedStatus()
    {
        var protest = new Protest(Guid.NewGuid(), Guid.NewGuid(), null, Guid.NewGuid(), "Reason");

        Assert.Equal(ProtestStatus.Lodged, protest.Status);
    }

    [Fact]
    public void Constructor_SetsLodgedAtTimestamp()
    {
        var before = DateTime.UtcNow;
        var protest = new Protest(Guid.NewGuid(), Guid.NewGuid(), null, Guid.NewGuid(), "Reason");
        var after = DateTime.UtcNow;

        Assert.InRange(protest.LodgedAt, before, after);
    }

    [Fact]
    public void Constructor_WithHeatId_StoresIt()
    {
        var heatId = Guid.NewGuid();
        var protest = new Protest(Guid.NewGuid(), Guid.NewGuid(), heatId, Guid.NewGuid(), "Reason");

        Assert.Equal(heatId, protest.HeatId);
    }

    [Fact]
    public void Constructor_WithoutHeatId_NullHeatId()
    {
        var protest = new Protest(Guid.NewGuid(), Guid.NewGuid(), null, Guid.NewGuid(), "Reason");

        Assert.Null(protest.HeatId);
    }

    [Fact]
    public void Adjudicate_Upheld_SetsStatusAndReason()
    {
        var protest = new Protest(Guid.NewGuid(), Guid.NewGuid(), null, Guid.NewGuid(), "Reason");

        protest.Adjudicate(ProtestStatus.Upheld, "Review confirmed error");

        Assert.Equal(ProtestStatus.Upheld, protest.Status);
        Assert.Equal("Review confirmed error", protest.AdjudicationReason);
    }

    [Fact]
    public void Adjudicate_Dismissed_SetsStatusAndReason()
    {
        var protest = new Protest(Guid.NewGuid(), Guid.NewGuid(), null, Guid.NewGuid(), "Reason");

        protest.Adjudicate(ProtestStatus.Dismissed, "No evidence");

        Assert.Equal(ProtestStatus.Dismissed, protest.Status);
        Assert.Equal("No evidence", protest.AdjudicationReason);
    }

    [Fact]
    public void Adjudicate_WithLodged_Throws()
    {
        var protest = new Protest(Guid.NewGuid(), Guid.NewGuid(), null, Guid.NewGuid(), "Reason");

        Assert.Throws<ArgumentException>(() =>
            protest.Adjudicate(ProtestStatus.Lodged, "Invalid outcome"));
    }

    [Fact]
    public void Adjudicate_WithUnderReview_Throws()
    {
        var protest = new Protest(Guid.NewGuid(), Guid.NewGuid(), null, Guid.NewGuid(), "Reason");

        Assert.Throws<ArgumentException>(() =>
            protest.Adjudicate(ProtestStatus.UnderReview, "Invalid outcome"));
    }

    [Fact]
    public void Adjudicate_SetsAdjudicatedAtTimestamp()
    {
        var protest = new Protest(Guid.NewGuid(), Guid.NewGuid(), null, Guid.NewGuid(), "Reason");
        var before = DateTime.UtcNow;

        protest.Adjudicate(ProtestStatus.Upheld, "Confirmed");

        Assert.NotNull(protest.AdjudicatedAt);
        Assert.True(protest.AdjudicatedAt >= before);
    }
}
