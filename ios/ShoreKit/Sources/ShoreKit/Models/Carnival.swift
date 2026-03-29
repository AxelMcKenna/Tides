import Foundation
import GRDB

public struct Carnival: Codable, Identifiable, Sendable {
    public var id: UUID
    public var name: String
    public var hostingClubId: UUID
    public var sanction: String
    public var startDate: String
    public var endDate: String

    public init(id: UUID, name: String, hostingClubId: UUID, sanction: String, startDate: String, endDate: String) {
        self.id = id
        self.name = name
        self.hostingClubId = hostingClubId
        self.sanction = sanction
        self.startDate = startDate
        self.endDate = endDate
    }
}

extension Carnival: FetchableRecord, PersistableRecord {
    public static let databaseTableName = "carnivals"
}
