import Foundation
import GRDB

public struct SyncQueueItem: Codable, Identifiable, Sendable {
    public var id: Int64?
    public var type: String
    public var payload: String
    public var createdAt: Date
    public var attempts: Int
    public var lastError: String?

    public init(type: String, payload: String) {
        self.type = type
        self.payload = payload
        self.createdAt = .now
        self.attempts = 0
    }
}

extension SyncQueueItem: FetchableRecord, PersistableRecord {
    public static let databaseTableName = "sync_queue"
}
