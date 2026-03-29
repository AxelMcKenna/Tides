using Tides.Core.Domain;
using Tides.Core.Domain.Enums;
using Tides.Core.Domain.ValueObjects;

namespace Tides.Infrastructure.Persistence;

public static class SeedData
{
    // Fixed GUIDs for seed data so they're stable across migrations
    public static readonly Guid SlsnzId = Guid.Parse("a1000000-0000-0000-0000-000000000001");
    public static readonly Guid NorthernRegionId = Guid.Parse("a2000000-0000-0000-0000-000000000001");
    public static readonly Guid AucklandBranchId = Guid.Parse("a3000000-0000-0000-0000-000000000001");
    public static readonly Guid PihaClubId = Guid.Parse("a4000000-0000-0000-0000-000000000001");
    public static readonly Guid MurraysClubId = Guid.Parse("a4000000-0000-0000-0000-000000000002");
    public static readonly Guid BethellsClubId = Guid.Parse("a4000000-0000-0000-0000-000000000003");
    public static readonly Guid OrewaClubId = Guid.Parse("a4000000-0000-0000-0000-000000000004");
    public static readonly Guid WaipuClubId = Guid.Parse("a4000000-0000-0000-0000-000000000005");
    public static readonly Guid RedBeachClubId = Guid.Parse("a4000000-0000-0000-0000-000000000006");

    // Carnival IDs
    public static readonly Guid LiveCarnivalId = Guid.Parse("c1000000-0000-0000-0000-000000000001");
    public static readonly Guid CompletedCarnivalId = Guid.Parse("c1000000-0000-0000-0000-000000000002");
    public static readonly Guid UpcomingCarnivalId = Guid.Parse("c1000000-0000-0000-0000-000000000003");

    // Points table
    public static readonly Guid StandardPointsTableId = Guid.Parse("d1000000-0000-0000-0000-000000000001");

    public static void Seed(TidesDbContext context)
    {
        if (context.Organisations.Any())
            return;

        // Organisation hierarchy
        var slsnz = new Organisation(SlsnzId, "Surf Life Saving New Zealand", "NZ");
        context.Organisations.Add(slsnz);

        var northernRegion = new Region(NorthernRegionId, SlsnzId, "Northern Region");
        context.Regions.Add(northernRegion);

        var aucklandBranch = new Branch(AucklandBranchId, NorthernRegionId, "Auckland");
        context.Branches.Add(aucklandBranch);

        // 6 Clubs
        var piha = new Club(PihaClubId, AucklandBranchId, "Piha Surf Life Saving Club", "PIH");
        var murrays = new Club(MurraysClubId, AucklandBranchId, "Muriwai Volunteer Lifeguard Service", "MUR");
        var bethells = new Club(BethellsClubId, AucklandBranchId, "Bethells Beach Surf Life Saving Patrol", "BET");
        var orewa = new Club(OrewaClubId, AucklandBranchId, "Orewa Surf Life Saving Club", "ORE");
        var waipu = new Club(WaipuClubId, AucklandBranchId, "Waipu Cove Surf Life Saving Club", "WAI");
        var redBeach = new Club(RedBeachClubId, AucklandBranchId, "Red Beach Surf Life Saving Club", "RED");
        context.Clubs.AddRange(piha, murrays, bethells, orewa, waipu, redBeach);

        // Members — mix of age groups and genders across all clubs
        var members = CreateMembers();
        context.Members.AddRange(members);

        // Standard points table
        var pointsTable = new PointsTable(StandardPointsTableId, "Standard Branch", [
            new PointsTableEntry(1, 8),
            new PointsTableEntry(2, 6),
            new PointsTableEntry(3, 4),
            new PointsTableEntry(4, 3),
            new PointsTableEntry(5, 2),
            new PointsTableEntry(6, 1)
        ]);

        // === CARNIVAL 1: Live — in progress, partial results ===
        var liveCarnival = new Carnival(LiveCarnivalId, "Auckland Branch Championships 2026",
            PihaClubId, SanctionLevel.Branch,
            new DateOnly(2026, 3, 29), new DateOnly(2026, 3, 30));
        liveCarnival.SetPointsTable(pointsTable);
        SeedLiveCarnival(liveCarnival, members);
        context.Carnivals.Add(liveCarnival);

        // === CARNIVAL 2: Completed — all events finished, confirmed results ===
        var completedCarnival = new Carnival(CompletedCarnivalId, "Piha Classic 2026",
            PihaClubId, SanctionLevel.Club,
            new DateOnly(2026, 2, 15), new DateOnly(2026, 2, 15));
        var completedPts = new PointsTable(Guid.Parse("d1000000-0000-0000-0000-000000000002"), "Standard Branch", [
            new PointsTableEntry(1, 8),
            new PointsTableEntry(2, 6),
            new PointsTableEntry(3, 4),
            new PointsTableEntry(4, 3),
            new PointsTableEntry(5, 2),
            new PointsTableEntry(6, 1)
        ]);
        completedCarnival.SetPointsTable(completedPts);
        SeedCompletedCarnival(completedCarnival, members);
        context.Carnivals.Add(completedCarnival);

        // === CARNIVAL 3: Upcoming — draw generated, no results yet ===
        var upcomingCarnival = new Carnival(UpcomingCarnivalId, "Northern Region Championships 2026",
            OrewaClubId, SanctionLevel.State,
            new DateOnly(2026, 5, 10), new DateOnly(2026, 5, 11));
        var upcomingPts = new PointsTable(Guid.Parse("d1000000-0000-0000-0000-000000000003"), "Standard Branch", [
            new PointsTableEntry(1, 8),
            new PointsTableEntry(2, 6),
            new PointsTableEntry(3, 4),
            new PointsTableEntry(4, 3),
            new PointsTableEntry(5, 2),
            new PointsTableEntry(6, 1)
        ]);
        upcomingCarnival.SetPointsTable(upcomingPts);
        SeedUpcomingCarnival(upcomingCarnival, members);
        context.Carnivals.Add(upcomingCarnival);

        context.SaveChanges();
    }

