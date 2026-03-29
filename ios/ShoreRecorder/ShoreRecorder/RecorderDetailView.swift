import SwiftUI
import ShoreKit

struct RecorderDetailView: View {
    @Environment(AppState.self) private var appState
    @State private var events: [EventDefinition] = []
    @State private var selectedEvent: EventDefinition?

    var body: some View {
        NavigationSplitView {
            eventList
        } detail: {
            if let selectedEvent {
                ResultEntryView(event: selectedEvent)
            } else {
                ContentUnavailableView("Select an Event", systemImage: "list.bullet",
                    description: Text("Choose an event from the left to enter results."))
            }
        }
        .navigationTitle(appState.selectedCarnival?.name ?? "")
        .task { await loadData() }
    }

    private var eventList: some View {
        List(events, id: \.id, selection: Binding(
            get: { selectedEvent?.id },
            set: { id in selectedEvent = events.first { $0.id == id } }
        )) { event in
            HStack {
                VStack(alignment: .leading, spacing: 2) {
                    Text(event.name).font(.headline)
                    HStack(spacing: 4) {
                        Text(event.category)
                        Text("·")
                        Text(event.ageGroup)
                        Text("·")
                        Text(event.gender)
                    }
                    .font(.caption)
                    .foregroundStyle(.secondary)
                }
                Spacer()
                if hasResults(for: event.id) {
                    Image(systemName: "checkmark.circle.fill")
                        .foregroundStyle(.green)
                        .font(.caption)
                }
            }
            .tag(event.id)
        }
        .navigationTitle("Events")
    }

    private func loadData() async {
        guard let carnival = appState.selectedCarnival else { return }
        // Load from local GRDB — offline-first
        events = (try? appState.db.read { db in
            try EventDefinition
                .filter(Column("carnivalId") == carnival.id.uuidString)
                .fetchAll(db)
        }) ?? []
    }

    private func hasResults(for eventId: UUID) -> Bool {
        (try? appState.db.read { db in
            // Check if any results exist for heats in this event's rounds
            let sql = """
                SELECT COUNT(*) FROM results r
                JOIN heats h ON r.heatId = h.id
                JOIN rounds rd ON h.roundId = rd.id
                WHERE rd.eventDefinitionId = ?
                """
            return try Int.fetchOne(db, sql: sql, arguments: [eventId.uuidString]) ?? 0
        } ?? 0) > 0
    }
}

// MARK: - Local view model for heat entries

struct HeatEntry: Identifiable {
    let id: UUID          // entry ID
    let heatId: UUID
    let lane: Int?
    let clubName: String
    let memberNames: String
    let isWithdrawn: Bool
    let entryCount: Int   // total entries in heat
}

struct LocalRound: Identifiable {
    let id: UUID
    let type: String
    let roundNumber: Int
    let heats: [LocalHeat]
}

struct LocalHeat: Identifiable {
    let id: UUID
    let heatNumber: Int
    let entries: [HeatEntry]
}

// MARK: - Result Entry

struct ResultEntryView: View {
    let event: EventDefinition

    @Environment(AppState.self) private var appState
    @State private var rounds: [LocalRound] = []
    @State private var placings: [UUID: EntryResult] = [:]
    @State private var selectedHeatIndex = 0
    @State private var isSaving = false
    @State private var savedMessage: String?

    private var currentRound: LocalRound? { rounds.last }
    private var heats: [LocalHeat] { currentRound?.heats ?? [] }
    private var currentHeat: LocalHeat? {
        guard heats.indices.contains(selectedHeatIndex) else { return nil }
        return heats[selectedHeatIndex]
    }

    var body: some View {
        VStack(spacing: 0) {
            header
            Divider()

            if let heat = currentHeat {
                ScrollView {
                    VStack(spacing: 0) {
                        ForEach(heat.entries.filter { !$0.isWithdrawn }) { entry in
                            entryRow(entry: entry)
                            Divider().padding(.leading, 16)
                        }
                    }
                }
                Divider()
                bottomBar(heat: heat)
            } else {
                Spacer()
                ContentUnavailableView("No Draw", systemImage: "rectangle.split.3x1",
                    description: Text("Generate a draw for this event first."))
                Spacer()
            }
        }
        .task { loadFromDB() }
        .onChange(of: event.id) {
            placings.removeAll()
            selectedHeatIndex = 0
            loadFromDB()
        }
    }

