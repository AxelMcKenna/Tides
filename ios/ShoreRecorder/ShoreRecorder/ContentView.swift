import SwiftUI
import ShoreKit

struct ContentView: View {
    @Environment(AppState.self) private var appState

    var body: some View {
        VStack(spacing: 0) {
            NavigationSplitView {
                CarnivalSidebar()
            } detail: {
                if appState.selectedCarnival != nil {
                    RecorderDetailView()
                } else {
                    ContentUnavailableView("Select a Carnival", systemImage: "flag.2.crossed",
                        description: Text("Choose a carnival from the sidebar to begin recording."))
                }
            }

            SyncStatusBar(sync: appState.sync)
        }
    }
}
