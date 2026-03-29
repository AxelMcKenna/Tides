# Shore

Live results and competition management for surf lifesaving carnivals.

## Stack

| Layer | Tech |
|-------|------|
| API | .NET 10, ASP.NET Core, EF Core, SignalR |
| Web | Next.js 16, React 19, Tailwind CSS 4, TypeScript |
| iOS | Swift, SwiftUI, GRDB (offline-first SQLite) |
| Database | PostgreSQL 16 |
| CI | GitHub Actions |

## Project Structure

```
src/
  Tides.Api/            ASP.NET Core REST API + SignalR hub
  Tides.Core/           Domain models, events, services
  Tides.Infrastructure/  EF Core, migrations, repositories
  Tides.Web/            Next.js frontend
ios/
  ShoreKit/             Swift package — models, DB, sync, API client
  ShoreRecorder/        iPad app for officials recording results on the beach
  ShoreResults/         Results viewer app
tests/
  Tides.Core.Tests/
  Tides.Api.Tests/
  Tides.Integration.Tests/
```

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/)
- [Node.js](https://nodejs.org/) (v20+)
- [Docker](https://www.docker.com/)

### Run

```bash
# Start PostgreSQL
docker compose up -d

# Run the API (http://localhost:5266)
cd src/Tides.Api
dotnet run

# Run the web frontend (http://localhost:3000)
cd src/Tides.Web
npm install
npm run dev
```

### Test

```bash
dotnet test
```

## Key Features

- **Heat draw generation** — automatic lane assignments across rounds
- **Live results** — real-time updates via SignalR to web and display screens
- **Points & leaderboards** — automated points calculation and club standings
- **Protest management** — lodge, review, and adjudicate protests
- **Offline-first iOS** — officials record results on iPads without connectivity, sync when back online
- **Display mode** — auto-rotating results view for big screens at the beach
