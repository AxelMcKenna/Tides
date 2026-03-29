import Foundation
import GRDB

public struct Result: Codable, Identifiable, Sendable {
    public var id: UUID
    public var heatId: UUID
    public var entryId: UUID
    public var placing: Int?
    public var time: String?
    public var judgeScore: Double?
    public var status: String

    public init(id: UUID, heatId: UUID, entryId: UUID, placing: Int? = nil, time: String? = nil, judgeScore: Double? = nil, status: String = "Provisional") {
        self.id = id
        self.heatId = heatId
        self.entryId = entryId
        self.placing = placing
        self.time = time
        self.judgeScore = judgeScore
        self.status = status
    }
}

extension Result: FetchableRecord, PersistableRecord {
    public static let databaseTableName = "results"
}
