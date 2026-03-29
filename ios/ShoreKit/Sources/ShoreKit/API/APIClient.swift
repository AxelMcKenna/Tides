import Foundation

public actor APIClient {
    private let baseURL: URL
    private let session: URLSession
    private let decoder: JSONDecoder
    private let encoder: JSONEncoder

    public init(baseURL: URL, session: URLSession = .shared) {
        self.baseURL = baseURL
        self.session = session
        self.decoder = JSONDecoder()
        self.encoder = JSONEncoder()
    }

    // MARK: - Carnivals

    public func getCarnival(_ id: UUID) async throws -> CarnivalResponse {
        try await get("/api/carnivals/\(id)")
    }

    public func getDraw(_ carnivalId: UUID) async throws -> DrawResponse {
        try await get("/api/carnivals/\(carnivalId)/draw")
    }

    public func getResults(_ carnivalId: UUID, eventId: UUID? = nil) async throws -> CarnivalResultsResponse {
        var path = "/api/carnivals/\(carnivalId)/results"
        if let eventId { path += "?eventId=\(eventId)" }
        return try await get(path)
    }

    public func getLeaderboard(_ carnivalId: UUID, ageGroup: String? = nil) async throws -> LeaderboardResponse {
        var path = "/api/carnivals/\(carnivalId)/leaderboard"
        if let ageGroup { path += "?ageGroup=\(ageGroup)" }
        return try await get(path)
    }

    // MARK: - Results

    public func recordResult(_ carnivalId: UUID, request: RecordResultRequest) async throws -> ResultResponse {
        try await post("/api/carnivals/\(carnivalId)/results", body: request)
    }

    public func correctResult(_ resultId: UUID, request: CorrectResultRequest) async throws -> ResultResponse {
        try await patch("/api/results/\(resultId)", body: request)
    }

    // MARK: - Entries

    public func createEntry(_ carnivalId: UUID, request: CreateEntryRequest) async throws {
        let _: EntryResponse = try await post("/api/carnivals/\(carnivalId)/entries", body: request)
    }

    public func withdrawEntry(_ carnivalId: UUID, entryId: UUID) async throws {
        try await delete("/api/carnivals/\(carnivalId)/entries/\(entryId)")
    }

    // MARK: - Draw

    public func generateDraw(_ carnivalId: UUID, request: GenerateDrawRequest) async throws -> DrawResponse {
        try await post("/api/carnivals/\(carnivalId)/draws/generate", body: request)
    }

    // MARK: - Members

    public func getMembers(clubId: UUID) async throws -> [MemberResponse] {
        try await get("/api/clubs/\(clubId)/members")
    }

    // MARK: - Private

    private func get<T: Decodable>(_ path: String) async throws -> T {
        let url = baseURL.appendingPathComponent(path)
        let (data, response) = try await session.data(from: url)
        try checkResponse(response)
        return try decoder.decode(T.self, from: data)
    }

    private func post<T: Decodable, B: Encodable>(_ path: String, body: B) async throws -> T {
        var request = URLRequest(url: baseURL.appendingPathComponent(path))
        request.httpMethod = "POST"
        request.setValue("application/json", forHTTPHeaderField: "Content-Type")
        request.httpBody = try encoder.encode(body)
        let (data, response) = try await session.data(for: request)
        try checkResponse(response)
        return try decoder.decode(T.self, from: data)
    }

    private func patch<T: Decodable, B: Encodable>(_ path: String, body: B) async throws -> T {
        var request = URLRequest(url: baseURL.appendingPathComponent(path))
        request.httpMethod = "PATCH"
        request.setValue("application/json", forHTTPHeaderField: "Content-Type")
        request.httpBody = try encoder.encode(body)
        let (data, response) = try await session.data(for: request)
        try checkResponse(response)
        return try decoder.decode(T.self, from: data)
    }

    private func delete(_ path: String) async throws {
        var request = URLRequest(url: baseURL.appendingPathComponent(path))
        request.httpMethod = "DELETE"
        let (_, response) = try await session.data(for: request)
        try checkResponse(response)
    }

    private func checkResponse(_ response: URLResponse) throws {
        guard let http = response as? HTTPURLResponse else {
            throw APIError.invalidResponse
        }
        guard (200...299).contains(http.statusCode) else {
            throw APIError.httpError(http.statusCode)
        }
    }
}

// Entry response (only used internally for decoding)
private struct EntryResponse: Codable {
    let id: UUID
}

public struct MemberResponse: Codable, Sendable {
    public let id: UUID
    public let firstName: String
    public let lastName: String
    public let gender: String
    public let ageGroup: String
    public let surfguardId: String?
}

public enum APIError: Error, LocalizedError {
    case invalidResponse
    case httpError(Int)

    public var errorDescription: String? {
        switch self {
        case .invalidResponse: "Invalid server response"
        case .httpError(let code): "HTTP \(code)"
        }
    }
}
