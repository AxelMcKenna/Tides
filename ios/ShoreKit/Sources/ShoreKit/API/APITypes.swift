import Foundation

// MARK: - Carnival

public struct CarnivalResponse: Codable, Sendable {
    public let id: UUID
    public let name: String
    public let hostingClubId: UUID
    public let sanction: String
    public let startDate: String
    public let endDate: String
    public let events: [EventSummaryResponse]
}

public struct EventSummaryResponse: Codable, Sendable {
    public let id: UUID
    public let name: String
    public let category: String
    public let ageGroup: String
    public let gender: String
    public let maxLanes: Int
    public let roundCount: Int
    public let hasResults: Bool
}

// MARK: - Draw

public struct DrawResponse: Codable, Sendable {
    public let carnivalId: UUID
    public let events: [EventDrawResponse]
}

public struct EventDrawResponse: Codable, Sendable {
    public let eventId: UUID
    public let eventName: String
    public let rounds: [RoundDrawResponse]
}

public struct RoundDrawResponse: Codable, Sendable {
    public let roundId: UUID
    public let roundType: String
    public let roundNumber: Int
    public let heats: [HeatDrawResponse]
}

public struct HeatDrawResponse: Codable, Sendable {
    public let heatId: UUID
    public let heatNumber: Int
    public let isComplete: Bool
    public let entries: [LaneEntryResponse]
}

public struct LaneEntryResponse: Codable, Sendable {
    public let entryId: UUID
    public let lane: Int?
    public let clubId: UUID
    public let clubName: String
    public let members: [MemberBriefResponse]
    public let isWithdrawn: Bool
}

public struct MemberBriefResponse: Codable, Sendable {
    public let id: UUID
    public let firstName: String
    public let lastName: String
}

// MARK: - Results

public struct CarnivalResultsResponse: Codable, Sendable {
    public let carnivalId: UUID
    public let events: [EventResultsResponse]
}

public struct EventResultsResponse: Codable, Sendable {
    public let eventId: UUID
    public let eventName: String
    public let ageGroup: String
    public let gender: String
    public let heats: [HeatResultsResponse]
}

public struct HeatResultsResponse: Codable, Sendable {
    public let heatId: UUID
    public let heatNumber: Int
    public let roundType: String
    public let isComplete: Bool
    public let results: [ResultResponse]
}

public struct ResultResponse: Codable, Sendable {
    public let id: UUID
    public let heatId: UUID
    public let entryId: UUID
    public let placing: Int?
    public let time: String?
    public let judgeScore: Double?
    public let status: String
    public let clubId: UUID
    public let clubName: String
    public let members: [MemberBriefResponse]
}

// MARK: - Leaderboard

public struct LeaderboardResponse: Codable, Sendable {
    public let carnivalId: UUID
    public let standings: [ClubStandingResponse]
}

public struct ClubStandingResponse: Codable, Sendable {
    public let rank: Int
    public let clubId: UUID
    public let clubName: String
    public let clubAbbreviation: String
    public let totalPoints: Double
}

// MARK: - Requests

public struct RecordResultRequest: Codable, Sendable {
    public let eventId: UUID
    public let roundId: UUID
    public let heatId: UUID
    public let entryId: UUID
    public let placing: Int?
    public let time: String?
    public let judgeScore: Double?
    public let status: String

    public init(eventId: UUID, roundId: UUID, heatId: UUID, entryId: UUID, placing: Int? = nil, time: String? = nil, judgeScore: Double? = nil, status: String = "Provisional") {
        self.eventId = eventId
        self.roundId = roundId
        self.heatId = heatId
        self.entryId = entryId
        self.placing = placing
        self.time = time
        self.judgeScore = judgeScore
        self.status = status
    }
}

public struct CorrectResultRequest: Codable, Sendable {
    public let newPlacing: Int?
    public let newTime: String?
    public let reason: String

    public init(newPlacing: Int? = nil, newTime: String? = nil, reason: String) {
        self.newPlacing = newPlacing
        self.newTime = newTime
        self.reason = reason
    }
}

public struct CreateEntryRequest: Codable, Sendable {
    public let eventId: UUID
    public let clubId: UUID
    public let memberIds: [UUID]

    public init(eventId: UUID, clubId: UUID, memberIds: [UUID]) {
        self.eventId = eventId
        self.clubId = clubId
        self.memberIds = memberIds
    }
}

public struct GenerateDrawRequest: Codable, Sendable {
    public let eventId: UUID
    public let roundType: String

    public init(eventId: UUID, roundType: String) {
        self.eventId = eventId
        self.roundType = roundType
    }
}
