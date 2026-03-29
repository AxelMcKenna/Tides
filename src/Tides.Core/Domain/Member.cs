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
            < 8 => AgeGroup.U8,
            8 => AgeGroup.U9,
            9 => AgeGroup.U10,
            10 => AgeGroup.U11,
            11 => AgeGroup.U12,
            12 => AgeGroup.U13,
            13 => AgeGroup.U14,
            14 => AgeGroup.U15,
            15 or 16 => AgeGroup.U17,
            >= 17 and < 30 => AgeGroup.Open,
            >= 30 and < 40 => AgeGroup.Masters30,
            >= 40 and < 50 => AgeGroup.Masters40,
            >= 50 and < 60 => AgeGroup.Masters50,
            _ => AgeGroup.Masters60
        };
    }
}
