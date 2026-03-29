namespace Tides.Core.Domain;

public class Organisation
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string Country { get; private set; } = null!;

    private Organisation() { }

    public Organisation(Guid id, string name, string country)
    {
        Id = id;
        Name = name;
        Country = country;
    }
}
