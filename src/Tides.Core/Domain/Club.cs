namespace Tides.Core.Domain;

public class Club
{
    public Guid Id { get; private set; }
    public Guid RegionId { get; private set; }
    public string Name { get; private set; } = null!;
    public string Abbreviation { get; private set; } = null!;

    private Club() { }

    public Club(Guid id, Guid regionId, string name, string abbreviation)
    {
        Id = id;
        RegionId = regionId;
        Name = name;
        Abbreviation = abbreviation;
    }
}