    private static List<Member> CreateMembers()
    {
        // U14 members (born 2012 — age 13 at carnival date in 2026)
        // Open members (born 1995-2002)
        // U17 members (born 2009-2010)
        return
        [
            // Piha — 6 members
            new(Guid.Parse("b1000000-0000-0000-0000-000000000001"), PihaClubId, "Tane", "Walker", new DateOnly(2012, 5, 10), Gender.Male),
            new(Guid.Parse("b1000000-0000-0000-0000-000000000002"), PihaClubId, "Aroha", "Smith", new DateOnly(2012, 8, 22), Gender.Female),
            new(Guid.Parse("b1000000-0000-0000-0000-000000000003"), PihaClubId, "Jack", "Wilson", new DateOnly(1998, 3, 14), Gender.Male),
            new(Guid.Parse("b1000000-0000-0000-0000-000000000004"), PihaClubId, "Mia", "Anderson", new DateOnly(1999, 6, 8), Gender.Female),
            new(Guid.Parse("b1000000-0000-0000-0000-000000000005"), PihaClubId, "Finn", "Walker", new DateOnly(2010, 2, 15), Gender.Male),
            new(Guid.Parse("b1000000-0000-0000-0000-000000000006"), PihaClubId, "Lila", "Wilson", new DateOnly(2010, 7, 3), Gender.Female),

            // Muriwai — 6 members
            new(Guid.Parse("b1000000-0000-0000-0000-000000000011"), MurraysClubId, "Nikau", "Brown", new DateOnly(2012, 1, 30), Gender.Male),
            new(Guid.Parse("b1000000-0000-0000-0000-000000000012"), MurraysClubId, "Kaia", "Jones", new DateOnly(2012, 11, 5), Gender.Female),
            new(Guid.Parse("b1000000-0000-0000-0000-000000000013"), MurraysClubId, "Liam", "Taylor", new DateOnly(1995, 7, 19), Gender.Male),
            new(Guid.Parse("b1000000-0000-0000-0000-000000000014"), MurraysClubId, "Sophie", "Brown", new DateOnly(1997, 4, 22), Gender.Female),
            new(Guid.Parse("b1000000-0000-0000-0000-000000000015"), MurraysClubId, "Caleb", "Jones", new DateOnly(2010, 9, 12), Gender.Male),
            new(Guid.Parse("b1000000-0000-0000-0000-000000000016"), MurraysClubId, "Zoe", "Taylor", new DateOnly(2009, 11, 28), Gender.Female),

            // Bethells — 6 members
            new(Guid.Parse("b1000000-0000-0000-0000-000000000021"), BethellsClubId, "Maia", "Davis", new DateOnly(2012, 4, 2), Gender.Female),
            new(Guid.Parse("b1000000-0000-0000-0000-000000000022"), BethellsClubId, "Reuben", "Moore", new DateOnly(2012, 9, 18), Gender.Male),
            new(Guid.Parse("b1000000-0000-0000-0000-000000000023"), BethellsClubId, "Isla", "Thomas", new DateOnly(1999, 12, 1), Gender.Female),
            new(Guid.Parse("b1000000-0000-0000-0000-000000000024"), BethellsClubId, "Ethan", "Davis", new DateOnly(2000, 8, 15), Gender.Male),
            new(Guid.Parse("b1000000-0000-0000-0000-000000000025"), BethellsClubId, "Harper", "Moore", new DateOnly(2010, 3, 7), Gender.Female),
            new(Guid.Parse("b1000000-0000-0000-0000-000000000026"), BethellsClubId, "Mason", "Thomas", new DateOnly(2009, 6, 21), Gender.Male),

            // Orewa — 6 members
            new(Guid.Parse("b1000000-0000-0000-0000-000000000031"), OrewaClubId, "Hunter", "Lee", new DateOnly(2012, 3, 14), Gender.Male),
            new(Guid.Parse("b1000000-0000-0000-0000-000000000032"), OrewaClubId, "Willow", "Martin", new DateOnly(2012, 7, 28), Gender.Female),
            new(Guid.Parse("b1000000-0000-0000-0000-000000000033"), OrewaClubId, "Ben", "Lee", new DateOnly(1996, 10, 5), Gender.Male),
            new(Guid.Parse("b1000000-0000-0000-0000-000000000034"), OrewaClubId, "Emma", "Martin", new DateOnly(2001, 1, 19), Gender.Female),
            new(Guid.Parse("b1000000-0000-0000-0000-000000000035"), OrewaClubId, "Riley", "Lee", new DateOnly(2010, 5, 30), Gender.Male),
            new(Guid.Parse("b1000000-0000-0000-0000-000000000036"), OrewaClubId, "Ivy", "Martin", new DateOnly(2009, 8, 14), Gender.Female),

            // Waipu Cove — 6 members
            new(Guid.Parse("b1000000-0000-0000-0000-000000000041"), WaipuClubId, "Koa", "Harris", new DateOnly(2012, 6, 20), Gender.Male),
            new(Guid.Parse("b1000000-0000-0000-0000-000000000042"), WaipuClubId, "Ava", "White", new DateOnly(2012, 10, 9), Gender.Female),
            new(Guid.Parse("b1000000-0000-0000-0000-000000000043"), WaipuClubId, "Sam", "Harris", new DateOnly(1997, 2, 28), Gender.Male),
            new(Guid.Parse("b1000000-0000-0000-0000-000000000044"), WaipuClubId, "Grace", "White", new DateOnly(2000, 5, 12), Gender.Female),
            new(Guid.Parse("b1000000-0000-0000-0000-000000000045"), WaipuClubId, "Leo", "Harris", new DateOnly(2010, 1, 18), Gender.Male),
            new(Guid.Parse("b1000000-0000-0000-0000-000000000046"), WaipuClubId, "Ella", "White", new DateOnly(2009, 4, 6), Gender.Female),

            // Red Beach — 6 members
            new(Guid.Parse("b1000000-0000-0000-0000-000000000051"), RedBeachClubId, "Cruz", "Garcia", new DateOnly(2012, 2, 11), Gender.Male),
            new(Guid.Parse("b1000000-0000-0000-0000-000000000052"), RedBeachClubId, "Lily", "Chen", new DateOnly(2012, 12, 25), Gender.Female),
            new(Guid.Parse("b1000000-0000-0000-0000-000000000053"), RedBeachClubId, "Jake", "Garcia", new DateOnly(1998, 9, 7), Gender.Male),
            new(Guid.Parse("b1000000-0000-0000-0000-000000000054"), RedBeachClubId, "Chloe", "Chen", new DateOnly(2001, 11, 3), Gender.Female),
            new(Guid.Parse("b1000000-0000-0000-0000-000000000055"), RedBeachClubId, "Max", "Garcia", new DateOnly(2010, 8, 22), Gender.Male),
            new(Guid.Parse("b1000000-0000-0000-0000-000000000056"), RedBeachClubId, "Ruby", "Chen", new DateOnly(2009, 10, 17), Gender.Female),
        ];
    }

