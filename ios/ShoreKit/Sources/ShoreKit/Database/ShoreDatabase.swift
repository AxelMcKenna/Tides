import Foundation
import GRDB

public final class ShoreDatabase: Sendable {
    private let dbQueue: DatabaseQueue

    public init(path: String) throws {
        dbQueue = try DatabaseQueue(path: path)
        try migrate()
    }

    /// In-memory database for testing
    public init() throws {
        dbQueue = try DatabaseQueue()
        try migrate()
    }

    private func migrate() throws {
        var migrator = DatabaseMigrator()

        migrator.registerMigration("v1") { db in
            try db.create(table: "carnivals") { t in
                t.column("id", .text).primaryKey()
                t.column("name", .text).notNull()
                t.column("hostingClubId", .text).notNull()
                t.column("sanction", .text).notNull()
                t.column("startDate", .text).notNull()
                t.column("endDate", .text).notNull()
            }

            try db.create(table: "clubs") { t in
                t.column("id", .text).primaryKey()
                t.column("branchId", .text).notNull()
                t.column("name", .text).notNull()
                t.column("abbreviation", .text).notNull()
            }

            try db.create(table: "members") { t in
                t.column("id", .text).primaryKey()
                t.column("clubId", .text).notNull().references("clubs")
                t.column("firstName", .text).notNull()
                t.column("lastName", .text).notNull()
                t.column("gender", .text).notNull()
                t.column("ageGroup", .text)
                t.column("surfguardId", .text)
            }

            try db.create(table: "event_definitions") { t in
                t.column("id", .text).primaryKey()
                t.column("carnivalId", .text).notNull().references("carnivals")
                t.column("name", .text).notNull()
                t.column("category", .text).notNull()
                t.column("ageGroup", .text).notNull()
                t.column("gender", .text).notNull()
                t.column("maxLanes", .integer).notNull()
                t.column("advancementRule", .text).notNull()
                t.column("advanceTopN", .integer).notNull()
                t.column("advanceFastestN", .integer).notNull()
            }

            try db.create(table: "rounds") { t in
                t.column("id", .text).primaryKey()
                t.column("eventDefinitionId", .text).notNull().references("event_definitions")
                t.column("type", .text).notNull()
                t.column("roundNumber", .integer).notNull()
                t.column("isComplete", .boolean).notNull().defaults(to: false)
            }

            try db.create(table: "heats") { t in
                t.column("id", .text).primaryKey()
                t.column("roundId", .text).notNull().references("rounds")
                t.column("heatNumber", .integer).notNull()
                t.column("isComplete", .boolean).notNull().defaults(to: false)
            }

            try db.create(table: "entries") { t in
                t.column("id", .text).primaryKey()
                t.column("eventDefinitionId", .text).notNull().references("event_definitions")
                t.column("heatId", .text).references("heats")
                t.column("clubId", .text).notNull().references("clubs")
                t.column("memberIds", .text).notNull() // JSON array
                t.column("lane", .integer)
                t.column("isWithdrawn", .boolean).notNull().defaults(to: false)
            }

            try db.create(table: "results") { t in
                t.column("id", .text).primaryKey()
                t.column("heatId", .text).notNull().references("heats")
                t.column("entryId", .text).notNull().references("entries")
                t.column("placing", .integer)
                t.column("time", .text)
                t.column("judgeScore", .double)
                t.column("status", .text).notNull().defaults(to: "Provisional")
            }

            try db.create(table: "protests") { t in
                t.column("id", .text).primaryKey()
                t.column("carnivalId", .text).notNull().references("carnivals")
                t.column("eventId", .text).notNull()
                t.column("heatId", .text)
                t.column("lodgedByClubId", .text).notNull()
                t.column("reason", .text).notNull()
                t.column("status", .text).notNull().defaults(to: "Lodged")
                t.column("adjudicationReason", .text)
                t.column("lodgedAt", .datetime).notNull()
                t.column("adjudicatedAt", .datetime)
            }

            // Sync queue for offline writes
            try db.create(table: "sync_queue") { t in
                t.autoIncrementedPrimaryKey("id")
                t.column("type", .text).notNull()
                t.column("payload", .text).notNull() // JSON
                t.column("createdAt", .datetime).notNull()
                t.column("attempts", .integer).notNull().defaults(to: 0)
                t.column("lastError", .text)
            }
        }

        try migrator.migrate(dbQueue)
    }

    // MARK: - Read

    public func read<T>(_ block: (Database) throws -> T) throws -> T {
        try dbQueue.read(block)
    }

    // MARK: - Write

    public func write<T>(_ block: (Database) throws -> T) throws -> T {
        try dbQueue.write(block)
    }
}
