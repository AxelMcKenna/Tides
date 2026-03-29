import SwiftUI
import ShoreKit

struct SyncStatusBar: View {
    let sync: SyncService
    @State private var pendingCount = 0
    @State private var timer: Timer?

    var body: some View {
        HStack(spacing: 8) {
            // Connectivity indicator
            Circle()
                .fill(pendingCount == 0 ? .green : .orange)
                .frame(width: 8, height: 8)

            if pendingCount > 0 {
                Text("\(pendingCount) pending")
                    .font(.caption.bold())
                    .foregroundStyle(.orange)

                Button("Sync Now") {
                    Task { await sync.flush() }
                }
                .font(.caption)
                .buttonStyle(.bordered)
                .controlSize(.mini)
            } else {
                Text("Synced")
                    .font(.caption)
                    .foregroundStyle(.green)
            }

            Spacer()
        }
        .padding(.horizontal, 16)
        .padding(.vertical, 6)
        .background(.ultraThinMaterial)
        .task {
            // Poll pending count every 3 seconds
            while !Task.isCancelled {
                pendingCount = await sync.pendingCount
                try? await Task.sleep(for: .seconds(3))
            }
        }
    }
}
