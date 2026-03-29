namespace Tides.Core.Domain.ValueObjects;

public readonly record struct Points(decimal Value)
{
    public static Points Zero => new(0m);

    public static Points operator +(Points a, Points b) => new(a.Value + b.Value);
    public static Points operator -(Points a, Points b) => new(a.Value - b.Value);
}
