# Shore

Live results and competition management for surf lifesaving carnivals.

## Stack

![.NET](https://img.shields.io/badge/.NET_10-512BD4?logo=dotnet&logoColor=white)
![Next.js](https://img.shields.io/badge/Next.js_16-000000?logo=nextdotjs&logoColor=white)
![React](https://img.shields.io/badge/React_19-61DAFB?logo=react&logoColor=black)
![Tailwind CSS](https://img.shields.io/badge/Tailwind_CSS_4-06B6D4?logo=tailwindcss&logoColor=white)
![TypeScript](https://img.shields.io/badge/TypeScript-3178C6?logo=typescript&logoColor=white)
![Swift](https://img.shields.io/badge/Swift-F05138?logo=swift&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL_16-4169E1?logo=postgresql&logoColor=white)
![SignalR](https://img.shields.io/badge/SignalR-512BD4?logo=dotnet&logoColor=white)
![GitHub Actions](https://img.shields.io/badge/GitHub_Actions-2088FF?logo=githubactions&logoColor=white)

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