    // MARK: - Load from local GRDB

    private func loadFromDB() {
        rounds = (try? appState.db.read { db in
            let roundRecords = try Round
                .filter(Column("eventDefinitionId") == event.id.uuidString)
                .order(Column("roundNumber").asc)
                .fetchAll(db)

            return try roundRecords.map { round in
                let heatRecords = try Heat
                    .filter(Column("roundId") == round.id.uuidString)
                    .order(Column("heatNumber").asc)
                    .fetchAll(db)

                let localHeats = try heatRecords.map { heat in
                    let entries = try Entry
                        .filter(Column("heatId") == heat.id.uuidString)
                        .fetchAll(db)

                    let heatEntries = try entries.map { entry -> HeatEntry in
                        let club = try Club.fetchOne(db, key: entry.clubId)
                        let members = try entry.memberIds.compactMap { memberId in
                            try Member.fetchOne(db, key: memberId)
                        }
                        let names = members.map(\.fullName).joined(separator: ", ")

                        return HeatEntry(
                            id: entry.id,
                            heatId: heat.id,
                            lane: entry.lane,
                            clubName: club?.name ?? "Unknown",
                            memberNames: names.isEmpty ? "Unknown" : names,
                            isWithdrawn: entry.isWithdrawn,
                            entryCount: entries.count
                        )
                    }

                    return LocalHeat(id: heat.id, heatNumber: heat.heatNumber, entries: heatEntries)
                }

                return LocalRound(id: round.id, type: round.type, roundNumber: round.roundNumber, heats: localHeats)
            }
        }) ?? []
    }

    // MARK: - Header

    private var header: some View {
        VStack(spacing: 8) {
            HStack {
                VStack(alignment: .leading) {
                    Text(event.name)
                        .font(.title2.bold())
                    if let round = currentRound {
                        Text("\(round.type) \(round.roundNumber)")
                            .font(.subheadline)
                            .foregroundStyle(.secondary)
                    }
                }
                Spacer()
            }
            .padding(.horizontal, 16)
            .padding(.top, 12)

            if heats.count > 1 {
                Picker("Heat", selection: $selectedHeatIndex) {
                    ForEach(heats.indices, id: \.self) { i in
                        Text("Heat \(heats[i].heatNumber)").tag(i)
                    }
                }
                .pickerStyle(.segmented)
                .padding(.horizontal, 16)
            } else if let heat = currentHeat {
                Text("Heat \(heat.heatNumber)")
                    .font(.headline)
                    .foregroundStyle(.secondary)
            }
        }
        .padding(.bottom, 8)
    }

    // MARK: - Entry Row

    private func entryRow(entry: HeatEntry) -> some View {
        let entryResult = placings[entry.id]
        let isAssigned = entryResult?.placing != nil || entryResult?.isDQ == true

        return HStack(spacing: 12) {
            Text("\(entry.lane ?? 0)")
                .font(.system(size: 28, weight: .bold, design: .rounded))
                .foregroundStyle(.secondary)
                .frame(width: 44)

            VStack(alignment: .leading, spacing: 2) {
                Text(entry.memberNames)
                    .font(.body.weight(.medium))
                Text(entry.clubName)
                    .font(.caption)
                    .foregroundStyle(.secondary)
            }

            Spacer()

            if entryResult?.isDQ == true {
                Button(action: { clearResult(entryId: entry.id) }) {
                    Text("DQ")
                        .font(.headline)
                        .foregroundStyle(.white)
                        .frame(width: 60, height: 44)
                        .background(.red, in: RoundedRectangle(cornerRadius: 10))
                }
            } else {
                HStack(spacing: 6) {
                    ForEach(1...max(entry.entryCount, 4), id: \.self) { placing in
                        placingButton(placing: placing, entryId: entry.id, currentPlacing: entryResult?.placing)
                    }

                    Button(action: { assignDQ(entryId: entry.id) }) {
                        Text("DQ")
                            .font(.caption.bold())
                            .foregroundStyle(.red)
                            .frame(width: 40, height: 44)
                            .background(.red.opacity(0.1), in: RoundedRectangle(cornerRadius: 10))
                    }
                }
            }
        }
        .padding(.horizontal, 16)
        .padding(.vertical, 10)
        .background(isAssigned ? Color.green.opacity(0.05) : .clear)
    }

