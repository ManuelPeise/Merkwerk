# Architecture decision records

Format and process: see the `write-adr` skill. Status values: Proposed, Accepted, Superseded by NNN.

| No. | Decision | Status |
| --- | --- | --- |
| [001](001-client-server-offline-later.md) | Client/server architecture; offline mode in a later stage | Accepted |
| [002](002-dotnet-10-single-process.md) | Backend on .NET 10 / ASP.NET Core as a single process | Accepted |
| [003](003-blazor-for-all-expo-later.md) | Blazor for all users: adults in Server mode, children in WebAssembly mode as PWA; Expo app later | Accepted |
| [004](004-mysql-ef-core-oracle-provider.md) | MySQL 8.4 LTS with EF Core 10 and the Oracle provider (no Pomelo) | Accepted |
| [005](005-exercise-content-as-json.md) | Exercise content as JSON columns with polymorphic C# types | Proposed |
| [006](006-children-sign-in-via-paired-devices.md) | Children sign in on paired devices without passwords | Proposed |
| [007](007-tenant-isolation.md) | Tenant isolation via OrganizationId, global query filter and service checks | Proposed |
| [008](008-long-ids-with-client-ids.md) | IDs as long; Guid ClientId only for attempts and answers | Accepted |
| [009](009-docker-compose-caddy-multiarch.md) | Operation via Docker Compose with Caddy and multi-arch images | Accepted |
| [010](010-solution-layout.md) | Solution layout with numbered solution folders | Accepted |
| [011](011-entity-base-class-and-audit.md) | All entities derive from AEntityBase; audit fields set by an interceptor | Accepted |
| [012](012-repository-unit-of-work.md) | Repository and unit of work in a separate accessor project | Accepted |
| [013](013-jwt-for-all-clients.md) | JWT for all clients; HttpOnly cookie in browsers; refresh tokens in the database | Accepted |
| [014](014-lightweight-mvvm.md) | Lightweight MVVM with CommunityToolkit.Mvvm | Proposed |
