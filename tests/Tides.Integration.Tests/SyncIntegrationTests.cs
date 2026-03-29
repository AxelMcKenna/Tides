using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Tides.Api.Dtos;
using Tides.Api.Hubs;
using Tides.Api.Services;
using Tides.Core.Domain;
using Tides.Core.Domain.Enums;
using Tides.Core.Domain.ValueObjects;
using Tides.Infrastructure.Persistence;

namespace Tides.Integration.Tests;

[Collection("Database")]
public class SyncIntegrationTests(DatabaseFixture fixture)
{
    private SyncService CreateService(TidesDbContext db)
    {
        var hubContext = new FakeHubContext();
        return new SyncService(db, hubContext);
    }

    private async Task<(Guid carnivalId, Guid heatId, Guid entryId, Guid clubId)> SeedCarnivalWithHeat()
    {
        await using var db = fixture.CreateContext();

        var org = new Organisation(Guid.NewGuid(), "SLSNZ-" + Guid.NewGuid(), "NZ");
        var region = new Region(Guid.NewGuid(), org.Id, "Northern");
        var club = new Club(Guid.NewGuid(), region.Id, "Piha", "PIH");
        var member = new Member(Guid.NewGuid(), club.Id, "Test", "Athlete",
            new DateOnly(2010, 1, 1), Gender.Male, null);

        var carnival = new Carnival(Guid.NewGuid(), "Test Carnival", club.Id,
            SanctionLevel.Club, DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today));
        carnival.Activate();

        var eventDef = new EventDefinition(Guid.NewGuid(), "Sprint",
            EventCategory.Sprint, AgeGroup.U15, Gender.Male, 6);
        carnival.AddEvent(eventDef);

        var round = eventDef.AddRound(RoundType.Final);

        var heat = new Heat(Guid.NewGuid(), 1);
        round.AddHeat(heat);

        var entry = new Entry(Guid.NewGuid(), eventDef.Id, club.Id, [member.Id]);
        heat.AssignEntry(entry, 1);

        db.Add(org);
        db.Add(region);
        db.Add(club);
        db.Add(member);
        db.Carnivals.Add(carnival);

        await db.SaveChangesAsync();

