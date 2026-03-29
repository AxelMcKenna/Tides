import Foundation
import GRDB

public struct Protest: Codable, Identifiable, Sendable {
    public var id: UUID
    public var carnivalId: UUID
    public var eventId: UUID
    public var heatId: UUID?
    public var lodgedByClubId: UUID
    public var reason: String
    public var status: String
    public var adjudicationReason: String?
    public var lodgedAt: Date
    public var adjudicatedAt: Date?

    public init(id: UUID, carnivalId: UUID, eventId: UUID, heatId: UUID? = nil, lodgedByClubId: UUID, reason: String, status: String = "Lodged", adjudicationReason: String? = nil, lodgedAt: Date = .now, adjudicatedAt: Date? = nil) {
        self.id = id
        self.carnivalId = carnivalId
        self.eventId = eventId
        self.heatId = heatId
        self.lodgedByClubId = lodgedByClubId
        self.reason = reason
        self.status = status
        self.adjudicationReason = adjudicationReason
        self.lodgedAt = lodgedAt
        self.adjudicatedAt = adjudicatedAt
    }
}

extension Protest: FetchableRecord, PersistableRecord {
    public static let databaseTableName = "protests"
}
