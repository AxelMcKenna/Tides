import SwiftUI
import ShoreKit

@main
struct ShoreRecorderApp: App {
    @State private var appState = AppState()

    var body: some Scene {
        WindowGroup {
            ContentView()
                .environment(appState)
        }
    }
}

@Observable
final class AppState {
    let db: ShoreDatabase
    let api: APIClient
    let sync: SyncService
    let carnivalSync: CarnivalSyncService

    var selectedCarnival: Carnival?
    var syncState: CarnivalSyncService.SyncState = .idle

    init() {
        let dbPath = FileManager.default
            .urls(for: .documentDirectory, in: .userDomainMask)[0]
            .appendingPathComponent("shore.sqlite").path
        db = try! ShoreDatabase(path: dbPath)
        api = APIClient(baseURL: URL(string: "http://localhost:5266")!)
        sync = SyncService(db: db, api: api)
        carnivalSync = CarnivalSyncService(db: db, api: api)

        Task { await sync.startMonitoring() }
    }

    func syncCarnival(_ carnivalId: UUID) async {
        syncState = .syncing(step: "Starting...")
        let result = await carnivalSync.sync(carnivalId: carnivalId)
        syncState = result
    }
}
