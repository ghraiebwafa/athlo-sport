# ATHLO Sport

Fitness platform — .NET 9 microservices API + Expo React Native mobile app.

## Stack

| Layer | Tech |
|-------|------|
| Auth API | ASP.NET Core 9 — port **5001** |
| Management API | ASP.NET Core 9 — port **5000** |
| Database | PostgreSQL 16 |
| Mobile | React Native (Expo Router) |

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20.19.4+](https://nodejs.org/) (or 22.13+)
- [Docker](https://www.docker.com/) (for Postgres + optional full backend stack)
- [Expo Go](https://expo.dev/go) or iOS/Android simulator

## Quick start

### 1. Environment

```bash
./scripts/setup-env.sh
# Creates .env with auto-generated passwords (replaces placeholders if .env already exists)
# Use ./scripts/setup-env.sh --force to regenerate passwords in an existing .env
```

### 2. Backend

**Option A — Docker (recommended)**

```bash
docker compose up -d
```

**Option B — Local APIs**

```bash
docker compose up -d postgres

# Terminal 1
cd Backend/Services/Athlo.AuthService && dotnet run

# Terminal 2
cd Backend/Services/Athlo.ManagementService && dotnet run
```

| Service | Swagger | Health |
|---------|---------|--------|
| Auth | http://localhost:5001/swagger | http://localhost:5001/health |
| Management | http://localhost:5000/swagger | http://localhost:5000/health |

### 3. Frontend

```bash
cd Frontend
cp .env.example .env   # adjust API URLs for your device (see Frontend/README.md)
npm install
npm start
# press i (iOS), a (Android), or w (web)
```

## Project structure

```
Athlo_sport/
├── Backend/          # .NET APIs, database, tests
├── Frontend/         # Expo mobile app
├── docker-compose.yml
├── .env.example
└── scripts/
```

## Environment variables

See [.env.example](.env.example) for all backend variables.

Frontend uses `EXPO_PUBLIC_AUTH_API_URL` and `EXPO_PUBLIC_MANAGEMENT_API_URL` — see [Frontend/.env.example](Frontend/.env.example).

## Tests

```bash
cd Backend && dotnet test
cd Frontend && npm run typecheck && npm test
```

## Documentation

- [Backend/README.md](Backend/README.md) — API endpoints, migrations, admin
- [Frontend/README.md](Frontend/README.md) — mobile setup, device-specific API URLs

## Super admin

Credentials are set in `.env` only (not via public registration). On startup, Auth service seeds this account. See [Backend/README.md](Backend/README.md) for admin endpoints.
