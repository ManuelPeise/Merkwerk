# Architecture decision records

Architecture decision records (ADRs) capture the context, decision, alternatives, and consequences of significant
architecture choices. Accepted decisions are the source of truth when repository summaries or older documentation
differ. A superseded ADR remains as historical context; follow the superseding decision instead.

Status values: **Proposed**, **Accepted**, or **Superseded by NNN**.

| No. | Decision | Status |
| --- | --- | --- |
| [001](001-client-server-offline-later.md) | Client/server architecture; offline mode in a later stage | Accepted |
| [002](002-dotnet-10-single-process.md) | Backend on .NET 10 / ASP.NET Core as a single process | Accepted |
| [003](003-blazor-for-all-expo-later.md) | Blazor for all users; Expo later | Superseded by 015 |
| [004](004-mysql-ef-core-oracle-provider.md) | MySQL 8.4 with EF Core 10 and Oracle's provider (no Pomelo) | Accepted |
| [005](005-exercise-content-as-json.md) | Exercise content as JSON columns with polymorphic C# types | Proposed |
| [006](006-children-sign-in-via-paired-devices.md) | Children sign in via paired devices without passwords | Proposed |
| [007](007-tenant-isolation.md) | Tenant isolation using `OrganizationId`, query filters, and service checks | Proposed |
| [008](008-long-ids-with-client-ids.md) | `long` entity IDs; `Guid ClientId` for attempts and answers | Accepted |
| [009](009-docker-compose-caddy-multiarch.md) | Docker Compose, Caddy, and multi-architecture deployment | Accepted (migrations: see 016) |
| [010](010-solution-layout.md) | Numbered solution folders and project layout | Accepted |
| [011](011-entity-base-class-and-audit.md) | Entity base classes and interceptor-managed audit fields | Accepted |
| [012](012-repository-unit-of-work.md) | Repository and unit-of-work data access layer | Accepted |
| [013](013-jwt-for-all-clients.md) | JWT authentication; HttpOnly browser cookies and refresh tokens | Accepted |
| [014](014-lightweight-mvvm.md) | Lightweight MVVM with CommunityToolkit.Mvvm | Superseded by 015 |
| [015](015-react-typescript-ui.md) | React/TypeScript UI, REST API, and Caddy delivery | Accepted |
| [016](016-migrations-at-startup.md) | The app applies pending migrations at startup | Accepted |

In particular, [ADR 015](015-react-typescript-ui.md) replaces ADRs 003 and 014 for the current UI architecture.