        return (carnival.Id, heat.Id, entry.Id, club.Id);
    }

    [Fact]
    public async Task Sync_NewResult_IsAcknowledged()
    {
        var (carnivalId, heatId, entryId, _) = await SeedCarnivalWithHeat();

        await using var db = fixture.CreateContext();
        var service = CreateService(db);

        var eventId = Guid.NewGuid();
        var request = new SyncBatchRequest(carnivalId, "device-1", "recorder-1",
        [
            new SyncEventRequest(eventId, heatId, entryId, 1, null, null,
                "Provisional", DateTime.UtcNow, 1)
        ]);

        var response = await service.ProcessBatchAsync(request);

        Assert.Empty(response.Errors);
        Assert.Empty(response.Conflicts);
        Assert.Single(response.Acknowledged);
        Assert.Equal(eventId, response.Acknowledged[0].EventId);
        Assert.Equal(2, response.Acknowledged[0].NewHeatVersion);
    }

    [Fact]
    public async Task Sync_DuplicateEvent_IsDeduplicatedOnRetry()
    {
        var (carnivalId, heatId, entryId, _) = await SeedCarnivalWithHeat();

        var eventId = Guid.NewGuid();

        // First sync
        {
            await using var db = fixture.CreateContext();
            var service = CreateService(db);
            var request = new SyncBatchRequest(carnivalId, "device-1", "recorder-1",
            [
                new SyncEventRequest(eventId, heatId, entryId, 1, null, null,
                    "Provisional", DateTime.UtcNow, 1)
            ]);
            var response = await service.ProcessBatchAsync(request);
            Assert.Single(response.Acknowledged);
        }

        // Retry — same eventId, fresh context
        {
            await using var db = fixture.CreateContext();
            var service = CreateService(db);
            var retryRequest = new SyncBatchRequest(carnivalId, "device-1", "recorder-1",
            [
                new SyncEventRequest(eventId, heatId, entryId, 1, null, null,
                    "Provisional", DateTime.UtcNow, 1)
            ]);
            var response = await service.ProcessBatchAsync(retryRequest);

            Assert.Single(response.Acknowledged);
            Assert.Empty(response.Errors);

            // Only one result in the database
            var resultCount = await db.Results.CountAsync(r => r.HeatId == heatId);
            Assert.Equal(1, resultCount);

            // Only one event in the log
            var eventCount = await db.ResultEvents.CountAsync(e => e.EventId == eventId);
            Assert.Equal(1, eventCount);
        }
    }

    [Fact]
    public async Task Sync_VersionMismatch_ReturnsConflict()
    {
        var (carnivalId, heatId, entryId, clubId) = await SeedCarnivalWithHeat();

        // First sync advances version to 2
        {
            await using var db = fixture.CreateContext();
            var service = CreateService(db);
            var request = new SyncBatchRequest(carnivalId, "device-1", "recorder-1",
            [
                new SyncEventRequest(Guid.NewGuid(), heatId, entryId, 1, null, null,
                    "Provisional", DateTime.UtcNow, 1)
            ]);
            var response = await service.ProcessBatchAsync(request);
            Assert.Single(response.Acknowledged);
        }

        // Add a second entry to the heat
        Guid entry2Id;
        {
            await using var db = fixture.CreateContext();
            var regionId = await db.Regions.Select(r => r.Id).FirstAsync();
            var eventDefId = await db.Events.Select(e => e.Id).FirstAsync();

            var club2 = new Club(Guid.NewGuid(), regionId, "Muriwai", "MUR");
            var member2 = new Member(Guid.NewGuid(), club2.Id, "Second", "Athlete",
                new DateOnly(2010, 1, 1), Gender.Male, null);
            db.Add(club2);
            db.Add(member2);

            var heat = await db.Heats.Include(h => h.Entries).FirstAsync(h => h.Id == heatId);
            var entry2 = new Entry(Guid.NewGuid(), eventDefId, club2.Id, [member2.Id]);
            heat.AssignEntry(entry2, 2);
            entry2Id = entry2.Id;
            await db.SaveChangesAsync();
        }

        // Second device syncs with stale version (1)
        {
            await using var db = fixture.CreateContext();
            var service = CreateService(db);
            var request = new SyncBatchRequest(carnivalId, "device-2", "recorder-2",
            [
                new SyncEventRequest(Guid.NewGuid(), heatId, entry2Id, 1, null, null,
                    "Provisional", DateTime.UtcNow, 1) // expectedVersion=1, but actual=2
            ]);
            var response = await service.ProcessBatchAsync(request);

            Assert.Empty(response.Acknowledged);
            Assert.Single(response.Conflicts);
            Assert.Equal(1, response.Conflicts[0].ExpectedVersion);
        }
    }

    [Fact]
    public async Task Sync_HeatNotFound_ReturnsError()
    {
        var (carnivalId, _, _, _) = await SeedCarnivalWithHeat();

        await using var db = fixture.CreateContext();
        var service = CreateService(db);

        var request = new SyncBatchRequest(carnivalId, "device-1", "recorder-1",
        [
            new SyncEventRequest(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                1, null, null, "Provisional", DateTime.UtcNow, 1)
        ]);

        var response = await service.ProcessBatchAsync(request);

        Assert.Empty(response.Acknowledged);
        Assert.Single(response.Errors);
        Assert.Equal("HEAT_NOT_FOUND", response.Errors[0].ErrorCode);
    }

    [Fact]
    public async Task Sync_InactiveCarnival_Throws()
    {
        await using var seedDb = fixture.CreateContext();
        var org = new Organisation(Guid.NewGuid(), "Test Org-" + Guid.NewGuid(), "NZ");
        var region = new Region(Guid.NewGuid(), org.Id, "Test");
        var club = new Club(Guid.NewGuid(), region.Id, "Test Club", "TST");
        var carnival = new Carnival(Guid.NewGuid(), "Draft Carnival", club.Id,
            SanctionLevel.Club, DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today));
        // Status is Draft — not activated

        seedDb.Add(org);
        seedDb.Add(region);
        seedDb.Add(club);
        seedDb.Carnivals.Add(carnival);
        await seedDb.SaveChangesAsync();

        await using var db = fixture.CreateContext();
        var service = CreateService(db);
        var request = new SyncBatchRequest(carnival.Id, "device-1", "recorder-1",
        [
            new SyncEventRequest(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                1, null, null, "Provisional", DateTime.UtcNow, 1)
        ]);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ProcessBatchAsync(request));
    }

    [Fact]
    public async Task Sync_BatchSizeExceeded_Throws()
    {
        var (carnivalId, heatId, entryId, _) = await SeedCarnivalWithHeat();

        await using var db = fixture.CreateContext();
        var service = CreateService(db);

        var events = Enumerable.Range(0, 51)
            .Select(_ => new SyncEventRequest(Guid.NewGuid(), heatId, entryId,
                1, null, null, "Provisional", DateTime.UtcNow, 1))
            .ToList();

        var request = new SyncBatchRequest(carnivalId, "device-1", "recorder-1", events);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ProcessBatchAsync(request));
    }

    [Fact]
    public async Task Sync_PartialBatchSuccess_SomeAckedSomeErrored()
    {
        var (carnivalId, heatId, entryId, _) = await SeedCarnivalWithHeat();

        await using var db = fixture.CreateContext();
        var service = CreateService(db);

        var request = new SyncBatchRequest(carnivalId, "device-1", "recorder-1",
        [
            // Valid event
            new SyncEventRequest(Guid.NewGuid(), heatId, entryId, 1, null, null,
                "Provisional", DateTime.UtcNow, 1),
            // Invalid — bad heat
            new SyncEventRequest(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                1, null, null, "Provisional", DateTime.UtcNow, 1)
        ]);

        var response = await service.ProcessBatchAsync(request);

        Assert.Single(response.Acknowledged);
        Assert.Single(response.Errors);
    }

    [Fact]
    public async Task Sync_EventLog_IsAppendOnly()
    {
        var (carnivalId, heatId, entryId, _) = await SeedCarnivalWithHeat();
        var eventId = Guid.NewGuid();

        {
            await using var db = fixture.CreateContext();
            var service = CreateService(db);

            var request = new SyncBatchRequest(carnivalId, "device-1", "recorder-1",
            [
                new SyncEventRequest(eventId, heatId, entryId, 1, null, null,
                    "Provisional", DateTime.UtcNow, 1)
            ]);

            await service.ProcessBatchAsync(request);
        }

        await using var readDb = fixture.CreateContext();
        var logEntry = await readDb.ResultEvents.FirstOrDefaultAsync(e => e.EventId == eventId);
        Assert.NotNull(logEntry);
        Assert.Equal(carnivalId, logEntry.CarnivalId);
        Assert.Equal(heatId, logEntry.HeatId);
        Assert.Equal(entryId, logEntry.EntryId);
        Assert.Equal("device-1", logEntry.DeviceId);
        Assert.Null(logEntry.SupersededBy);
    }

    // Fake SignalR hub context for tests — does nothing
    private class FakeHubContext : IHubContext<ResultsHub>
    {
        public IHubClients Clients => new FakeHubClients();
        public IGroupManager Groups => null!;
    }

    private class FakeHubClients : IHubClients
    {
        public IClientProxy Client(string connectionId) => new FakeClientProxy();
        public IClientProxy Group(string groupName) => new FakeClientProxy();
        public IClientProxy All => new FakeClientProxy();
        public IClientProxy AllExcept(IReadOnlyList<string> excludedConnectionIds) => new FakeClientProxy();
        public IClientProxy Clients(IReadOnlyList<string> connectionIds) => new FakeClientProxy();
        public IClientProxy Groups(IReadOnlyList<string> groupNames) => new FakeClientProxy();
        public IClientProxy GroupExcept(string groupName, IReadOnlyList<string> excludedConnectionIds) => new FakeClientProxy();
        public IClientProxy User(string userId) => new FakeClientProxy();
        public IClientProxy Users(IReadOnlyList<string> userIds) => new FakeClientProxy();
    }

    private class FakeClientProxy : IClientProxy
    {
        public Task SendCoreAsync(string method, object?[] args,
            CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