    /// <summary>
    /// Live carnival: today's date, multiple events in various states.
    /// - U14 Male Sprint: heats complete with results, final in progress (partial results)
    /// - U14 Female Sprint: heats complete with results, no final yet
    /// - Open Male Beach Flags: heats drawn, one heat has results, one pending
    /// - Open Female Swim: entries only, no draw yet
    /// </summary>
    private static void SeedLiveCarnival(Carnival carnival, List<Member> members)
    {
        var pihaU14M = members.Where(m => m.ClubId == PihaClubId && m.Gender == Gender.Male && m.DateOfBirth.Year == 2012).ToList();
        var murU14M = members.Where(m => m.ClubId == MurraysClubId && m.Gender == Gender.Male && m.DateOfBirth.Year == 2012).ToList();
        var betU14M = members.Where(m => m.ClubId == BethellsClubId && m.Gender == Gender.Male && m.DateOfBirth.Year == 2012).ToList();
        var oreU14M = members.Where(m => m.ClubId == OrewaClubId && m.Gender == Gender.Male && m.DateOfBirth.Year == 2012).ToList();
        var waiU14M = members.Where(m => m.ClubId == WaipuClubId && m.Gender == Gender.Male && m.DateOfBirth.Year == 2012).ToList();
        var redU14M = members.Where(m => m.ClubId == RedBeachClubId && m.Gender == Gender.Male && m.DateOfBirth.Year == 2012).ToList();

        // --- Event 1: U14 Male Sprint — heats done, final in progress ---
        var u14Sprint = new EventDefinition(Guid.Parse("e1000000-0000-0000-0000-000000000001"),
            "U14 Male Sprint", EventCategory.Sprint, AgeGroup.U14, Gender.Male, 4,
            AdvancementRule.TopNPerHeat, 3);
        carnival.AddEvent(u14Sprint);

        // Heat round
        var heatRound = u14Sprint.AddRound(RoundType.Heat);

        var heat1 = new Heat(Guid.Parse("f1000000-0000-0000-0000-000000000001"), 1);
        var e1 = new Entry(Guid.Parse("ee100000-0000-0000-0000-000000000001"), u14Sprint.Id, PihaClubId, pihaU14M[0].Id);
        var e2 = new Entry(Guid.Parse("ee100000-0000-0000-0000-000000000002"), u14Sprint.Id, MurraysClubId, murU14M[0].Id);
        var e3 = new Entry(Guid.Parse("ee100000-0000-0000-0000-000000000003"), u14Sprint.Id, BethellsClubId, betU14M[0].Id);
        heat1.AssignEntry(e1, 1);
        heat1.AssignEntry(e2, 2);
        heat1.AssignEntry(e3, 3);
        heat1.RecordResult(new Result(Guid.Parse("a0100000-0000-0000-0000-000000000001"), e1.Id, new Placing(2), new TimeResult(TimeSpan.FromSeconds(47.23)), status: ResultStatus.Confirmed));
        heat1.RecordResult(new Result(Guid.Parse("a0100000-0000-0000-0000-000000000002"), e2.Id, new Placing(1), new TimeResult(TimeSpan.FromSeconds(45.87)), status: ResultStatus.Confirmed));
        heat1.RecordResult(new Result(Guid.Parse("a0100000-0000-0000-0000-000000000003"), e3.Id, new Placing(3), new TimeResult(TimeSpan.FromSeconds(49.11)), status: ResultStatus.Confirmed));
        heat1.MarkComplete();
        heatRound.AddHeat(heat1);

        var heat2 = new Heat(Guid.Parse("f1000000-0000-0000-0000-000000000002"), 2);
        var e4 = new Entry(Guid.Parse("ee100000-0000-0000-0000-000000000004"), u14Sprint.Id, OrewaClubId, oreU14M[0].Id);
        var e5 = new Entry(Guid.Parse("ee100000-0000-0000-0000-000000000005"), u14Sprint.Id, WaipuClubId, waiU14M[0].Id);
        var e6 = new Entry(Guid.Parse("ee100000-0000-0000-0000-000000000006"), u14Sprint.Id, RedBeachClubId, redU14M[0].Id);
        heat2.AssignEntry(e4, 1);
        heat2.AssignEntry(e5, 2);
        heat2.AssignEntry(e6, 3);
        heat2.RecordResult(new Result(Guid.Parse("a0100000-0000-0000-0000-000000000004"), e4.Id, new Placing(1), new TimeResult(TimeSpan.FromSeconds(44.52)), status: ResultStatus.Confirmed));
        heat2.RecordResult(new Result(Guid.Parse("a0100000-0000-0000-0000-000000000005"), e5.Id, new Placing(3), new TimeResult(TimeSpan.FromSeconds(50.04)), status: ResultStatus.Confirmed));
        heat2.RecordResult(new Result(Guid.Parse("a0100000-0000-0000-0000-000000000006"), e6.Id, new Placing(2), new TimeResult(TimeSpan.FromSeconds(46.33)), status: ResultStatus.Confirmed));
        heat2.MarkComplete();
        heatRound.AddHeat(heat2);
        heatRound.MarkComplete();

        // Final — in progress, partial results (only 2 of 4 entered)
        var finalRound = u14Sprint.AddRound(RoundType.Final);
        var finalHeat = new Heat(Guid.Parse("f1000000-0000-0000-0000-000000000003"), 1);
        var fe1 = new Entry(Guid.Parse("ee100000-0000-0000-0000-000000000011"), u14Sprint.Id, OrewaClubId, oreU14M[0].Id);
        var fe2 = new Entry(Guid.Parse("ee100000-0000-0000-0000-000000000012"), u14Sprint.Id, MurraysClubId, murU14M[0].Id);
        var fe3 = new Entry(Guid.Parse("ee100000-0000-0000-0000-000000000013"), u14Sprint.Id, RedBeachClubId, redU14M[0].Id);
        var fe4 = new Entry(Guid.Parse("ee100000-0000-0000-0000-000000000014"), u14Sprint.Id, PihaClubId, pihaU14M[0].Id);
        finalHeat.AssignEntry(fe1, 1);
        finalHeat.AssignEntry(fe2, 2);
        finalHeat.AssignEntry(fe3, 3);
        finalHeat.AssignEntry(fe4, 4);
        // Only 2 results entered so far — provisional
        finalHeat.RecordResult(new Result(Guid.Parse("a0100000-0000-0000-0000-000000000011"), fe1.Id, new Placing(1), new TimeResult(TimeSpan.FromSeconds(43.91)), status: ResultStatus.Provisional));
        finalHeat.RecordResult(new Result(Guid.Parse("a0100000-0000-0000-0000-000000000012"), fe2.Id, new Placing(2), new TimeResult(TimeSpan.FromSeconds(44.56)), status: ResultStatus.Provisional));
        finalRound.AddHeat(finalHeat);

        // --- Event 2: U14 Female Sprint — heats complete, no final yet ---
        var pihaU14F = members.Where(m => m.ClubId == PihaClubId && m.Gender == Gender.Female && m.DateOfBirth.Year == 2012).ToList();
        var murU14F = members.Where(m => m.ClubId == MurraysClubId && m.Gender == Gender.Female && m.DateOfBirth.Year == 2012).ToList();
        var betU14F = members.Where(m => m.ClubId == BethellsClubId && m.Gender == Gender.Female && m.DateOfBirth.Year == 2012).ToList();
        var oreU14F = members.Where(m => m.ClubId == OrewaClubId && m.Gender == Gender.Female && m.DateOfBirth.Year == 2012).ToList();
        var waiU14F = members.Where(m => m.ClubId == WaipuClubId && m.Gender == Gender.Female && m.DateOfBirth.Year == 2012).ToList();
        var redU14F = members.Where(m => m.ClubId == RedBeachClubId && m.Gender == Gender.Female && m.DateOfBirth.Year == 2012).ToList();

        var u14FSprint = new EventDefinition(Guid.Parse("e1000000-0000-0000-0000-000000000003"),
            "U14 Female Sprint", EventCategory.Sprint, AgeGroup.U14, Gender.Female, 4,
            AdvancementRule.TopNPerHeat, 3);
        carnival.AddEvent(u14FSprint);

        var fHeatRound = u14FSprint.AddRound(RoundType.Heat);
        var fHeat1 = new Heat(Guid.Parse("f1000000-0000-0000-0000-000000000011"), 1);
        var fe11 = new Entry(Guid.Parse("ee100000-0000-0000-0000-000000000021"), u14FSprint.Id, PihaClubId, pihaU14F[0].Id);
        var fe12 = new Entry(Guid.Parse("ee100000-0000-0000-0000-000000000022"), u14FSprint.Id, MurraysClubId, murU14F[0].Id);
        var fe13 = new Entry(Guid.Parse("ee100000-0000-0000-0000-000000000023"), u14FSprint.Id, BethellsClubId, betU14F[0].Id);
        fHeat1.AssignEntry(fe11, 1);
        fHeat1.AssignEntry(fe12, 2);
        fHeat1.AssignEntry(fe13, 3);
        fHeat1.RecordResult(new Result(Guid.Parse("a0100000-0000-0000-0000-000000000021"), fe11.Id, new Placing(1), new TimeResult(TimeSpan.FromSeconds(48.44)), status: ResultStatus.Confirmed));
        fHeat1.RecordResult(new Result(Guid.Parse("a0100000-0000-0000-0000-000000000022"), fe12.Id, new Placing(2), new TimeResult(TimeSpan.FromSeconds(49.18)), status: ResultStatus.Confirmed));
        fHeat1.RecordResult(new Result(Guid.Parse("a0100000-0000-0000-0000-000000000023"), fe13.Id, new Placing(3), new TimeResult(TimeSpan.FromSeconds(51.02)), status: ResultStatus.Confirmed));
        fHeat1.MarkComplete();
        fHeatRound.AddHeat(fHeat1);

        var fHeat2 = new Heat(Guid.Parse("f1000000-0000-0000-0000-000000000012"), 2);
        var fe14 = new Entry(Guid.Parse("ee100000-0000-0000-0000-000000000024"), u14FSprint.Id, OrewaClubId, oreU14F[0].Id);
        var fe15 = new Entry(Guid.Parse("ee100000-0000-0000-0000-000000000025"), u14FSprint.Id, WaipuClubId, waiU14F[0].Id);
        var fe16 = new Entry(Guid.Parse("ee100000-0000-0000-0000-000000000026"), u14FSprint.Id, RedBeachClubId, redU14F[0].Id);
        fHeat2.AssignEntry(fe14, 1);
        fHeat2.AssignEntry(fe15, 2);
        fHeat2.AssignEntry(fe16, 3);
        fHeat2.RecordResult(new Result(Guid.Parse("a0100000-0000-0000-0000-000000000024"), fe14.Id, new Placing(2), new TimeResult(TimeSpan.FromSeconds(49.88)), status: ResultStatus.Confirmed));
        fHeat2.RecordResult(new Result(Guid.Parse("a0100000-0000-0000-0000-000000000025"), fe15.Id, new Placing(1), new TimeResult(TimeSpan.FromSeconds(47.65)), status: ResultStatus.Confirmed));
        fHeat2.RecordResult(new Result(Guid.Parse("a0100000-0000-0000-0000-000000000026"), fe16.Id, new Placing(3), new TimeResult(TimeSpan.FromSeconds(52.30)), status: ResultStatus.Confirmed));
        fHeat2.MarkComplete();
        fHeatRound.AddHeat(fHeat2);
        fHeatRound.MarkComplete();

        // --- Event 3: Open Male Beach Flags — one heat done, one pending ---
        var openMales = members.Where(m => m.Gender == Gender.Male && m.DateOfBirth.Year < 2005).ToList();

        var openFlags = new EventDefinition(Guid.Parse("e1000000-0000-0000-0000-000000000002"),
            "Open Male Beach Flags", EventCategory.Flags, AgeGroup.Open, Gender.Male, 6,
            AdvancementRule.TopNPerHeat, 4);
        carnival.AddEvent(openFlags);

        var flagsHeatRound = openFlags.AddRound(RoundType.Heat);
        var flagsHeat1 = new Heat(Guid.Parse("f1000000-0000-0000-0000-000000000021"), 1);
        var ofe1 = new Entry(Guid.Parse("ee100000-0000-0000-0000-000000000031"), openFlags.Id, PihaClubId, openMales.First(m => m.ClubId == PihaClubId).Id);
        var ofe2 = new Entry(Guid.Parse("ee100000-0000-0000-0000-000000000032"), openFlags.Id, MurraysClubId, openMales.First(m => m.ClubId == MurraysClubId).Id);
        var ofe3 = new Entry(Guid.Parse("ee100000-0000-0000-0000-000000000033"), openFlags.Id, OrewaClubId, openMales.First(m => m.ClubId == OrewaClubId).Id);
        flagsHeat1.AssignEntry(ofe1, 1);
        flagsHeat1.AssignEntry(ofe2, 2);
        flagsHeat1.AssignEntry(ofe3, 3);
        flagsHeat1.RecordResult(new Result(Guid.Parse("a0100000-0000-0000-0000-000000000031"), ofe1.Id, new Placing(1), status: ResultStatus.Confirmed));
        flagsHeat1.RecordResult(new Result(Guid.Parse("a0100000-0000-0000-0000-000000000032"), ofe2.Id, new Placing(3), status: ResultStatus.Confirmed));
        flagsHeat1.RecordResult(new Result(Guid.Parse("a0100000-0000-0000-0000-000000000033"), ofe3.Id, new Placing(2), status: ResultStatus.Confirmed));
        flagsHeat1.MarkComplete();
        flagsHeatRound.AddHeat(flagsHeat1);

        // Heat 2 — drawn but no results yet
        var flagsHeat2 = new Heat(Guid.Parse("f1000000-0000-0000-0000-000000000022"), 2);
        var ofe4 = new Entry(Guid.Parse("ee100000-0000-0000-0000-000000000034"), openFlags.Id, BethellsClubId, openMales.First(m => m.ClubId == BethellsClubId).Id);
        var ofe5 = new Entry(Guid.Parse("ee100000-0000-0000-0000-000000000035"), openFlags.Id, WaipuClubId, openMales.First(m => m.ClubId == WaipuClubId).Id);
        var ofe6 = new Entry(Guid.Parse("ee100000-0000-0000-0000-000000000036"), openFlags.Id, RedBeachClubId, openMales.First(m => m.ClubId == RedBeachClubId).Id);
        flagsHeat2.AssignEntry(ofe4, 1);
        flagsHeat2.AssignEntry(ofe5, 2);
        flagsHeat2.AssignEntry(ofe6, 3);
        flagsHeatRound.AddHeat(flagsHeat2);

        // --- Event 4: Open Female Swim — event defined, no draw ---
        var openFSwim = new EventDefinition(Guid.Parse("e1000000-0000-0000-0000-000000000004"),
            "Open Female Ocean Swim", EventCategory.Swim, AgeGroup.Open, Gender.Female, 8,
            AdvancementRule.TopNPerHeat, 4);
        carnival.AddEvent(openFSwim);

        // --- Event 5: U14 Male Board Race — DQ and DNS examples ---
        var u14Board = new EventDefinition(Guid.Parse("e1000000-0000-0000-0000-000000000005"),
            "U14 Male Board Race", EventCategory.Board, AgeGroup.U14, Gender.Male, 4,
            AdvancementRule.TopNPerHeat, 3);
        carnival.AddEvent(u14Board);

        var boardRound = u14Board.AddRound(RoundType.Final);
        var boardHeat = new Heat(Guid.Parse("f1000000-0000-0000-0000-000000000031"), 1);
        var be1 = new Entry(Guid.Parse("ee100000-0000-0000-0000-000000000041"), u14Board.Id, PihaClubId, pihaU14M[0].Id);
        var be2 = new Entry(Guid.Parse("ee100000-0000-0000-0000-000000000042"), u14Board.Id, MurraysClubId, murU14M[0].Id);
        var be3 = new Entry(Guid.Parse("ee100000-0000-0000-0000-000000000043"), u14Board.Id, OrewaClubId, oreU14M[0].Id);
        var be4 = new Entry(Guid.Parse("ee100000-0000-0000-0000-000000000044"), u14Board.Id, WaipuClubId, waiU14M[0].Id);
        boardHeat.AssignEntry(be1, 1);
        boardHeat.AssignEntry(be2, 2);
        boardHeat.AssignEntry(be3, 3);
        boardHeat.AssignEntry(be4, 4);
        boardHeat.RecordResult(new Result(Guid.Parse("a0100000-0000-0000-0000-000000000041"), be1.Id, new Placing(1), new TimeResult(TimeSpan.FromSeconds(132.44)), status: ResultStatus.Provisional));
        boardHeat.RecordResult(new Result(Guid.Parse("a0100000-0000-0000-0000-000000000042"), be2.Id, status: ResultStatus.Disqualified));
        boardHeat.RecordResult(new Result(Guid.Parse("a0100000-0000-0000-0000-000000000043"), be3.Id, new Placing(2), new TimeResult(TimeSpan.FromSeconds(138.71)), status: ResultStatus.Provisional));
        boardHeat.RecordResult(new Result(Guid.Parse("a0100000-0000-0000-0000-000000000044"), be4.Id, status: ResultStatus.DidNotStart));
        boardRound.AddHeat(boardHeat);
    }

