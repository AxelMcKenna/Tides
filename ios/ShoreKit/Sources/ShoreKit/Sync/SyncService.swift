import Foundation
import GRDB
import Network

public actor SyncService {
    private let db: ShoreDatabase
    private let api: APIClient
    private let monitor: NWPathMonitor
    private var isConnected = false

    public init(db: ShoreDatabase, api: APIClient) {
        self.db = db
        self.api = api
        self.monitor = NWPathMonitor()
    }

    public func startMonitoring() {
        monitor.pathUpdateHandler = { [weak self] path in
            guard let self else { return }
            Task {
                await self.handleConnectivityChange(path.status == .satisfied)
            }
        }
        monitor.start(queue: DispatchQueue(label: "nz.shore.sync.monitor"))
    }

    public func stopMonitoring() {
        monitor.cancel()
    }

    private func handleConnectivityChange(_ connected: Bool) async {
        let wasDisconnected = !isConnected
        isConnected = connected
        if connected && wasDisconnected {
            await flush()
        }
    }

    /// Process all queued sync items
    public func flush() async {
        guard isConnected else { return }

        do {
            let items = try db.read { db in
                try SyncQueueItem
                    .order(Column("createdAt").asc)
                    .fetchAll(db)
            }

            for var item in items {
                do {
                    try await processItem(item)
                    _ = try db.write { db in
                        try item.delete(db)
                    }
                } catch {
                    item.attempts += 1
                    item.lastError = error.localizedDescription
                    _ = try? db.write { db in
                        try item.update(db)
                    }
                    // Stop on first failure — maintain ordering
                    break
                }
            }
        } catch {
            // DB read failed — will retry on next connectivity change
        }
    }

    private func processItem(_ item: SyncQueueItem) async throws {
        guard let data = item.payload.data(using: .utf8) else { return }
        let decoder = JSONDecoder()

        switch item.type {
        case "RecordResult":
            let payload = try decoder.decode(RecordResultPayload.self, from: data)
            _ = try await api.recordResult(payload.carnivalId, request: payload.request)

        case "CorrectResult":
            let payload = try decoder.decode(CorrectResultPayload.self, from: data)
            _ = try await api.correctResult(payload.resultId, request: payload.request)

        default:
            break
        }
    }

    /// Queue a result to be synced
    public func enqueueResult(carnivalId: UUID, request: RecordResultRequest) throws {
        let payload = RecordResultPayload(carnivalId: carnivalId, request: request)
        let json = try JSONEncoder().encode(payload)
        let item = SyncQueueItem(type: "RecordResult", payload: String(data: json, encoding: .utf8)!)
        try db.write { db in
            try item.insert(db)
        }

        if isConnected {
            Task { await flush() }
        }
    }

    /// Queue a correction to be synced
    public func enqueueCorrection(resultId: UUID, request: CorrectResultRequest) throws {
        let payload = CorrectResultPayload(resultId: resultId, request: request)
        let json = try JSONEncoder().encode(payload)
        let item = SyncQueueItem(type: "CorrectResult", payload: String(data: json, encoding: .utf8)!)
        try db.write { db in
            try item.insert(db)
        }

        if isConnected {
            Task { await flush() }
        }
    }

    public var pendingCount: Int {
        (try? db.read { db in try SyncQueueItem.fetchCount(db) }) ?? 0
    }
}

// MARK: - Sync payloads

private struct RecordResultPayload: Codable {
    let carnivalId: UUID
    let request: RecordResultRequest
}

private struct CorrectResultPayload: Codable {
    let resultId: UUID
    let request: CorrectResultRequest
}
