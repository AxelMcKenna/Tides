using Tides.Core.Domain;

namespace Tides.Core.Tests;

public class EntryTests
{
    [Fact]
    public void Constructor_Individual_SingleMemberId()
    {
        var memberId = Guid.NewGuid();
        var entry = new Entry(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), memberId);

        Assert.Single(entry.MemberIds);
        Assert.Equal(memberId, entry.MemberIds[0]);
    }

    [Fact]
    public void Constructor_Relay_MultipleMemberIds()
    {
        var members = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        var entry = new Entry(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), members);

        Assert.Equal(4, entry.MemberIds.Count);
        Assert.Equal(members, entry.MemberIds);
    }

    [Fact]
    public void Constructor_InitializesDefaults()
    {
        var entry = new Entry(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        Assert.Null(entry.HeatId);
        Assert.Null(entry.Lane);
        Assert.False(entry.IsWithdrawn);
    }

    [Fact]
    public void AssignToHeat_SetsHeatId()
    {
        var entry = new Entry(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        var heatId = Guid.NewGuid();

        entry.AssignToHeat(heatId);

        Assert.Equal(heatId, entry.HeatId);
    }

    [Fact]
    public void AssignLane_ValidLane_SetsLane()
    {
        var entry = new Entry(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        entry.AssignLane(1);
        Assert.Equal(1, entry.Lane);

        entry.AssignLane(6);
        Assert.Equal(6, entry.Lane);
    }

    [Fact]
    public void AssignLane_Zero_Throws()
    {
        var entry = new Entry(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        Assert.Throws<ArgumentOutOfRangeException>(() => entry.AssignLane(0));
    }

    [Fact]
    public void AssignLane_Negative_Throws()
    {
        var entry = new Entry(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        Assert.Throws<ArgumentOutOfRangeException>(() => entry.AssignLane(-1));
    }

    [Fact]
    public void Withdraw_SetsIsWithdrawnAndClearsLane()
    {
        var entry = new Entry(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        entry.AssignLane(3);

        entry.Withdraw();

        Assert.True(entry.IsWithdrawn);
        Assert.Null(entry.Lane);
    }

    [Fact]
    public void Withdraw_AlreadyWithdrawn_IsIdempotent()
    {
        var entry = new Entry(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        entry.Withdraw();

        entry.Withdraw(); // should not throw

        Assert.True(entry.IsWithdrawn);
    }
}