    /// <summary>
    /// Completed carnival: all events finished, all results confirmed.
    /// Small club-level event with 3 events, all finals complete.
    /// </summary>
    private static void SeedCompletedCarnival(Carnival carnival, List<Member> members)
    {
        // --- Event 1: U14 Male Sprint (Final only — small carnival) ---
        var u14Sprint = new EventDefinition(Guid.Parse("e2000000-0000-0000-0000-000000000001"),
            "U14 Male Sprint", EventCategory.Sprint, AgeGroup.U14, Gender.Male, 4,
            AdvancementRule.TopNPerHeat, 3);
        carnival.AddEvent(u14Sprint);

        var round1 = u14Sprint.AddRound(RoundType.Final);
        var heat1 = new Heat(Guid.Parse("f2000000-0000-0000-0000-000000000001"), 1);
        var ce1 = new Entry(Guid.Parse("ee200000-0000-0000-0000-000000000001"), u14Sprint.Id, PihaClubId, members.First(m => m.Id == Guid.Parse("b1000000-0000-0000-0000-000000000001")).Id);
        var ce2 = new Entry(Guid.Parse("ee200000-0000-0000-0000-000000000002"), u14Sprint.Id, MurraysClubId, members.First(m => m.Id == Guid.Parse("b1000000-0000-0000-0000-000000000011")).Id);
        var ce3 = new Entry(Guid.Parse("ee200000-0000-0000-0000-000000000003"), u14Sprint.Id, BethellsClubId, members.First(m => m.Id == Guid.Parse("b1000000-0000-0000-0000-000000000022")).Id);
        var ce4 = new Entry(Guid.Parse("ee200000-0000-0000-0000-000000000004"), u14Sprint.Id, OrewaClubId, members.First(m => m.Id == Guid.Parse("b1000000-0000-0000-0000-000000000031")).Id);
        heat1.AssignEntry(ce1, 1);
        heat1.AssignEntry(ce2, 2);
        heat1.AssignEntry(ce3, 3);
        heat1.AssignEntry(ce4, 4);
        heat1.RecordResult(new Result(Guid.Parse("a2000000-0000-0000-0000-000000000001"), ce1.Id, new Placing(3), new TimeResult(TimeSpan.FromSeconds(48.92)), status: ResultStatus.Confirmed));
        heat1.RecordResult(new Result(Guid.Parse("a2000000-0000-0000-0000-000000000002"), ce2.Id, new Placing(1), new TimeResult(TimeSpan.FromSeconds(44.31)), status: ResultStatus.Confirmed));
        heat1.RecordResult(new Result(Guid.Parse("a2000000-0000-0000-0000-000000000003"), ce3.Id, new Placing(2), new TimeResult(TimeSpan.FromSeconds(46.55)), status: ResultStatus.Confirmed));
        heat1.RecordResult(new Result(Guid.Parse("a2000000-0000-0000-0000-000000000004"), ce4.Id, new Placing(4), new TimeResult(TimeSpan.FromSeconds(50.17)), status: ResultStatus.Confirmed));
        heat1.MarkComplete();
        round1.AddHeat(heat1);
        round1.MarkComplete();

        // --- Event 2: U14 Female Sprint ---
        var u14FSprint = new EventDefinition(Guid.Parse("e2000000-0000-0000-0000-000000000002"),
            "U14 Female Sprint", EventCategory.Sprint, AgeGroup.U14, Gender.Female, 4,
            AdvancementRule.TopNPerHeat, 3);
        carnival.AddEvent(u14FSprint);

        var round2 = u14FSprint.AddRound(RoundType.Final);
        var heat2 = new Heat(Guid.Parse("f2000000-0000-0000-0000-000000000002"), 1);
        var ce5 = new Entry(Guid.Parse("ee200000-0000-0000-0000-000000000005"), u14FSprint.Id, PihaClubId, members.First(m => m.Id == Guid.Parse("b1000000-0000-0000-0000-000000000002")).Id);
        var ce6 = new Entry(Guid.Parse("ee200000-0000-0000-0000-000000000006"), u14FSprint.Id, MurraysClubId, members.First(m => m.Id == Guid.Parse("b1000000-0000-0000-0000-000000000012")).Id);
        var ce7 = new Entry(Guid.Parse("ee200000-0000-0000-0000-000000000007"), u14FSprint.Id, BethellsClubId, members.First(m => m.Id == Guid.Parse("b1000000-0000-0000-0000-000000000021")).Id);
        var ce8 = new Entry(Guid.Parse("ee200000-0000-0000-0000-000000000008"), u14FSprint.Id, OrewaClubId, members.First(m => m.Id == Guid.Parse("b1000000-0000-0000-0000-000000000032")).Id);
        heat2.AssignEntry(ce5, 1);
        heat2.AssignEntry(ce6, 2);
        heat2.AssignEntry(ce7, 3);
        heat2.AssignEntry(ce8, 4);
        heat2.RecordResult(new Result(Guid.Parse("a2000000-0000-0000-0000-000000000005"), ce5.Id, new Placing(2), new TimeResult(TimeSpan.FromSeconds(49.87)), status: ResultStatus.Confirmed));
        heat2.RecordResult(new Result(Guid.Parse("a2000000-0000-0000-0000-000000000006"), ce6.Id, new Placing(1), new TimeResult(TimeSpan.FromSeconds(47.22)), status: ResultStatus.Confirmed));
        heat2.RecordResult(new Result(Guid.Parse("a2000000-0000-0000-0000-000000000007"), ce7.Id, new Placing(4), new TimeResult(TimeSpan.FromSeconds(53.41)), status: ResultStatus.Confirmed));
        heat2.RecordResult(new Result(Guid.Parse("a2000000-0000-0000-0000-000000000008"), ce8.Id, new Placing(3), new TimeResult(TimeSpan.FromSeconds(50.63)), status: ResultStatus.Confirmed));
        heat2.MarkComplete();
        round2.AddHeat(heat2);
        round2.MarkComplete();

        // --- Event 3: Open Male Beach Flags ---
        var openFlags = new EventDefinition(Guid.Parse("e2000000-0000-0000-0000-000000000003"),
            "Open Male Beach Flags", EventCategory.Flags, AgeGroup.Open, Gender.Male, 6,
            AdvancementRule.TopNPerHeat, 4);
        carnival.AddEvent(openFlags);

        var round3 = openFlags.AddRound(RoundType.Final);
        var heat3 = new Heat(Guid.Parse("f2000000-0000-0000-0000-000000000003"), 1);
        var ce9 = new Entry(Guid.Parse("ee200000-0000-0000-0000-000000000009"), openFlags.Id, PihaClubId, members.First(m => m.Id == Guid.Parse("b1000000-0000-0000-0000-000000000003")).Id);
        var ce10 = new Entry(Guid.Parse("ee200000-0000-0000-0000-000000000010"), openFlags.Id, MurraysClubId, members.First(m => m.Id == Guid.Parse("b1000000-0000-0000-0000-000000000013")).Id);
        var ce11 = new Entry(Guid.Parse("ee200000-0000-0000-0000-000000000011"), openFlags.Id, BethellsClubId, members.First(m => m.Id == Guid.Parse("b1000000-0000-0000-0000-000000000024")).Id);
        var ce12 = new Entry(Guid.Parse("ee200000-0000-0000-0000-000000000012"), openFlags.Id, OrewaClubId, members.First(m => m.Id == Guid.Parse("b1000000-0000-0000-0000-000000000033")).Id);
        heat3.AssignEntry(ce9, 1);
        heat3.AssignEntry(ce10, 2);
        heat3.AssignEntry(ce11, 3);
        heat3.AssignEntry(ce12, 4);
        heat3.RecordResult(new Result(Guid.Parse("a2000000-0000-0000-0000-000000000009"), ce9.Id, new Placing(2), status: ResultStatus.Confirmed));
        heat3.RecordResult(new Result(Guid.Parse("a2000000-0000-0000-0000-000000000010"), ce10.Id, new Placing(1), status: ResultStatus.Confirmed));
        heat3.RecordResult(new Result(Guid.Parse("a2000000-0000-0000-0000-000000000011"), ce11.Id, new Placing(4), status: ResultStatus.Confirmed));
        heat3.RecordResult(new Result(Guid.Parse("a2000000-0000-0000-0000-000000000012"), ce12.Id, new Placing(3), status: ResultStatus.Confirmed));
        heat3.MarkComplete();
        round3.AddHeat(heat3);
        round3.MarkComplete();
    }

