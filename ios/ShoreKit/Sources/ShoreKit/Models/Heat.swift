import Foundation
import GRDB

public struct Heat: Codable, Identifiable, Sendable {
    public var id: UUID
    public var roundId: UUID
    public var heatNumber: Int
    public var isComplete: Bool

    public init(id: UUID, roundId: UUID, heatNumber: Int, isComplete: Bool = false) {
        self.id = id
        self.roundId = roundId
        self.heatNumber = heatNumber
        self.isComplete = isComplete
    }
}

extension Heat: FetchableRecord, PersistableRecord {
    public static let databaseTableName = "heats"
}
