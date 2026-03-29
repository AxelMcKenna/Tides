import SwiftUI
import ShoreKit

struct CarnivalSidebar: View {
    @Environment(AppState.self) private var appState
    @State private var carnivals: [Carnival] = []

    var body: some View {
        List(carnivals, selection: Binding(
            get: { appState.selectedCarnival?.id },
            set: { id in
                appState.selectedCarnival = carnivals.first { $0.id == id }
            }
        )) { carnival in
            VStack(alignment: .leading, spacing: 2) {
                Text(carnival.name).font(.headline)
                Text("\(carnival.startDate) · \(carnival.sanction)")
                    .font(.caption)
                    .foregroundStyle(.secondary)
            }
            .tag(carnival.id)
            .swipeActions(edge: .trailing) {
                Button("Sync") {
                    Task { await fullSync(carnival) }
                }
                .tint(.blue)
            }
        }
        .navigationTitle("Carnivals")
        .toolbar {
            ToolbarItem(placement: .primaryAction) {
                Button(action: { Task { await fetchCarnivalList() } }) {
                    Label("Refresh", systemImage: "arrow.clockwise")
                }
            }
        }
        .overlay {
            if carnivals.isEmpty {
                ContentUnavailableView("No Carnivals", systemImage: "tray",
                    description: Text("Tap refresh to download carnivals."))
            }
        }
        .safeAreaInset(edge: .bottom) {
            syncProgressBanner
        }
        .task { loadFromDB() }
        .onChange(of: appState.selectedCarnival?.id) { _, newId in
            if let id = newId, let carnival = carnivals.first(where: { $0.id == id }) {
                Task { await fullSync(carnival) }
            }
        }
    }

    @ViewBuilder
    private var syncProgressBanner: some View {
        switch appState.syncState {
        case .idle:
            EmptyView()
        case .syncing(let step):
            HStack(spacing: 8) {
                ProgressView().controlSize(.small)
                Text(step)
                    .font(.caption)
                    .foregroundStyle(.secondary)
            }
            .frame(maxWidth: .infinity)
            .padding(10)
            .background(.ultraThinMaterial)
        case .completed(let date):
            HStack(spacing: 6) {
                Image(systemName: "checkmark.circle.fill")
                    .foregroundStyle(.green)
                Text("Synced \(date.formatted(.dateTime.hour().minute()))")
                    .font(.caption)
                    .foregroundStyle(.secondary)
            }
            .frame(maxWidth: .infinity)
            .padding(10)
            .background(.ultraThinMaterial)
        case .failed(let error):
            HStack(spacing: 6) {
                Image(systemName: "exclamationmark.triangle.fill")
                    .foregroundStyle(.orange)
                Text(error)
                    .font(.caption)
                    .foregroundStyle(.secondary)
                    .lineLimit(1)
            }
            .frame(maxWidth: .infinity)
            .padding(10)
            .background(.ultraThinMaterial)
        }
    }

    private func loadFromDB() {
        carnivals = (try? appState.db.read { db in try Carnival.fetchAll(db) }) ?? []
    }

    private func fetchCarnivalList() async {
        let id = UUID(uuidString: "c1000000-0000-0000-0000-000000000001")!
        guard let response = try? await appState.api.getCarnival(id) else { return }

        let carnival = Carnival(
            id: response.id, name: response.name,
            hostingClubId: response.hostingClubId, sanction: response.sanction,
            startDate: response.startDate, endDate: response.endDate
        )
        try? appState.db.write { db in try carnival.save(db) }
        loadFromDB()
    }

    private func fullSync(_ carnival: Carnival) async {
        await appState.syncCarnival(carnival.id)
        loadFromDB()
    }
}
