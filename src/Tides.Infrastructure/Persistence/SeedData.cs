using Tides.Core.Domain;
using Tides.Core.Domain.Enums;
using Tides.Core.Domain.ValueObjects;

namespace Tides.Infrastructure.Persistence;

public static class SeedData
{
    // Clubs
    static readonly Guid PihaId = G("a4000001"), MuriwaiId = G("a4000002"), BethellsId = G("a4000003"),
        OrewaId = G("a4000004"), WaipuId = G("a4000005"), RedBeachId = G("a4000006"),
        MtMaunganuiId = G("a4000007"), PapmoaId = G("a4000008"), OmanuId = G("a4000009"),
        MairangiBayId = G("a400000a"), KareKareId = G("a400000b"), RaglanId = G("a400000c");

    static Guid G(string short7) => Guid.Parse($"{short7}-0000-0000-0000-000000000000");
    static Guid Seq(int prefix, int n) => Guid.Parse($"{prefix:x2}{n:x6}-0000-0000-0000-000000000000");

    public static void Seed(TidesDbContext context)
    {
        if (context.Organisations.Any()) return;

        // Org hierarchy
        var slsnz = new Organisation(G("a1000001"), "Surf Life Saving New Zealand", "NZ");
        context.Organisations.Add(slsnz);
        var northern = new Region(G("a2000001"), slsnz.Id, "Northern Region");
        var bop = new Region(G("a2000002"), slsnz.Id, "Bay of Plenty");
        var waikato = new Region(G("a2000003"), slsnz.Id, "Waikato");
        context.Regions.AddRange(northern, bop, waikato);
        var auckland = new Branch(G("a3000001"), northern.Id, "Auckland");
        var tauranga = new Branch(G("a3000002"), bop.Id, "Tauranga");
        var waikatoBranch = new Branch(G("a3000003"), waikato.Id, "Waikato");
        context.Branches.AddRange(auckland, tauranga, waikatoBranch);

        // 12 Clubs
        Club[] clubs = [
            new(PihaId, auckland.Id, "Piha Surf Life Saving Club", "PIH"),
            new(MuriwaiId, auckland.Id, "Muriwai Volunteer Lifeguard Service", "MUR"),
            new(BethellsId, auckland.Id, "Bethells Beach SLSP", "BET"),
            new(OrewaId, auckland.Id, "Orewa Surf Life Saving Club", "ORE"),
            new(WaipuId, auckland.Id, "Waipu Cove SLSC", "WAI"),
            new(RedBeachId, auckland.Id, "Red Beach Surf Life Saving Club", "RED"),
            new(MtMaunganuiId, tauranga.Id, "Mount Maunganui Lifeguard Service", "MTM"),
            new(PapmoaId, tauranga.Id, "Papamoa Surf Life Saving Club", "PAP"),
            new(OmanuId, tauranga.Id, "Omanu Beach SLSC", "OMA"),
            new(MairangiBayId, auckland.Id, "Mairangi Bay Surf Lifesaving Club", "MAI"),
            new(KareKareId, auckland.Id, "Karekare SLSC", "KAR"),
            new(RaglanId, waikatoBranch.Id, "Raglan Surf Life Saving Club", "RAG"),
        ];
        context.Clubs.AddRange(clubs);

        // Members — 4 per club across age groups (48 total)
        var members = new List<Member>();
        var clubIds = clubs.Select(c => c.Id).ToArray();
        string[][] names = [
            ["Tane","Walker"], ["Aroha","Smith"], ["Jack","Wilson"], ["Mia","Anderson"],
            ["Nikau","Brown"], ["Kaia","Jones"], ["Liam","Taylor"], ["Sophie","Brown"],
            ["Maia","Davis"], ["Reuben","Moore"], ["Isla","Thomas"], ["Ethan","Davis"],
            ["Hunter","Lee"], ["Willow","Martin"], ["Ben","Lee"], ["Emma","Martin"],
            ["Koa","Harris"], ["Ava","White"], ["Sam","Harris"], ["Grace","White"],
            ["Cruz","Garcia"], ["Lily","Chen"], ["Jake","Garcia"], ["Chloe","Chen"],
            ["Finn","Walker"], ["Lila","Wilson"], ["Caleb","Jones"], ["Zoe","Taylor"],
            ["Harper","Moore"], ["Mason","Thomas"], ["Riley","Lee"], ["Ivy","Martin"],
            ["Leo","Harris"], ["Ella","White"], ["Max","Garcia"], ["Ruby","Chen"],
            ["Tama","Ngata"], ["Hine","Patel"], ["Josh","Scott"], ["Aria","Thompson"],
            ["Te Koha","Henare"], ["Manaia","Wiremu"], ["Dylan","Fraser"], ["Olive","Campbell"],
            ["Rua","Matiu"], ["Anika","Sharma"], ["Brody","Mitchell"], ["Sienna","Murphy"],
        ];
        var dobs = new[] {
            new DateOnly(2012,5,10), new DateOnly(2012,8,22), new DateOnly(1998,3,14), new DateOnly(1999,6,8),
        };
        var genders = new[] { Gender.Male, Gender.Female, Gender.Male, Gender.Female };
        for (var i = 0; i < 48; i++)
        {
            var clubId = clubIds[i / 4];
            members.Add(new Member(Seq(0xb1, i + 1), clubId, names[i][0], names[i][1],
                dobs[i % 4].AddDays(i * 17), genders[i % 4]));
        }
        context.Members.AddRange(members);

        // Points table factory
        PointsTable Pts(Guid id) => new(id, "Standard", [
            new(1, 8), new(2, 6), new(3, 4), new(4, 3), new(5, 2), new(6, 1)
        ]);

        // Helper: create a seeded final with results for an event
        void SeedFinal(EventDefinition evt, List<(Guid clubId, Guid memberId)> entries,
            int[] placings, int heatSeq, int resultSeq, ResultStatus status = ResultStatus.Confirmed)
        {
            var round = evt.AddRound(RoundType.Final);
            var heat = new Heat(Seq(0xf0, heatSeq), 1);
            for (var i = 0; i < entries.Count; i++)
            {
                var entry = new Entry(Seq(0xee, heatSeq * 100 + i + 1), evt.Id, entries[i].clubId, entries[i].memberId);
                heat.AssignEntry(entry, i + 1);
                var placing = i < placings.Length ? new Placing(placings[i]) : (Placing?)null;
                heat.RecordResult(new Result(Seq(0xa0, resultSeq * 100 + i + 1), entry.Id, placing, status: status));
            }
            heat.MarkComplete();
            round.AddHeat(heat);
            round.MarkComplete();
        }

        // Helper: members by club
        List<Member> ByClub(Guid clubId) => members.Where(m => m.ClubId == clubId).ToList();

        // ====================================================================
        // CARNIVAL 1: Live — Auckland Branch Championships (today)
        // ====================================================================
        var c1 = new Carnival(Seq(0xc1, 1), "Auckland Branch Championships 2026",
            PihaId, SanctionLevel.Branch, new DateOnly(2026, 3, 29), new DateOnly(2026, 3, 30));
        c1.SetPointsTable(Pts(Seq(0xd1, 1)));

        // 8 events across age groups
        var c1Events = new (string name, EventCategory cat, AgeGroup age, Gender gender)[]
        {
            ("U14 Male Sprint", EventCategory.Sprint, AgeGroup.U14, Gender.Male),
            ("U14 Female Sprint", EventCategory.Sprint, AgeGroup.U14, Gender.Female),
            ("Open Male Beach Flags", EventCategory.Flags, AgeGroup.Open, Gender.Male),
            ("Open Female Beach Flags", EventCategory.Flags, AgeGroup.Open, Gender.Female),
            ("U14 Male Board Race", EventCategory.Board, AgeGroup.U14, Gender.Male),
            ("Open Male Ocean Swim", EventCategory.Swim, AgeGroup.Open, Gender.Male),
            ("Open Female Ocean Swim", EventCategory.Swim, AgeGroup.Open, Gender.Female),
            ("Open Male Ironman", EventCategory.Ironman, AgeGroup.Open, Gender.Male),
        };
        var c1Clubs = new[] { PihaId, MuriwaiId, BethellsId, OrewaId, WaipuId, RedBeachId };
        for (var e = 0; e < c1Events.Length; e++)
        {
            var (name, cat, age, gender) = c1Events[e];
            var evt = new EventDefinition(Seq(0xe1, e + 1), name, cat, age, gender, 6);
            c1.AddEvent(evt);

            // Get one member per club matching gender
            var entrants = c1Clubs
                .Select(cid => ByClub(cid).FirstOrDefault(m => m.Gender == gender))
                .Where(m => m != null)
                .Select(m => (clubId: m!.ClubId, memberId: m.Id))
                .ToList();

            if (e < 5) // First 5 events have results
            {
                var placingOrder = Enumerable.Range(1, entrants.Count).ToArray();
                var st = e < 3 ? ResultStatus.Confirmed : ResultStatus.Provisional;
                SeedFinal(evt, entrants, placingOrder, 100 + e * 10, 100 + e * 10, st);
            }
            else if (e == 5) // Event 6: heats drawn, no results
            {
                var round = evt.AddRound(RoundType.Heat);
                var h1 = new Heat(Seq(0xf0, 200 + e), 1);
                for (var i = 0; i < 3 && i < entrants.Count; i++)
                    h1.AssignEntry(new Entry(Seq(0xee, 900 + e * 10 + i), evt.Id, entrants[i].clubId, entrants[i].memberId), i + 1);
                round.AddHeat(h1);
                var h2 = new Heat(Seq(0xf0, 210 + e), 2);
                for (var i = 3; i < entrants.Count; i++)
                    h2.AssignEntry(new Entry(Seq(0xee, 900 + e * 10 + i), evt.Id, entrants[i].clubId, entrants[i].memberId), i - 2);
                round.AddHeat(h2);
            }
            // Events 7-8: just defined, no draw
        }
        context.Carnivals.Add(c1);

        // ====================================================================
        // CARNIVAL 2: Completed — Piha Classic (past)
        // ====================================================================
        var c2 = new Carnival(Seq(0xc1, 2), "Piha Classic 2026",
            PihaId, SanctionLevel.Club, new DateOnly(2026, 2, 15), new DateOnly(2026, 2, 15));
        c2.SetPointsTable(Pts(Seq(0xd1, 2)));
        var c2Events = new[] { "U14 Male Sprint", "U14 Female Sprint", "Open Male Beach Flags", "Open Female Ocean Swim" };
        var c2Clubs = new[] { PihaId, MuriwaiId, BethellsId, OrewaId };
        for (var e = 0; e < c2Events.Length; e++)
        {
            var gender = e % 2 == 0 ? Gender.Male : Gender.Female;
            var cat = e < 2 ? EventCategory.Sprint : e == 2 ? EventCategory.Flags : EventCategory.Swim;
            var age = e < 2 ? AgeGroup.U14 : AgeGroup.Open;
            var evt = new EventDefinition(Seq(0xe2, e + 1), c2Events[e], cat, age, gender, 4);
            c2.AddEvent(evt);
            var entrants = c2Clubs.Select(cid => ByClub(cid).FirstOrDefault(m => m.Gender == gender))
                .Where(m => m != null).Select(m => (clubId: m!.ClubId, memberId: m.Id)).ToList();
            SeedFinal(evt, entrants, [1, 2, 3, 4], 300 + e * 10, 300 + e * 10);
        }
        context.Carnivals.Add(c2);

        // ====================================================================
        // CARNIVAL 3: Completed — Mt Maunganui Summer Series (past)
        // ====================================================================
        var c3 = new Carnival(Seq(0xc1, 3), "Mount Summer Series Round 3",
            MtMaunganuiId, SanctionLevel.Club, new DateOnly(2026, 1, 24), new DateOnly(2026, 1, 24));
        c3.SetPointsTable(Pts(Seq(0xd1, 3)));
        var c3Clubs = new[] { MtMaunganuiId, PapmoaId, OmanuId };
        var c3Events2 = new[] { "U14 Male Sprint", "Open Male Board Race", "Open Female Sprint" };
        for (var e = 0; e < c3Events2.Length; e++)
        {
            var gender = e == 2 ? Gender.Female : Gender.Male;
            var cat = e == 1 ? EventCategory.Board : EventCategory.Sprint;
            var age = e == 0 ? AgeGroup.U14 : AgeGroup.Open;
            var evt = new EventDefinition(Seq(0xe3, e + 1), c3Events2[e], cat, age, gender, 4);
            c3.AddEvent(evt);
            var entrants = c3Clubs.Select(cid => ByClub(cid).FirstOrDefault(m => m.Gender == gender))
                .Where(m => m != null).Select(m => (clubId: m!.ClubId, memberId: m.Id)).ToList();
            SeedFinal(evt, entrants, [1, 2, 3], 400 + e * 10, 400 + e * 10);
        }
        context.Carnivals.Add(c3);

        // ====================================================================
        // CARNIVAL 4: Completed — Orewa New Year Carnival (past)
        // ====================================================================
        var c4 = new Carnival(Seq(0xc1, 4), "Orewa New Year Carnival 2026",
            OrewaId, SanctionLevel.Club, new DateOnly(2026, 1, 3), new DateOnly(2026, 1, 3));
        c4.SetPointsTable(Pts(Seq(0xd1, 4)));
        var c4Clubs = new[] { OrewaId, RedBeachId, MairangiBayId, WaipuId };
        foreach (var (eName, eCat, eAge, eGender, eIdx) in new[] {
            ("U14 Male Sprint", EventCategory.Sprint, AgeGroup.U14, Gender.Male, 1),
            ("U14 Female Sprint", EventCategory.Sprint, AgeGroup.U14, Gender.Female, 2),
            ("Open Male Beach Flags", EventCategory.Flags, AgeGroup.Open, Gender.Male, 3),
            ("Open Female Beach Flags", EventCategory.Flags, AgeGroup.Open, Gender.Female, 4),
            ("U14 Male Board Race", EventCategory.Board, AgeGroup.U14, Gender.Male, 5),
        })
        {
            var evt = new EventDefinition(Seq(0xe4, eIdx), eName, eCat, eAge, eGender, 4);
            c4.AddEvent(evt);
            var entrants = c4Clubs.Select(cid => ByClub(cid).FirstOrDefault(m => m.Gender == eGender))
                .Where(m => m != null).Select(m => (clubId: m!.ClubId, memberId: m.Id)).ToList();
            SeedFinal(evt, entrants, Enumerable.Range(1, entrants.Count).ToArray(), 500 + eIdx * 10, 500 + eIdx * 10);
        }
        context.Carnivals.Add(c4);

        // ====================================================================
        // CARNIVAL 5: Completed — Papamoa Beach Challenge (past)
        // ====================================================================
        var c5 = new Carnival(Seq(0xc1, 5), "Papamoa Beach Challenge 2025",
            PapmoaId, SanctionLevel.Club, new DateOnly(2025, 12, 6), new DateOnly(2025, 12, 6));
        c5.SetPointsTable(Pts(Seq(0xd1, 5)));
        var c5Clubs = new[] { PapmoaId, OmanuId, MtMaunganuiId };
        for (var e = 0; e < 3; e++)
        {
            var gender = e == 1 ? Gender.Female : Gender.Male;
            var evt = new EventDefinition(Seq(0xe5, e + 1), e == 0 ? "Open Male Sprint" : e == 1 ? "Open Female Sprint" : "Open Male Ironman",
                e == 2 ? EventCategory.Ironman : EventCategory.Sprint, AgeGroup.Open, gender, 4);
            c5.AddEvent(evt);
            var entrants = c5Clubs.Select(cid => ByClub(cid).FirstOrDefault(m => m.Gender == gender))
                .Where(m => m != null).Select(m => (clubId: m!.ClubId, memberId: m.Id)).ToList();
            SeedFinal(evt, entrants, [1, 2, 3], 600 + e * 10, 600 + e * 10);
        }
        context.Carnivals.Add(c5);

        // ====================================================================
        // CARNIVAL 6: Upcoming — Northern Region Championships
        // ====================================================================
        var c6 = new Carnival(Seq(0xc1, 6), "Northern Region Championships 2026",
            OrewaId, SanctionLevel.State, new DateOnly(2026, 5, 10), new DateOnly(2026, 5, 11));
        c6.SetPointsTable(Pts(Seq(0xd1, 6)));
        foreach (var (eName, eCat, eAge, eGender, eIdx) in new[] {
            ("U14 Male Sprint", EventCategory.Sprint, AgeGroup.U14, Gender.Male, 1),
            ("U14 Female Sprint", EventCategory.Sprint, AgeGroup.U14, Gender.Female, 2),
            ("Open Male Beach Flags", EventCategory.Flags, AgeGroup.Open, Gender.Male, 3),
            ("Open Female Ocean Swim", EventCategory.Swim, AgeGroup.Open, Gender.Female, 4),
            ("Open Male Ironman", EventCategory.Ironman, AgeGroup.Open, Gender.Male, 5),
            ("U14 Male Board Race", EventCategory.Board, AgeGroup.U14, Gender.Male, 6),
        })
        {
            var evt = new EventDefinition(Seq(0xe6, eIdx), eName, eCat, eAge, eGender, 6);
            c6.AddEvent(evt);
            // Draw for first 3 events
            if (eIdx <= 3)
            {
                var allClubs = new[] { PihaId, MuriwaiId, BethellsId, OrewaId, WaipuId, RedBeachId };
                var round = evt.AddRound(RoundType.Heat);
                var h1 = new Heat(Seq(0xf0, 700 + eIdx * 10), 1);
                for (var i = 0; i < 3; i++)
                {
                    var m = ByClub(allClubs[i]).FirstOrDefault(mm => mm.Gender == eGender);
                    if (m != null) h1.AssignEntry(new Entry(Seq(0xee, 700 + eIdx * 10 + i), evt.Id, allClubs[i], m.Id), i + 1);
                }
                round.AddHeat(h1);
                var h2 = new Heat(Seq(0xf0, 700 + eIdx * 10 + 1), 2);
                for (var i = 3; i < 6; i++)
                {
                    var m = ByClub(allClubs[i]).FirstOrDefault(mm => mm.Gender == eGender);
                    if (m != null) h2.AssignEntry(new Entry(Seq(0xee, 700 + eIdx * 10 + i), evt.Id, allClubs[i], m.Id), i - 2);
                }
                round.AddHeat(h2);
            }
        }
        context.Carnivals.Add(c6);

        // ====================================================================
        // CARNIVAL 7: Upcoming — Raglan Surf Challenge
        // ====================================================================
        var c7 = new Carnival(Seq(0xc1, 7), "Raglan Surf Challenge 2026",
            RaglanId, SanctionLevel.Club, new DateOnly(2026, 4, 19), new DateOnly(2026, 4, 19));
        c7.SetPointsTable(Pts(Seq(0xd1, 7)));
        for (var e = 0; e < 4; e++)
        {
            var gender = e % 2 == 0 ? Gender.Male : Gender.Female;
            var evt = new EventDefinition(Seq(0xe7, e + 1),
                e < 2 ? $"Open {(gender == Gender.Male ? "Male" : "Female")} Sprint" : $"Open {(gender == Gender.Male ? "Male" : "Female")} Board Race",
                e < 2 ? EventCategory.Sprint : EventCategory.Board, AgeGroup.Open, gender, 4);
            c7.AddEvent(evt);
        }
        context.Carnivals.Add(c7);

        // ====================================================================
        // CARNIVAL 8: Upcoming — NZ National Championships
        // ====================================================================
        var c8 = new Carnival(Seq(0xc1, 8), "NZ National Championships 2026",
            MtMaunganuiId, SanctionLevel.National, new DateOnly(2026, 7, 5), new DateOnly(2026, 7, 6));
        c8.SetPointsTable(Pts(Seq(0xd1, 8)));
        foreach (var (eName, eCat, eAge, eGender, eIdx) in new[] {
            ("U14 Male Sprint", EventCategory.Sprint, AgeGroup.U14, Gender.Male, 1),
            ("U14 Female Sprint", EventCategory.Sprint, AgeGroup.U14, Gender.Female, 2),
            ("U17 Male Sprint", EventCategory.Sprint, AgeGroup.U17, Gender.Male, 3),
            ("U17 Female Sprint", EventCategory.Sprint, AgeGroup.U17, Gender.Female, 4),
            ("Open Male Sprint", EventCategory.Sprint, AgeGroup.Open, Gender.Male, 5),
            ("Open Female Sprint", EventCategory.Sprint, AgeGroup.Open, Gender.Female, 6),
            ("Open Male Beach Flags", EventCategory.Flags, AgeGroup.Open, Gender.Male, 7),
            ("Open Female Beach Flags", EventCategory.Flags, AgeGroup.Open, Gender.Female, 8),
            ("Open Male Board Race", EventCategory.Board, AgeGroup.Open, Gender.Male, 9),
            ("Open Female Board Race", EventCategory.Board, AgeGroup.Open, Gender.Female, 10),
            ("Open Male Ocean Swim", EventCategory.Swim, AgeGroup.Open, Gender.Male, 11),
            ("Open Male Ironman", EventCategory.Ironman, AgeGroup.Open, Gender.Male, 12),
        })
        {
            c8.AddEvent(new EventDefinition(Seq(0xe8, eIdx), eName, eCat, eAge, eGender, 8));
        }
        context.Carnivals.Add(c8);

        context.SaveChanges();
    }
}
