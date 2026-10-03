# Merkwerk

[![CI](https://github.com/ManuelPeise/Merkwerk/actions/workflows/ci.yml/badge.svg?branch=Development)](https://github.com/ManuelPeise/Merkwerk/actions/workflows/ci.yml)

**An open-source learning platform for children, parents, and teachers.**

Merkwerk is designed for self-hosting at home or in a school. Adults manage learning activities; children practise on a
tablet or phone. The project prioritises privacy, accessibility, and a child-friendly interface.

> **Project status:** early development. The repository contains the React UI, an ASP.NET Core API host, shared
> infrastructure, authentication groundwork, and deployment scaffolding. Most planned exercise, assignment, and
> progress features are not yet implemented; this is not ready for production use.

## Technology

| Area | Technology |
| --- | --- |
| Web UI | React 19, TypeScript 6, Vite 8, MUI 9, React Router, i18next |
| API host | ASP.NET Core controllers on .NET 10, `/api/v1`, OpenAPI/Swagger in development |
| Business logic | `Logic.*` projects; `Logic.Shared` is intended for pure shared graders and generators |
| Data | EF Core 10, MySQL 8.4, Oracle's `MySql.EntityFrameworkCore` provider |
| Authentication | ASP.NET Core authentication services, JWT in HttpOnly browser cookies, refresh-token work in progress |
| Deployment | Docker Compose, Caddy, MySQL; multi-architecture images for amd64 and arm64 |

The current UI is bilingual (German and English). This does not imply that every planned backend or exercise feature is
available in either language.

## Repository layout

```text
Merkwerk/
├─ sources/
│  ├─ Merkwerk.slnx
│  ├─ Web.Client/                 React/TypeScript UI
│  ├─ Web.Core/                   ASP.NET Core API host
│  ├─ Service/                    API/authentication registration
│  ├─ Logic.Authentication/       authentication and session services
│  ├─ Logic.Notifications/        email abstractions, templates, and SMTP delivery
│  ├─ Logic.Shared/               shared logic library (planned exercise logic)
│  ├─ Data.Database/              EF Core model and migrations
│  ├─ Data.Accessor/              repositories and unit of work
│  └─ *Tests/ and Architecture.Tests
├─ shared/
│  ├─ grading-cases/              shared grader fixtures
│  └─ design-tokens/              design reference tokens
├─ deploy/                        Docker Compose, Dockerfile, Caddy, local setup
├─ docs/                          architecture decisions and contributor guides
├─ AGENTS.md                      repository conventions
└─ CLAUDE.md
```

See the [infrastructure guide](docs/infrastructure.README.md) for project responsibilities and runtime setup, and the
[ADR index](docs/adr/README.md) for architecture decisions. Layer and dependency rules are defined in
[AGENTS.md](AGENTS.md#3-solution-layout-and-dependency-rules) and checked by architecture tests.

## Development setup

Prerequisites: [.NET 10 SDK](https://dotnet.microsoft.com/download), Node.js LTS with npm, and Docker Desktop (for the
local MySQL and integration tests).

```powershell
git clone <repo-url> Merkwerk
cd Merkwerk

# Creates deploy\.env if missing; starts MySQL and Mailpit; saves local user secrets.
.\deploy\setup-local.ps1

# Build the backend and run the API.
cd sources
dotnet build Merkwerk.slnx
dotnet run --project Web.Core --launch-profile http
```

The API is available at `http://localhost:5138`; Swagger is at `/swagger`. In a second terminal:

```powershell
cd sources\Web.Client
npm ci
npm run dev
```

The UI is at `http://localhost:65350`. Vite proxies `/api` to the local API. Development uses plain HTTP. The app does
not automatically apply database migrations on startup; for schema updates use the migration commands in the
[infrastructure guide](docs/infrastructure.README.md).

To build and test the .NET solution:

```powershell
cd sources
dotnet build Merkwerk.slnx -c Release
dotnet test Merkwerk.slnx -c Release
```

Some integration tests require Docker. For frontend checks and details, see
[Web.Client/README.md](sources/Web.Client/README.md).

## Planned product capabilities

Planned work includes exercise authoring, generated maths and vocabulary practice, spaced repetition, learning aids,
assignments, and adult progress views. Tickets and acceptance criteria are maintained outside the repository. A listed
capability is not necessarily implemented; consult the source code and current project status before relying on it.

## Privacy and security

The product is intended to keep children's data within the self-hosted installation. No tracking or advertising SDKs
are used. Do not send children's data to third-party services, put secrets in source control, or log personal data.
Installations in schools must be reviewed by the responsible data controller for their legal and operational
requirements.

## Contributing

Read [AGENTS.md](AGENTS.md) before changing code. Work on one ticket branch created from `Development`, keep changes
focused, and validate with the relevant builds and tests. The detailed branch and pull-request process is in
[docs/git.README.md](docs/git.README.md).

## License

The application source is licensed under the [MIT License](LICENSE).
