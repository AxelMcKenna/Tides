import XCTest
import GRDB
@testable import ShoreKit

final class ShoreDatabaseTests: XCTestCase {
    func testCreateInMemoryDatabase() throws {
        let db = try ShoreDatabase()
        let count = try db.read { db in
            try Carnival.fetchCount(db)
        }
        XCTAssertEqual(count, 0)
    }

    func testInsertAndFetchCarnival() throws {
        let db = try ShoreDatabase()
        let carnival = Carnival(
            id: UUID(),
            name: "Test Carnival",
            hostingClubId: UUID(),
            sanction: "Branch",
            startDate: "2026-04-11",
            endDate: "2026-04-12"
        )

        try db.write { db in
            try carnival.insert(db)
        }

        let fetched = try db.read { db in
            try Carnival.fetchOne(db, key: carnival.id)
        }
        XCTAssertEqual(fetched?.name, "Test Carnival")
    }

    func testInsertFullHierarchy() throws {
        let db = try ShoreDatabase()

        let carnival = Carnival(id: UUID(), name: "Test", hostingClubId: UUID(), sanction: "Club", startDate: "2026-04-11", endDate: "2026-04-11")
        let club = Club(id: UUID(), branchId: UUID(), name: "Test Club", abbreviation: "TST")
        let eventDef = EventDefinition(id: UUID(), carnivalId: carnival.id, name: "Sprint", category: "Sprint", ageGroup: "U14", gender: "Male", maxLanes: 4)
        let round = Round(id: UUID(), eventDefinitionId: eventDef.id, type: "Heat", roundNumber: 1)
        let heat = Heat(id: UUID(), roundId: round.id, heatNumber: 1)
        let entry = Entry(id: UUID(), eventDefinitionId: eventDef.id, heatId: heat.id, clubId: club.id, memberIds: [UUID()])
        let result = Result(id: UUID(), heatId: heat.id, entryId: entry.id, placing: 1, status: "Provisional")

        try db.write { db in
            try carnival.insert(db)
            try club.insert(db)
            try eventDef.insert(db)
            try round.insert(db)
            try heat.insert(db)
            try entry.insert(db)
            try result.insert(db)
        }

        let results = try db.read { db in
            try Result.fetchAll(db)
        }
        XCTAssertEqual(results.count, 1)
        XCTAssertEqual(results[0].placing, 1)
    }

    func testSyncQueue() throws {
        let db = try ShoreDatabase()

        let item = SyncQueueItem(type: "RecordResult", payload: "{\"test\": true}")
        try db.write { db in
            try item.insert(db)
        }

        let count = try db.read { db in
            try SyncQueueItem.fetchCount(db)
        }
        XCTAssertEqual(count, 1)
    }
}
