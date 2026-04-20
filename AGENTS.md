# 📚 Colegio ERP - AI Agent Guidelines

This document contains the core application architecture, stack, and rules for the **Colegio ERP** project. All AI agents MUST adhere to these technical specifications when contributing code.

## 🛠 Tech Stack

### 🔹 Backend: .NET 8 (C# 12)
- **Framework**: ASP.NET Core Minimal APIs
- **ORM**: Entity Framework Core 8 (SQLite `colegio.db`)
- **Solution File**: `Colegio.slnx`

### 🔹 Frontend: React 19 + TypeScript
- **Build Tool**: Vite
- **Styling**: Tailwind CSS 4 + PostCSS
- **UI/UX Components**: `lucide-react`, `framer-motion`, `recharts`
- **Routing**: `react-router-dom`
- **Networking**: `axios`

---

## 🏗 Project Structure

```text
Colegio/
├── src/                         # Backend Application
│   ├── Colegio.Domain/          # Entities, enums, repository interfaces
│   ├── Colegio.Infrastructure/  # EF Core DbContext, migrations, configs
│   └── Colegio.Api/             # Minimal APIs (Endpoints folder), DI Setup
├── tests/
│   └── Colegio.Api.Tests/       # xUnit Tests (Integration / Unit)
├── web/                         # React Frontend Application
│   ├── src/                     # React components, pages, hooks, utils
│   └── public/                  # Static assets
└── docker/                      # Docker configurations
```

---

## 💻 Dev Commands

### Backend (.NET)
```bash
# Restore dependencies
dotnet restore

# Run API locally (http://localhost:8080)
dotnet run --project src/Colegio.Api

# Entity Framework Migrations (Run from root)
dotnet ef migrations add <Name> --project src/Colegio.Infrastructure --startup-project src/Colegio.Api
dotnet ef database update --project src/Colegio.Infrastructure --startup-project src/Colegio.Api

# Run tests
dotnet test
```

### Frontend (React/Vite)
```bash
# Navigate to frontend directory
cd web

# Run dev server
npm run dev

# Build for production
npm run build
```

### Docker
```bash
# Run full stack in Docker containers
docker-compose -f docker/docker-compose.yml up --build
```

---

## 📏 Technical Conventions & Rules

| Category | Rules to Enforce |
|----------|-----------------|
| **Database IDs** | ALWAYS use `Guid` (UUID). NEVER use `int` auto-increment. |
| **EF DbContext** | `AsNoTracking` by default for queries. Use Fluent API for relations. |
| **EF Queries** | ALWAYS use Global `SplitQuery` to avoid cartesian explosions. |
| **API Style** | NO CONTROLLERS. Use strict Minimal APIs mapped in `/Endpoints`. |
| **API Responses** | Rely on standard `IResult` outputs (`TypedResults.Ok`, `TypedResults.NotFound`). |
| **Security** | Manage Secrets through AppSettings/Environment Variables. |
| **Frontend UI** | Implement aesthetic, premium modern interfaces with `tailwindcss` primitives. |
| **Frontend State** | Opt for functional hooks and granular state. Minimal abstractions. |

---

## 🧪 Testing Protocol

- **Framework**: `xUnit`
- **Database Strategy**: Database integration testing using isolated/seeded instances (SQLite Db). No heavy mock reliance where actual DB tests are robust.
- **Bootstrapping**: Seed data is auto-created on application start via `SeedData.SeedAsync()`.

> **Note**: For core behavior, system heuristics, and thinking instructions for AI modules, refer to `claude.md`.