# Colegio ERP

## Dev Commands

```bash
# restore deps
dotnet restore

# migrations (run from src/, NOT root)
dotnet ef migrations add <Name> --project src/Colegio.Infrastructure --startup-project src/Colegio.Api
dotnet ef database update --project src/Colegio.Infrastructure --startup-project src/Colegio.Api

# run API locally (http://localhost:8080)
dotnet run --project src/Colegio.Api

# run with Docker
docker-compose -f docker/docker-compose.yml up --build

# tests
dotnet test
```

## Stack

- .NET 8 Minimal APIs + EF Core 8 + SQLite
- Solution: `Colegio.slnx` (not .sln)

## Conventions

| Aspect | Rule |
|--------|------|
| IDs | `Guid` (UUID), never `int` auto-increment |
| DbContext | NoTracking by default |
| Relations | Fluent API |
| Query Splitting | Global SplitQuery |
| API Style | Minimal APIs (no Controllers) |

## Project Structure

```
src/
├── Colegio.Domain/           # Entities, enums, repo interfaces
├── Colegio.Infrastructure/  # EF Core DbContext, migrations, configs
└── Colegio.Api/          # Minimal APIs, endpoints in Endpoints/
tests/
└── Colegio.Api.Tests/    # xUnit tests
```

## Testing

- Tests use xUnit
- Seed data auto-created on startup via `SeedData.SeedAsync()`
- SQLite DB: `colegio.db` (or in container)