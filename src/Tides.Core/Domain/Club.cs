namespace Tides.Core.Domain;

public class Club
{
    public Guid Id { get; private set; }
    public Guid BranchId { get; private set; }
    public string Name { get; private set; } = null!;
    public string Abbreviation { get; private set; } = null!;

    private Club() { }

    public Club(Guid id, Guid branchId, string name, string abbreviation)
    {
        Id = id;
        BranchId = branchId;
        Name = name;
        Abbreviation = abbreviation;
    }
}
