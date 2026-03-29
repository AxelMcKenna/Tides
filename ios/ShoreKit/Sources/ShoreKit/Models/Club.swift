import Foundation
import GRDB

public struct Club: Codable, Identifiable, Sendable {
    public var id: UUID
    public var regionId: UUID
    public var name: String
    public var abbreviation: String

    public init(id: UUID, regionId: UUID, name: String, abbreviation: String) {
        self.id = id
        self.regionId = regionId
        self.name = name
        self.abbreviation = abbreviation
    }
}

extension Club: FetchableRecord, PersistableRecord {
    public static let databaseTableName = "clubs"
}
