import Foundation
import GRDB

public struct Round: Codable, Identifiable, Sendable {
    public var id: UUID
    public var eventDefinitionId: UUID
    public var type: String
    public var roundNumber: Int
    public var isComplete: Bool

    public init(id: UUID, eventDefinitionId: UUID, type: String, roundNumber: Int, isComplete: Bool = false) {
        self.id = id
        self.eventDefinitionId = eventDefinitionId
        self.type = type
        self.roundNumber = roundNumber
        self.isComplete = isComplete
    }
}

extension Round: FetchableRecord, PersistableRecord {
    public static let databaseTableName = "rounds"
}
