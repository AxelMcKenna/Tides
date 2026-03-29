namespace Tides.Core.Domain.ValueObjects;

public readonly record struct Placing
{
    public int Position { get; }

    public Placing(int position)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(position, 1);
        Position = position;
    }
}
