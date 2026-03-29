import Foundation
import GRDB

/// Downloads a full carnival snapshot from the API into local GRDB.
/// Called once at the start of a carnival day when connectivity is available.
public actor CarnivalSyncService {
    private let db: TidesDatabase
    private let api: APIClient

    public enum SyncState: Sendable {
        case idle
        case syncing(step: String)
        case completed(Date)
        case failed(String)
    }

    public private(set) var state: SyncState = .idle

    public init(db: TidesDatabase, api: APIClient) {
        self.db = db
        self.api = api
    }

    /// Full sync for a carnival. Downloads everything needed to operate offline.
    public func sync(carnivalId: UUID) async -> SyncState {
        state = .syncing(step: "Fetching carnival...")
        do {
            // 1. Carnival + events
            let carnival = try await api.getCarnival(carnivalId)
            state = .syncing(step: "Saving carnival...")
            try saveCarnival(carnival)

            // 2. Draw (contains rounds, heats, entries with members and clubs)
            state = .syncing(step: "Fetching draw...")
            let draw = try await api.getDraw(carnivalId)
            state = .syncing(step: "Saving draw...")
            try saveDraw(draw, carnivalId: carnivalId)

            // 3. Members for all participating clubs
            let clubIds = extractClubIds(from: draw)
            state = .syncing(step: "Fetching members (\(clubIds.count) clubs)...")
            for clubId in clubIds {
                let members = try await api.getMembers(clubId: clubId)
                try saveMembers(members, clubId: clubId)
            }

            // 4. Existing results (for multi-day carnivals)
            state = .syncing(step: "Fetching results...")
            let results = try await api.getResults(carnivalId)
            try saveResults(results)

            state = .completed(.now)
            return state
        } catch {
            state = .failed(error.localizedDescription)
            return state
        }
    }

    // MARK: - Save Carnival

    private func saveCarnival(_ response: CarnivalResponse) throws {
        try db.write { db in
            // Carnival
            let carnival = Carnival(
                id: response.id,
                name: response.name,
                hostingClubId: response.hostingClubId,
                sanction: response.sanction,
                startDate: response.startDate,
                endDate: response.endDate
            )
            try carnival.save(db)

            // Events
            for event in response.events {
                let eventDef = EventDefinition(
                    id: event.id,
                    carnivalId: response.id,
                    name: event.name,
                    category: event.category,
                    ageGroup: event.ageGroup,
                    gender: event.gender,
                    maxLanes: event.maxLanes
                )
                try eventDef.save(db)
            }
        }
    }

    // MARK: - Save Draw

    private func saveDraw(_ draw: DrawResponse, carnivalId: UUID) throws {
        try db.write { db in
            for event in draw.events {
                for round in event.rounds {
                    let roundRecord = Round(
                        id: round.roundId,
                        eventDefinitionId: event.eventId,
                        type: round.roundType,
                        roundNumber: round.roundNumber
                    )
                    try roundRecord.save(db)

                    for heat in round.heats {
                        let heatRecord = Heat(
                            id: heat.heatId,
                            roundId: round.roundId,
                            heatNumber: heat.heatNumber,
                            isComplete: heat.isComplete
                        )
                        try heatRecord.save(db)

                        for entry in heat.entries {
                            // Save club from entry data
                            let club = Club(
                                id: entry.clubId,
                                branchId: UUID(), // Not available from draw — placeholder
                                name: entry.clubName,
                                abbreviation: String(entry.clubName.prefix(3).uppercased())
                            )
                            try club.save(db)

                            // Save members from entry data
                            for member in entry.members {
                                let memberRecord = Member(
                                    id: member.id,
                                    clubId: entry.clubId,
                                    firstName: member.firstName,
                                    lastName: member.lastName,
                                    gender: "" // Not available from draw brief
                                )
                                try memberRecord.save(db)
                            }

                            // Save entry
                            let entryRecord = Entry(
                                id: entry.entryId,
                                eventDefinitionId: event.eventId,
                                heatId: heat.heatId,
                                clubId: entry.clubId,
                                memberIds: entry.members.map(\.id),
                                lane: entry.lane,
                                isWithdrawn: entry.isWithdrawn
                            )
                            try entryRecord.save(db)
                        }
                    }
                }
            }
        }
    }

    // MARK: - Save Members (full records from /clubs/:id/members)

    private func saveMembers(_ members: [MemberResponse], clubId: UUID) throws {
        try db.write { db in
            for m in members {
                let member = Member(
                    id: m.id,
                    clubId: clubId,
                    firstName: m.firstName,
                    lastName: m.lastName,
                    gender: m.gender,
                    ageGroup: m.ageGroup,
                    surfguardId: m.surfguardId
                )
                try member.save(db) // Overwrites the brief record from draw
            }
        }
    }

    // MARK: - Save Results

    private func saveResults(_ response: CarnivalResultsResponse) throws {
        try db.write { db in
            for event in response.events {
                for heat in event.heats {
                    for result in heat.results {
                        let record = ShoreKit.Result(
                            id: result.id,
                            heatId: result.heatId,
                            entryId: result.entryId,
                            placing: result.placing,
                            time: result.time,
                            judgeScore: result.judgeScore,
                            status: result.status
                        )
                        try record.save(db)
                    }
                }
            }
        }
    }

    // MARK: - Helpers

    private func extractClubIds(from draw: DrawResponse) -> Set<UUID> {
        var ids = Set<UUID>()
        for event in draw.events {
            for round in event.rounds {
                for heat in round.heats {
                    for entry in heat.entries {
                        ids.insert(entry.clubId)
                    }
                }
            }
        }
        return ids
    }
}
