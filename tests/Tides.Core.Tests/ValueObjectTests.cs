using Tides.Core.Domain;
using Tides.Core.Domain.ValueObjects;

namespace Tides.Core.Tests;

public class ValueObjectTests
{
    // --- Points ---

    [Fact]
    public void Points_Addition()
    {
        var result = new Points(5) + new Points(3);
        Assert.Equal(8m, result.Value);
    }

    [Fact]
    public void Points_Subtraction()
    {
        var result = new Points(5) - new Points(3);
        Assert.Equal(2m, result.Value);
    }

    [Fact]
    public void Points_Zero()
    {
        Assert.Equal(0m, Points.Zero.Value);
    }

    [Fact]
    public void Points_DefaultStruct_IsZero()
    {
        Assert.Equal(0m, default(Points).Value);
    }

    [Fact]
    public void Points_EqualityByValue()
    {
        Assert.Equal(new Points(5), new Points(5));
        Assert.NotEqual(new Points(5), new Points(3));
    }

    // --- TimeResult ---

    [Fact]
    public void TimeResult_ToString_FormatsCorrectly()
    {
        var tr = new TimeResult(TimeSpan.FromSeconds(65.42));
        Assert.Equal("01:05.42", tr.ToString());
    }

    [Fact]
    public void TimeResult_ToString_SubMinute()
    {
        var tr = new TimeResult(TimeSpan.FromSeconds(9.5));
        Assert.Equal("00:09.50", tr.ToString());
    }

    [Fact]
    public void TimeResult_EqualityByValue()
    {
        var a = new TimeResult(TimeSpan.FromSeconds(30));
        var b = new TimeResult(TimeSpan.FromSeconds(30));
        Assert.Equal(a, b);
    }

    // --- Placing ---

    [Fact]
    public void Placing_ValidPosition_Stores()
    {
        Assert.Equal(1, new Placing(1).Position);
        Assert.Equal(100, new Placing(100).Position);
    }

    [Fact]
    public void Placing_Zero_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Placing(0));
    }

    [Fact]
    public void Placing_Negative_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Placing(-1));
    }

    [Fact]
    public void Placing_EqualityByValue()
    {
        Assert.Equal(new Placing(3), new Placing(3));
        Assert.NotEqual(new Placing(1), new Placing(2));
    }

    // --- PointsTable ---

    [Fact]
    public void PointsTable_GetPointsForPlacing_Valid()
    {
        var table = CreateTable();

        Assert.Equal(8m, table.GetPointsForPlacing(1));
        Assert.Equal(6m, table.GetPointsForPlacing(2));
        Assert.Equal(4m, table.GetPointsForPlacing(3));
    }

    [Fact]
    public void PointsTable_GetPointsForPlacing_OutOfRange_ReturnsZero()
    {
        var table = CreateTable();
        Assert.Equal(0m, table.GetPointsForPlacing(10));
    }

    [Fact]
    public void PointsTable_EntriesSortedByPlacing()
    {
        var table = new PointsTable(Guid.NewGuid(), "Test", [
            new PointsTableEntry(3, 4),
            new PointsTableEntry(1, 8),
            new PointsTableEntry(2, 6)
        ]);

        Assert.Equal(1, table.Entries[0].Placing);
        Assert.Equal(2, table.Entries[1].Placing);
        Assert.Equal(3, table.Entries[2].Placing);
    }

    [Fact]
    public void PointsTable_TiedPlacing_TwoWay_Fractional()
    {
        var table = CreateTable(fractional: true);

        // Tied for 2nd: average of 2nd (6) and 3rd (4) = 5
        Assert.Equal(5m, table.GetPointsForTiedPlacing(2, 2));
    }

    [Fact]
    public void PointsTable_TiedPlacing_FractionalDisabled()
    {
        var table = CreateTable(fractional: false);

        // Both get 2nd place points
        Assert.Equal(6m, table.GetPointsForTiedPlacing(2, 2));
    }

    [Fact]
    public void PointsTable_TiedPlacing_CountOfOne_ReturnsNormal()
    {
        var table = CreateTable(fractional: true);

        Assert.Equal(6m, table.GetPointsForTiedPlacing(2, 1));
    }

    [Fact]
    public void PointsTable_TiedPlacing_ThreeWay()
    {
        var table = new PointsTable(Guid.NewGuid(), "Test", [
            new PointsTableEntry(1, 10),
            new PointsTableEntry(2, 8),
            new PointsTableEntry(3, 6),
            new PointsTableEntry(4, 4)
        ], fractionalTiesEnabled: true);

        // Tied for 1st: average of 1st (10), 2nd (8), 3rd (6) = 8
        Assert.Equal(8m, table.GetPointsForTiedPlacing(1, 3));
    }

    private static PointsTable CreateTable(bool fractional = true)
    {
        return new PointsTable(Guid.NewGuid(), "Standard", [
            new PointsTableEntry(1, 8),
            new PointsTableEntry(2, 6),
            new PointsTableEntry(3, 4),
            new PointsTableEntry(4, 2)
        ], fractionalTiesEnabled: fractional);
    }
}
