import Foundation
import GRDB

public struct Entry: Codable, Identifiable, Sendable {
    public var id: UUID
    public var eventDefinitionId: UUID
    public var heatId: UUID?
    public var clubId: UUID
    public var memberIds: [UUID]
    public var lane: Int?
    public var isWithdrawn: Bool

    public init(id: UUID, eventDefinitionId: UUID, heatId: UUID? = nil, clubId: UUID, memberIds: [UUID], lane: Int? = nil, isWithdrawn: Bool = false) {
        self.id = id
        self.eventDefinitionId = eventDefinitionId
        self.heatId = heatId
        self.clubId = clubId
        self.memberIds = memberIds
        self.lane = lane
        self.isWithdrawn = isWithdrawn
    }
}

extension Entry: FetchableRecord, PersistableRecord {
    public static let databaseTableName = "entries"

    public enum Columns {
        static let memberIds = Column(CodingKeys.memberIds)
    }
}
