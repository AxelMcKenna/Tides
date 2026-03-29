namespace Tides.Core.Domain;

public class Branch
{
    public Guid Id { get; private set; }
    public Guid RegionId { get; private set; }
    public string Name { get; private set; } = null!;

    private Branch() { }

    public Branch(Guid id, Guid regionId, string name)
    {
        Id = id;
        RegionId = regionId;
        Name = name;
    }
}