    private func placingButton(placing: Int, entryId: UUID, currentPlacing: Int?) -> some View {
        let isSelected = currentPlacing == placing
        let color: Color = switch placing {
            case 1: .yellow
            case 2: Color(.systemGray3)
            case 3: .orange
            default: Color(.systemGray5)
        }

        return Button(action: { assignPlacing(placing, entryId: entryId) }) {
            Text("\(placing)")
                .font(.system(size: 18, weight: .bold, design: .rounded))
                .frame(width: 44, height: 44)
                .background(isSelected ? color : color.opacity(0.3), in: RoundedRectangle(cornerRadius: 10))
                .overlay(
                    RoundedRectangle(cornerRadius: 10)
                        .strokeBorder(isSelected ? .primary : .clear, lineWidth: 2)
                )
        }
        .buttonStyle(.plain)
        .sensoryFeedback(isSelected ? .success : .selection, trigger: isSelected)
    }

    // MARK: - Bottom Bar

    private func bottomBar(heat: LocalHeat) -> some View {
        HStack {
            let activeEntries = heat.entries.filter { !$0.isWithdrawn }
            let assignedCount = activeEntries.filter { placings[$0.id] != nil }.count
            Text("\(assignedCount) / \(activeEntries.count) entered")
                .font(.subheadline)
                .foregroundStyle(.secondary)

            Spacer()

            if let message = savedMessage {
                Text(message)
                    .font(.subheadline.bold())
                    .foregroundStyle(.green)
                    .transition(.opacity)
            }

            Button(action: { Task { await saveResults(heat: heat) } }) {
                HStack {
                    if isSaving {
                        ProgressView().controlSize(.small)
                    }
                    Text("Save Heat")
                        .font(.headline)
                }
                .padding(.horizontal, 24)
                .padding(.vertical, 12)
            }
            .buttonStyle(.borderedProminent)
            .disabled(assignedCount == 0 || isSaving)
        }
        .padding(16)
        .background(.ultraThinMaterial)
    }

    // MARK: - Actions

    private func assignPlacing(_ placing: Int, entryId: UUID) {
        if let existing = placings.first(where: { $0.value.placing == placing && $0.key != entryId }) {
            placings[existing.key]?.placing = placings[entryId]?.placing
        }
        placings[entryId] = EntryResult(placing: placing, isDQ: false)
    }

    private func assignDQ(entryId: UUID) {
        placings[entryId] = EntryResult(placing: nil, isDQ: true)
    }

    private func clearResult(entryId: UUID) {
        placings.removeValue(forKey: entryId)
    }

    private func saveResults(heat: LocalHeat) async {
        guard let carnival = appState.selectedCarnival,
              let round = currentRound else { return }

        isSaving = true
        defer { isSaving = false }

        for (entryId, entryResult) in placings {
            let resultId = UUID()
            let status = entryResult.isDQ ? "Disqualified" : "Provisional"

            // Write to local GRDB
            let localResult = ShoreKit.Result(
                id: resultId,
                heatId: heat.id,
                entryId: entryId,
                placing: entryResult.placing,
                status: status
            )
            try? appState.db.write { db in
                try localResult.insert(db)
            }

            // Queue for sync
            let request = RecordResultRequest(
                eventId: event.id,
                roundId: round.id,
                heatId: heat.id,
                entryId: entryId,
                placing: entryResult.placing,
                status: status
            )
            try? await appState.sync.enqueueResult(carnivalId: carnival.id, request: request)
        }

        // Haptic
        let generator = UINotificationFeedbackGenerator()
        generator.notificationOccurred(.success)

        savedMessage = "Saved \(placings.count) results"
        try? await Task.sleep(for: .seconds(2))
        savedMessage = nil
    }
}

// MARK: - Local result state

private struct EntryResult {
    var placing: Int?
    var isDQ: Bool
}
