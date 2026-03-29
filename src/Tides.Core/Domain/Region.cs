namespace Tides.Core.Domain;

public class Region
{
    public Guid Id { get; private set; }
    public Guid OrganisationId { get; private set; }
    public string Name { get; private set; } = null!;

    private Region() { }

    public Region(Guid id, Guid organisationId, string name)
    {
        Id = id;
        OrganisationId = organisationId;
        Name = name;
    }
}
