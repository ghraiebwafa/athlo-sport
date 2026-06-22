# ATHLO Backend

.NET 9 fitness API with a layered microservices architecture, structured logging, and consistent `api.error` responses.

## Structure

```
backend/
├── Athlo.Shared/           # Errors, middleware, JWT, logging, CORS, roles
├── DataAccess/
│   ├── Athlo.Database/     # DbContexts, migrations, seed
│   ├── Athlo.Models/
│   ├── Athlo.Mapper/
│   └── Athlo.Repositories/ # Domain repos + UnitOfWork
├── Services/
│   ├── Athlo.AuthService/        → :5001
│   └── Athlo.ManagementService/  → :5000
└── scripts/setup-env.sh
```

## Super admin

The super admin account is **not** created via public registration. Credentials live only in `.env`:

```env
SuperAdmin__Email=superadmin@athlo.internal
SuperAdmin__Password=your_strong_password_min_12_chars
SuperAdmin__FullName=Super Admin
```

On startup, AuthService migrates the database and ensures this account exists (password synced from `.env`).

Super admin can manage other admins:

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/admin/admins` | List admins and super admin |
| `POST` | `/api/admin/admins` | Create a new admin account |
| `DELETE` | `/api/admin/admins/{id}` | Demote an admin to regular user |

Admin management endpoints require the `SuperAdmin` role.

## User management (Auth API)

Admins and super admins can browse registered users:

| Method | Endpoint | Auth |
|--------|----------|------|
| `GET` | `/api/admin/users` | Admin or SuperAdmin |
| `GET` | `/api/admin/users/{id}` | Admin or SuperAdmin |

Supports `?page=1&pageSize=20` pagination.

## Content management (Management API)

Admins and super admins can manage the catalog:

**Programs**

| Method | Endpoint | Auth |
|--------|----------|------|
| `POST` | `/api/admin/programs` | Admin or SuperAdmin |
| `PUT` | `/api/admin/programs/{id}` | Admin or SuperAdmin |
| `DELETE` | `/api/admin/programs/{id}` | Admin or SuperAdmin |

**Exercises**

| Method | Endpoint | Auth |
|--------|----------|------|
| `GET` | `/api/exercises` | Anonymous |
| `GET` | `/api/exercises/{id}` | Anonymous |
| `POST` | `/api/admin/exercises` | Admin or SuperAdmin |
| `PUT` | `/api/admin/exercises/{id}` | Admin or SuperAdmin |
| `DELETE` | `/api/admin/exercises/{id}` | Admin or SuperAdmin |

**Categories**

| Method | Endpoint | Auth |
|--------|----------|------|
| `POST` | `/api/admin/categories` | Admin or SuperAdmin |
| `PUT` | `/api/admin/categories/{id}` | Admin or SuperAdmin |
| `DELETE` | `/api/admin/categories/{id}` | Admin or SuperAdmin |

Program catalog browsing is public (no login required):

| Method | Endpoint | Auth |
|--------|----------|------|
| `GET` | `/api/programs` | Anonymous |
| `GET` | `/api/programs/categories` | Anonymous |
| `GET` | `/api/programs/{id}` | Anonymous |

## Password change

`POST /api/auth/change-password` — authenticated users can change their password. All refresh tokens are revoked on success. Super admin must change password via `.env`.

## Error format

All errors return a consistent shape:

```json
{
  "api": {
    "error": {
      "code": "VALIDATION_FAILED",
      "message": "One or more validation errors occurred.",
      "details": [{ "field": "email", "message": "Invalid email address." }]
    }
  }
}
```

## Quick Start

From the **repository root**:

```bash
./scripts/setup-env.sh
# Creates .env at the repo root with auto-generated passwords (see SuperAdmin__* in .env)

docker compose up -d postgres

# Terminal 1
cd Backend/Services/Athlo.AuthService && dotnet run

# Terminal 2
cd Backend/Services/Athlo.ManagementService && dotnet run
```

- Auth Swagger: http://localhost:5001/swagger
- Management Swagger: http://localhost:5000/swagger
- Health: `/health` on each service (includes database check)

## Docker (full stack)

From the **repository root**:

```bash
docker compose up -d
```

Connection strings are overridden to use `Host=postgres` inside containers. Management service waits for Auth service (migrations run there on startup).

## Tests

```bash
dotnet test
```

Unit tests live in `Tests/Athlo.Tests`. Integration tests (WebApplicationFactory + in-memory DB) live in `Tests/Athlo.IntegrationTests`.

## Migrations

```bash
dotnet ef migrations add <Name> \
  --project DataAccess/Athlo.Database \
  --startup-project Services/Athlo.AuthService

dotnet ef database update \
  --project DataAccess/Athlo.Database \
  --startup-project Services/Athlo.AuthService
```
