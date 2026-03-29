import Foundation
import GRDB

public struct EventDefinition: Codable, Identifiable, Sendable {
    public var id: UUID
    public var carnivalId: UUID
    public var name: String
    public var category: String
    public var ageGroup: String
    public var gender: String
    public var maxLanes: Int
    public var advancementRule: String
    public var advanceTopN: Int
    public var advanceFastestN: Int

    public init(id: UUID, carnivalId: UUID, name: String, category: String, ageGroup: String, gender: String, maxLanes: Int, advancementRule: String = "TopNPerHeat", advanceTopN: Int = 3, advanceFastestN: Int = 0) {
        self.id = id
        self.carnivalId = carnivalId
        self.name = name
        self.category = category
        self.ageGroup = ageGroup
        self.gender = gender
        self.maxLanes = maxLanes
        self.advancementRule = advancementRule
        self.advanceTopN = advanceTopN
        self.advanceFastestN = advanceFastestN
    }
}

extension EventDefinition: FetchableRecord, PersistableRecord {
    public static let databaseTableName = "event_definitions"
}
