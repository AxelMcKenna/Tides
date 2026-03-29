import SwiftUI
import ShoreKit

@main
struct ShoreResultsApp: App {
    @State private var appState = ResultsAppState()

    var body: some Scene {
        WindowGroup {
            ResultsContentView()
                .environment(appState)
        }
    }
}

@Observable
final class ResultsAppState {
    let api: APIClient

    init() {
        api = APIClient(baseURL: URL(string: "http://localhost:5266")!)
    }
}
