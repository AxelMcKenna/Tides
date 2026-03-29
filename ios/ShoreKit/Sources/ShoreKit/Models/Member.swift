import Foundation
import GRDB

public struct Member: Codable, Identifiable, Sendable {
    public var id: UUID
    public var clubId: UUID
    public var firstName: String
    public var lastName: String
    public var gender: String
    public var ageGroup: String?
    public var surfguardId: String?

    public var fullName: String { "\(firstName) \(lastName)" }

    public init(id: UUID, clubId: UUID, firstName: String, lastName: String, gender: String, ageGroup: String? = nil, surfguardId: String? = nil) {
        self.id = id
        self.clubId = clubId
        self.firstName = firstName
        self.lastName = lastName
        self.gender = gender
        self.ageGroup = ageGroup
        self.surfguardId = surfguardId
    }
}

extension Member: FetchableRecord, PersistableRecord {
    public static let databaseTableName = "members"
}
