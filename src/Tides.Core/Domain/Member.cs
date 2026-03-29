using Tides.Core.Domain.Enums;

namespace Tides.Core.Domain;

public class Member
{
    public Guid Id { get; private set; }
    public Guid ClubId { get; private set; }
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public DateOnly DateOfBirth { get; private set; }
    public Gender Gender { get; private set; }
    public string? SurfguardId { get; private set; }

    private Member() { }

    public Member(Guid id, Guid clubId, string firstName, string lastName,
        DateOnly dateOfBirth, Gender gender, string? surfguardId = null)
    {
        Id = id;
        ClubId = clubId;
        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
        Gender = gender;
        SurfguardId = surfguardId;
    }

    /// <summary>
    /// Calculates age group based on age at the reference date (typically carnival date).
    /// SLS uses age on the day — if you turn 14 on carnival day, you are in U15.
    /// </summary>
    public AgeGroup GetAgeGroup(DateOnly referenceDate)
    {
        var age = referenceDate.Year - DateOfBirth.Year;
        if (DateOfBirth > referenceDate.AddYears(-age))
            age--;

        return age switch
        {
            < 15 => AgeGroup.U15,
            15 or 16 => AgeGroup.U17,
            17 or 18 => AgeGroup.U19,
            _ => AgeGroup.Open
        };
    }
}
