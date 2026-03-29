import SwiftUI
import ShoreKit

struct ResultsContentView: View {
    @Environment(ResultsAppState.self) private var appState
    @State private var carnival: CarnivalResponse?
    @State private var selectedTab = 0

    var body: some View {
        NavigationStack {
            if let carnival {
                TabView(selection: $selectedTab) {
                    ResultsListView(carnivalId: carnival.id)
                        .tabItem { Label("Results", systemImage: "list.number") }
                        .tag(0)

                    DrawListView(carnivalId: carnival.id)
                        .tabItem { Label("Draw", systemImage: "rectangle.split.3x1") }
                        .tag(1)

                    LeaderboardView(carnivalId: carnival.id)
                        .tabItem { Label("Standings", systemImage: "trophy") }
                        .tag(2)
                }
                .navigationTitle(carnival.name)
            } else {
                ProgressView("Loading...")
                    .task { await loadCarnival() }
            }
        }
    }

    private func loadCarnival() async {
        carnival = try? await appState.api.getCarnival(
            UUID(uuidString: "c1000000-0000-0000-0000-000000000001")!
        )
    }
}

// MARK: - Results

struct ResultsListView: View {
    let carnivalId: UUID
    @Environment(ResultsAppState.self) private var appState
    @State private var results: CarnivalResultsResponse?

    var body: some View {
        List {
            if let results {
                ForEach(results.events, id: \.eventId) { event in
                    Section(event.eventName) {
                        ForEach(event.heats, id: \.heatId) { heat in
                            ForEach(heat.results, id: \.id) { result in
                                HStack {
                                    Text(result.placing.map { "\($0)" } ?? "-")
                                        .font(.title3.bold())
                                        .frame(width: 30)
                                    VStack(alignment: .leading) {
                                        Text(result.members.map { "\($0.firstName) \($0.lastName)" }.joined(separator: ", "))
                                        Text(result.clubName)
                                            .font(.caption)
                                            .foregroundStyle(.secondary)
                                    }
                                    Spacer()
                                    if result.status == "Provisional" {
                                        Text("Provisional")
                                            .font(.caption2)
                                            .foregroundStyle(.orange)
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        .overlay {
            if results?.events.isEmpty ?? true {
                ContentUnavailableView("No Results Yet", systemImage: "clock", description: Text("Results will appear as they are entered."))
            }
        }
        .task { results = try? await appState.api.getResults(carnivalId) }
        .refreshable { results = try? await appState.api.getResults(carnivalId) }
    }
}

// MARK: - Draw

struct DrawListView: View {
    let carnivalId: UUID
    @Environment(ResultsAppState.self) private var appState
    @State private var draw: DrawResponse?

    var body: some View {
        List {
            if let draw {
                ForEach(draw.events, id: \.eventId) { event in
                    Section(event.eventName) {
                        ForEach(event.rounds, id: \.roundId) { round in
                            ForEach(round.heats, id: \.heatId) { heat in
                                DisclosureGroup("Heat \(heat.heatNumber)") {
                                    ForEach(heat.entries, id: \.entryId) { entry in
                                        HStack {
                                            Text("Lane \(entry.lane ?? 0)")
                                                .font(.caption.bold())
                                                .frame(width: 50)
                                            Text(entry.members.map { "\($0.firstName) \($0.lastName)" }.joined(separator: ", "))
                                            Spacer()
                                            Text(entry.clubName)
                                                .font(.caption)
                                                .foregroundStyle(.secondary)
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        .overlay {
            if draw?.events.allSatisfy({ $0.rounds.isEmpty }) ?? true {
                ContentUnavailableView("No Draws Yet", systemImage: "rectangle.split.3x1")
            }
        }
        .task { draw = try? await appState.api.getDraw(carnivalId) }
        .refreshable { draw = try? await appState.api.getDraw(carnivalId) }
    }
}

// MARK: - Leaderboard

struct LeaderboardView: View {
    let carnivalId: UUID
    @Environment(ResultsAppState.self) private var appState
    @State private var leaderboard: LeaderboardResponse?

    var body: some View {
        List {
            if let leaderboard {
                ForEach(leaderboard.standings, id: \.clubId) { standing in
                    HStack {
                        Text("\(standing.rank)")
                            .font(.title2.bold())
                            .foregroundStyle(standing.rank <= 3 ? .primary : .secondary)
                            .frame(width: 35)
                        VStack(alignment: .leading) {
                            Text(standing.clubName).font(.headline)
                            Text(standing.clubAbbreviation)
                                .font(.caption)
                                .foregroundStyle(.secondary)
                        }
                        Spacer()
                        Text("\(Int(standing.totalPoints))")
                            .font(.title.bold())
                    }
                }
            }
        }
        .overlay {
            if leaderboard?.standings.isEmpty ?? true {
                ContentUnavailableView("No Standings Yet", systemImage: "trophy")
            }
        }
        .task { leaderboard = try? await appState.api.getLeaderboard(carnivalId) }
        .refreshable { leaderboard = try? await appState.api.getLeaderboard(carnivalId) }
    }
}
