namespace Tides.Core.Domain.ValueObjects;

public readonly record struct TimeResult(TimeSpan Time)
{
    public override string ToString() => Time.ToString(@"mm\:ss\.ff");
}