    /// <summary>
    /// Upcoming carnival: events and draw generated, but no results.
    /// Regional-level event with entries assigned to heats.
    /// </summary>
    private static void SeedUpcomingCarnival(Carnival carnival, List<Member> members)
    {
        // --- Event 1: U14 Male Sprint ---
        var u14Sprint = new EventDefinition(Guid.Parse("e3000000-0000-0000-0000-000000000001"),
            "U14 Male Sprint", EventCategory.Sprint, AgeGroup.U14, Gender.Male, 4,
            AdvancementRule.TopNPerHeat, 3);
        carnival.AddEvent(u14Sprint);

        var round1 = u14Sprint.AddRound(RoundType.Heat);
        var heat1 = new Heat(Guid.Parse("f3000000-0000-0000-0000-000000000001"), 1);
        heat1.AssignEntry(new Entry(Guid.Parse("ee300000-0000-0000-0000-000000000001"), u14Sprint.Id, PihaClubId, Guid.Parse("b1000000-0000-0000-0000-000000000001")), 1);
        heat1.AssignEntry(new Entry(Guid.Parse("ee300000-0000-0000-0000-000000000002"), u14Sprint.Id, MurraysClubId, Guid.Parse("b1000000-0000-0000-0000-000000000011")), 2);
        heat1.AssignEntry(new Entry(Guid.Parse("ee300000-0000-0000-0000-000000000003"), u14Sprint.Id, OrewaClubId, Guid.Parse("b1000000-0000-0000-0000-000000000031")), 3);
        heat1.AssignEntry(new Entry(Guid.Parse("ee300000-0000-0000-0000-000000000004"), u14Sprint.Id, RedBeachClubId, Guid.Parse("b1000000-0000-0000-0000-000000000051")), 4);
        round1.AddHeat(heat1);

        var heat2 = new Heat(Guid.Parse("f3000000-0000-0000-0000-000000000002"), 2);
        heat2.AssignEntry(new Entry(Guid.Parse("ee300000-0000-0000-0000-000000000005"), u14Sprint.Id, BethellsClubId, Guid.Parse("b1000000-0000-0000-0000-000000000022")), 1);
        heat2.AssignEntry(new Entry(Guid.Parse("ee300000-0000-0000-0000-000000000006"), u14Sprint.Id, WaipuClubId, Guid.Parse("b1000000-0000-0000-0000-000000000041")), 2);
        round1.AddHeat(heat2);

        // --- Event 2: Open Female Ocean Swim ---
        var openFSwim = new EventDefinition(Guid.Parse("e3000000-0000-0000-0000-000000000002"),
            "Open Female Ocean Swim", EventCategory.Swim, AgeGroup.Open, Gender.Female, 8,
            AdvancementRule.TopNPerHeat, 4);
        carnival.AddEvent(openFSwim);

        var round2 = openFSwim.AddRound(RoundType.Final);
        var swimHeat = new Heat(Guid.Parse("f3000000-0000-0000-0000-000000000011"), 1);
        swimHeat.AssignEntry(new Entry(Guid.Parse("ee300000-0000-0000-0000-000000000011"), openFSwim.Id, PihaClubId, Guid.Parse("b1000000-0000-0000-0000-000000000004")), 1);
        swimHeat.AssignEntry(new Entry(Guid.Parse("ee300000-0000-0000-0000-000000000012"), openFSwim.Id, MurraysClubId, Guid.Parse("b1000000-0000-0000-0000-000000000014")), 2);
        swimHeat.AssignEntry(new Entry(Guid.Parse("ee300000-0000-0000-0000-000000000013"), openFSwim.Id, BethellsClubId, Guid.Parse("b1000000-0000-0000-0000-000000000023")), 3);
        swimHeat.AssignEntry(new Entry(Guid.Parse("ee300000-0000-0000-0000-000000000014"), openFSwim.Id, OrewaClubId, Guid.Parse("b1000000-0000-0000-0000-000000000034")), 4);
        swimHeat.AssignEntry(new Entry(Guid.Parse("ee300000-0000-0000-0000-000000000015"), openFSwim.Id, WaipuClubId, Guid.Parse("b1000000-0000-0000-0000-000000000044")), 5);
        swimHeat.AssignEntry(new Entry(Guid.Parse("ee300000-0000-0000-0000-000000000016"), openFSwim.Id, RedBeachClubId, Guid.Parse("b1000000-0000-0000-0000-000000000054")), 6);
        round2.AddHeat(swimHeat);

        // --- Event 3: U17 Male Board Race (entries only, no draw) ---
        var u17Board = new EventDefinition(Guid.Parse("e3000000-0000-0000-0000-000000000003"),
            "U17 Male Board Race", EventCategory.Board, AgeGroup.U17, Gender.Male, 4,
            AdvancementRule.TopNPerHeat, 3);
        carnival.AddEvent(u17Board);

        // --- Event 4: Open Male Beach Flags ---
        var openFlags = new EventDefinition(Guid.Parse("e3000000-0000-0000-0000-000000000004"),
            "Open Male Beach Flags", EventCategory.Flags, AgeGroup.Open, Gender.Male, 6,
            AdvancementRule.TopNPerHeat, 4);
        carnival.AddEvent(openFlags);

        var round4 = openFlags.AddRound(RoundType.Heat);
        var flagsHeat1 = new Heat(Guid.Parse("f3000000-0000-0000-0000-000000000021"), 1);
        flagsHeat1.AssignEntry(new Entry(Guid.Parse("ee300000-0000-0000-0000-000000000021"), openFlags.Id, PihaClubId, Guid.Parse("b1000000-0000-0000-0000-000000000003")), 1);
        flagsHeat1.AssignEntry(new Entry(Guid.Parse("ee300000-0000-0000-0000-000000000022"), openFlags.Id, MurraysClubId, Guid.Parse("b1000000-0000-0000-0000-000000000013")), 2);
        flagsHeat1.AssignEntry(new Entry(Guid.Parse("ee300000-0000-0000-0000-000000000023"), openFlags.Id, BethellsClubId, Guid.Parse("b1000000-0000-0000-0000-000000000024")), 3);
        round4.AddHeat(flagsHeat1);

        var flagsHeat2 = new Heat(Guid.Parse("f3000000-0000-0000-0000-000000000022"), 2);
        flagsHeat2.AssignEntry(new Entry(Guid.Parse("ee300000-0000-0000-0000-000000000024"), openFlags.Id, OrewaClubId, Guid.Parse("b1000000-0000-0000-0000-000000000033")), 1);
        flagsHeat2.AssignEntry(new Entry(Guid.Parse("ee300000-0000-0000-0000-000000000025"), openFlags.Id, WaipuClubId, Guid.Parse("b1000000-0000-0000-0000-000000000043")), 2);
        flagsHeat2.AssignEntry(new Entry(Guid.Parse("ee300000-0000-0000-0000-000000000026"), openFlags.Id, RedBeachClubId, Guid.Parse("b1000000-0000-0000-0000-000000000053")), 3);
        round4.AddHeat(flagsHeat2);
    }
}
