# Athlo Backend — Developer Guide

This document complements the **XML API documentation** embedded in the codebase and surfaced in Swagger (Development only).

## Solution layout

| Project | Role |
|---------|------|
| `Athlo.AuthService` | Authentication, profile, preferences, admin user management (`:5001`) |
| `Athlo.ManagementService` | Programs, workouts, progress, content admin (`:5000`) |
| `Athlo.Shared` | Cross-cutting: JWT, errors, middleware, extensions |
| `Athlo.Models` | Entities and DTOs |
| `Athlo.Database` | EF Core `AthloDbContext`, migrations |
| `Athlo.Repositories` | Data access |
| `Athlo.Mapper` | Entity ↔ DTO mapping |

Both APIs share **one PostgreSQL database**. **AuthService** runs migrations and seeds (including super admin) via `DataSeeder`.

## Running locally

```bash
docker compose up -d          # PostgreSQL (+ Redis optional)
dotnet run --project Backend/Services/Athlo.AuthService
dotnet run --project Backend/Services/Athlo.ManagementService
```

Swagger UI (Development):

- Auth: `http://localhost:5001/swagger`
- Management: `http://localhost:5002/swagger` (port may vary — check `launchSettings.json`)

## Authentication

- **JWT access tokens** (short-lived) + **refresh tokens** (rotating, stored hashed in DB).
- Access tokens include `jti` and `iat` claims.
- **Logout**, **change password**, and **reset password** revoke refresh tokens and invalidate outstanding access tokens.
- Revocation uses `IAccessTokenRevocationService` (Redis when `ConnectionStrings:Redis` is set; otherwise in-memory **per process**).

> **Production:** Configure Redis so revocation and login lockout are shared across all API instances. A startup warning is logged when Redis is missing.

## API errors

All errors use a consistent envelope:

```json
{
  "api": {
    "error": {
      "code": "VALIDATION_FAILED",
      "message": "One or more validation errors occurred.",
      "traceId": "…",
      "timestamp": "…",
      "details": [{ "field": "email", "message": "…" }]
    }
  }
}
```

See `Athlo.Shared.Models.ApiError` and `ExceptionHandlingMiddleware`.

## Authorization conventions

| Policy | Roles |
|--------|-------|
| `[Authorize]` | Any authenticated user |
| `AdminOrSuperAdmin` | `Admin`, `SuperAdmin` |
| `SuperAdminOnly` | `SuperAdmin` |

**Workout/session ownership:** Wrong user → **404** (not 403) to avoid leaking resource existence.

## Key domains

### Workouts (`IWorkoutService`)

- One `InProgress` session per user.
- Sets are unique per `(session, programExercise, setNumber)`.
- Stale `InProgress` sessions older than 24h are cancelled by `StaleWorkoutCleanupService`.
- Pause/resume tracks `PausedAt` and `PausedDurationSeconds`.

### Programs & saved programs

- Public program catalog; users bookmark via `saved_programs` (composite key `userId + programId`).

### User preferences (`users.preferences_json`)

- JSON blob synced with the mobile app (notifications, HR source, rest timer defaults).
- `GET/PUT /api/auth/preferences`.

### Retention (push, achievements, weekly summary)

- `POST/DELETE /api/devices/push-token` — register Expo / device tokens.
- `GET /api/achievements` — catalog + unlock state (unlocked on workout complete).
- `GET /api/progress/weekly-summary` — Monday–Sunday snapshot for Home.
- Background `WorkoutReminderService` sends daily reminders when preferences allow.
- Set `Push:UseExpo=false` to log notifications instead of calling Expo (tests always use logging sender).

## Testing

```bash
cd Backend
dotnet test
```

- `Athlo.Tests` — unit tests (shared helpers, security).
- `Athlo.IntegrationTests` — full API tests with in-memory EF database.

## Adding endpoints

1. DTO in `Athlo.Models/DTOs/…`
2. FluentValidation validator in the service's `Validators/` folder
3. Repository method if needed
4. Service interface + implementation (with XML docs)
5. Controller action (with XML docs)
6. Integration test in `Athlo.IntegrationTests`

## Media uploads

Admins can upload exercise/program images via `POST /api/admin/media` (multipart).
Files are stored under `Media:StoragePath` (default `App_Data/uploads`) and served at `/uploads/{file}`.

Configure:

```
Media__PublicBaseUrl=http://localhost:5000
Media__StoragePath=App_Data/uploads
```

## Account lifecycle

- `GET /api/auth/account/export` — JSON export (profile, preferences, workouts, saved programs)
- `DELETE /api/auth/account` — permanent delete after password confirmation (super admin blocked)

## Crash reporting

Set `Sentry__Dsn` (or `SENTRY_DSN`) to enable Sentry on Auth and Management. Leave empty to disable.

## Redis

Docker Compose starts Redis by default. Configure `ConnectionStrings__Redis` so JWT revocation and login lockout are shared across instances. Without Redis, a startup warning is logged.

## XML documentation

- Enabled solution-wide via `Directory.Build.props` (`GenerateDocumentationFile`).
- Controllers and service interfaces are documented with `///` comments.
- Swagger includes XML from the hosting assembly and `Athlo.Shared`.

In Visual Studio / Rider, hover public APIs for the same comments.
