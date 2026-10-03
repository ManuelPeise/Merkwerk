# AGENTS.md – Merkwerk

Working agreement for AI assistants (Claude Code, GitHub Copilot, Codex, …) and human contributors.
This file is the **single source of truth** for conventions. `CLAUDE.md` only imports it; GitHub Copilot gets a summary in
`.github/copilot-instructions.md` and `.github/instructions/*.instructions.md` (Visual Studio does not read AGENTS.md).
The UI has its own, more detailed rules in [`sources/Web.Client/AGENTS.md`](sources/Web.Client/AGENTS.md).

## 1. What this is

Merkwerk is an open-source learning platform (MIT). Adults (parents, teachers) create exercises in German,
English and maths and assign them to children; children (initially grades 1–4, older age groups later)
practise on a tablet or phone. Deployment: self-hosted, the family instance runs on a Raspberry Pi (ARM64).

- Product concept, architecture, exercise types and tickets live **outside** this repo (planning folder).
  Tickets are named `LP-xxx`; every change belongs to a ticket.
- Architecture decisions: `docs/adr/` (ADR 001–015). If something contradicts an ADR, the ADR wins – not your assumption.
- Further documentation:
  - [`docs/infrastructure.README.md`](docs/infrastructure.README.md) – runtime setup, configuration, CI/CD.
  - [`docs/git.README.md`](docs/git.README.md) – branch model, commands, pull requests, releases, migration conflicts.

## 2. Tech stack (fixed – change only via ADR)

| Area | Choice |
| --- | --- |
| Backend runtime | .NET 10, C# (latest language version), `Nullable` and `ImplicitUsings` enabled |
| UI | **React 19 + TypeScript** (Vite, MUI, react-router-dom, axios, i18next) in `Web.Client` – adults `/admin`, children `/practice` (PWA) – ADR 015 |
| API | ASP.NET Core **controllers** in `Web.Core` under `/api/v1`, OpenAPI + Swagger UI (development), errors as `ProblemDetails` |
| Database | MySQL 8.4 LTS, EF Core 10, provider **MySql.EntityFrameworkCore** (Oracle). **No Pomelo.** |
| Auth | ASP.NET Core Identity + **JWT** (access token 15 min, rotating refresh token stored hashed in the DB). Browsers: HttpOnly cookies `mw_access` / `mw_refresh` |
| Mail | SMTP (MailKit, proposed in LP-162); Mailpit catches all mails in development |
| Tests | xUnit, NSubstitute, Testcontainers (MySQL); UI: ESLint/TypeScript now, Vitest planned |
| Operations | Docker Compose (Caddy, app, MySQL), multi-arch images (amd64 + arm64). Development runs over plain HTTP |

Ask before adding any NuGet or npm package. NuGet versions are managed centrally in `sources/Directory.Packages.props`
(`<PackageReference>` without `Version`).

## 3. Solution layout and dependency rules

```
sources/Merkwerk.slnx
  01 Web     Web.Client             React/TypeScript UI (Web.Client.esproj, npm) – talks to the backend only over HTTP
             Web.Core               Startup project, API host: Bundels/ (registration, pipeline),
                                    Services/ApiControllers/<Module>/ (controller + Dtos/), Services/Cookies/
  02 Logic   Logic.Authentication   Login, token issuing, refresh-token rotation, sessions (DI/ for registration)
             Logic.Shared           Pure logic shared by modules (graders, generators) – no I/O
  03 Data    Data.Database          Entities, MerkwerkDbContext, configurations, interceptors, migrations
             Data.Accessor          Repositories and unit of work – the only way to reach the database
  04 Shared  (empty for now)
  05 Tests   Data.IntegrationTests  DbContext, repositories and unit of work against MySQL 8.4 (Testcontainers, needs Docker)
```

| Project | May reference |
| --- | --- |
| Logic.Shared | – (Microsoft.Extensions abstractions only) |
| Data.Database | – |
| Data.Accessor | Data.Database |
| Logic.* (e.g. Logic.Authentication) | Logic.Shared, Data.Accessor |
| Web.Core | Logic.*, Data.Accessor / Data.Database (the latter two for DI registration only) |
| Web.Client | no .NET project – only the REST API |

**Hard rules** (to be enforced by architecture tests again):

- `Web.Core` is **transport only**: controllers, DTOs, cookies, authentication middleware, OpenAPI. Business logic goes into a `Logic.*` project.
- `Logic.*` projects never reference ASP.NET Core (`HttpContext`, cookies, JwtBearer stay in `Web.Core`) and reach data
  **only** through `Data.Accessor` – never `MerkwerkDbContext`, `DbSet<T>` or `DbContextOptions`. Entity types and EF Core's
  async query extensions (`ToListAsync`, `AnyAsync`, …) are fine.
- `Logic.Shared` has **no** dependency on EF Core, ASP.NET Core or I/O.
- **Entities never leave the server.** Only DTOs (`sealed record` in `Web.Core/Services/ApiControllers/<Module>/Dtos/`) go over the wire;
  the UI mirrors them as TypeScript types in `Web.Client/src/lib/api/<module>/<module>Types.ts`.
- Controllers talk to **services**, never directly to repositories or the DbContext.
- New projects (e.g. another `Logic.<Area>` or `Logic.Notifications`) only after asking.

## 4. Modules

Planned business areas: `Organizations`, `Learners`, `Exercises`, `Assignments`, `Practice` (attempts, answers, learning state),
`Progress`, `WordLists`, later `Sharing`, `Administration`. Authentication lives in `Logic.Authentication`
(`IAuthSessionService`, `TokenService`, options) so it can grow (Identity, invitations, device pairing – LP-104 to LP-106).
Per module: `I<Name>Service` + implementation, validators, module-internal types, registration in a `DI/` extension.

## 5. Data access (ADR 004, 007, 008, 011, 012)

- Every entity derives from `AEntityBase` (`long Id`, `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`);
  everything owned by a family/school derives from `AOrganizationEntityBase` (`long OrganizationId`).
- **Never set audit fields by hand** – the `AuditSaveChangesInterceptor` does that.
- Time is always **UTC** and always comes from `TimeProvider`; never call `DateTime.Now`/`UtcNow` directly.
- IDs: `long` (AUTO_INCREMENT). `Attempt` and `Answer` additionally have a unique `Guid ClientId` for idempotency/offline use.
- Access data only through an `IUnitOfWork` from `IUnitOfWorkFactory`; **one unit of work per business operation**
  (`await using var uow = _uowFactory.Create();`). Never hold a DbContext in long-lived services.
- `IRepository<T>`: `GetByIdAsync` (tracked), `Query()` (**no tracking**, for reads), `QueryTracked()` (for loading entities
  you change), `Add`, `Remove`. Read queries: `Query()` + `Select(...)` into DTOs. Don't load whole entity graphs just to build DTOs.
- Queries Logic needs often get a named method in a specialized repository (e.g. `IOrganizationRepository.AnyAsync`),
  exposed as a property on `IUnitOfWork` and returned by `Repository<T>()` as well – services stay unit-testable without
  mocking `IQueryable`. Implementations in `Data.Accessor` are `internal`; Logic sees only `Data.Accessor.Abstractions`.
- Tenant isolation: global query filter on `OrganizationId` **plus** an explicit check in the service
  (IDs are sequential and guessable). Use `IgnoreQueryFilters()` only in the `Administration` module, with a comment explaining why.
- Exercise content (`Question.Payload`, `Question.Solution`, generator parameters) is stored in JSON columns with polymorphic types
  (`System.Text.Json`, type discriminator). A new question type = a new class, **no** migration.
- Migrations: `dotnet ef migrations add <Name> -p Data.Database -s Web.Core` (run `dotnet tool restore` once – dotnet-ef is
  pinned in `.config/dotnet-tools.json`). Names in English, PascalCase. Never edit a generated migration by hand without review.
  MySQL does not run DDL transactionally → keep migrations small, never mix schema and data changes.
  Only exception: the first migration (`InitializeDatabase`) seeds the standard subjects (`HasData` in `SubjectConfiguration`, fixed values).
- Production applies migrations with the bundle image (`docker compose run --rm migrate`) before the new app version starts;
  the app itself never migrates at startup. CI fails if the model has changes without a migration.
- Character set `utf8mb4`, collation `utf8mb4_0900_ai_ci`.

## 6. Graders, generators, learning state

- Every question type has one grader in C# (`Logic.Shared`, **authoritative**) and one in TypeScript in `Web.Client`
  (instant feedback only). Both must pass the same cases in `shared/grading-cases/<type>/*.json`.
- Solutions are never sent to the client – only the grading rules the client grader needs.
- Generators are **deterministic**: same seed → same exercises. No `Random.Shared`.
- Learning state: one generic `LearningState` (Leitner boxes 1–5) keyed by item, e.g. `word:{id}:write`, `math:mul:7x8`.

## 7. UI

All rules for the React app are in [`sources/Web.Client/AGENTS.md`](sources/Web.Client/AGENTS.md). The essentials:
imports only via `src/…`; components `React.FC<IProps>` with props destructured in the body and `export default`;
MUI styled through the theme (no system props, no hex colours outside the theme); every text via
`getResource(...)` with keys in `de` **and** `en`; API calls only through `statelessApi`; children's UI with
touch targets ≥ 64×64 px, icons plus text and friendly feedback; no CDNs, no tracking.

## 8. API

- Controllers live in `Web.Core/Services/ApiControllers/<Module>/`, derive from `ApiControllerBase`
  (`[ApiController]`, route `api/v1/[controller]/[action]`, URLs lowercase kebab-case) and stay thin:
  validate → call service → return DTO.
- Actions end in `Async`, take a `CancellationToken`, document responses with `[ProducesResponseType]`.
- Errors as `ProblemDetails`; never leak exception details. Validation via data annotations on the DTO record parameters.
- Submitting an answer is an idempotent `PUT`. Every endpoint has `[Authorize]` with a matching policy
  (`Learner`, `Member`, `OrgAdmin`, `InstanceAdmin`), except explicitly anonymous ones (login, setup, invitation, device pairing).
- Auth cookies only via `AuthCookieWriter` (HttpOnly, SameSite=Strict, Secure outside development over HTTP).

## 9. Security and privacy (non-negotiable)

- No tracking, analytics, advertising or error-reporting SDKs. No external CDNs (fonts are self-hosted).
- No children's data to third parties (including speech recognition, translation or AI services).
- Store only first name/pseudonym, grade and avatar for children. No email, no date of birth, no photos.
- Never commit secrets: use user secrets or `deploy/.env` (ignored). Sample values only in `.env.example`. Never commit `*.crt`, `*.pfx`, `*.key`.
- Passwords only through ASP.NET Core Identity; store refresh tokens, invitation and pairing codes hashed only.
- Logs contain no personal data (no names, e-mail addresses, tokens, children's answers).
- Mails only to adults and never with data about children.

## 10. C# conventions

- Code, identifiers, comments, commit messages and documentation in **English**; only UI strings are German (and English).
- File-scoped namespaces; one type per file; namespace = project + folder.
- Prefixes: interfaces `I…`, abstract base classes `A…` (e.g. `AEntityBase`).
- Async methods end in `Async` and take a `CancellationToken` as the last parameter.
- `sealed` for classes not designed for inheritance; `record` for DTOs.
- Primary constructors for DI are fine. No service locator, no static state.
- Options classes with `SectionName`, bound with `ValidateOnStart`.
- No warnings in commits (`TreatWarningsAsErrors` in Release).
- PowerShell scripts (`*.ps1`): ASCII only, or save as UTF-8 **with BOM**. Windows PowerShell 5.1 reads UTF-8 without BOM
  as ANSI, and characters like `–` turn into quote marks that break the script.
- Line endings: Git defaults (CRLF on Windows).

## 11. Tests – Definition of Done

A ticket is done when:

1. all acceptance criteria of the ticket are met,
2. backend: `dotnet build` has no warnings and `dotnet test` is green,
3. UI: `npm run lint`, `npm run build` and `npm run i18n:check` are green (in `sources/Web.Client`),
4. new logic has unit tests (services with a mocked `IUnitOfWork`),
5. changes to repositories, query filters or controllers have an integration test against MySQL (Testcontainers),
6. grader changes have JSON test cases,
7. permissions are tested (may X do this? may Y **not**?).

Test names: `Method_State_ExpectedResult`. Structure tests as Arrange/Act/Assert.

## 12. Commands

```powershell
cd sources
dotnet build Merkwerk.slnx
dotnet test Merkwerk.slnx
dotnet run --project Web.Core --launch-profile http          # http://localhost:5138, Swagger at /swagger
dotnet tool restore                                            # once: dotnet-ef from .config/dotnet-tools.json
dotnet ef migrations add <Name> -p Data.Database -s Web.Core
dotnet ef database update -p Data.Database -s Web.Core         # local DB (connection string from user secrets)
docker compose -f ../deploy/docker-compose.yml up -d db
docker compose -f ../deploy/docker-compose.yml --profile dev up -d mailpit   # after LP-162, UI at http://localhost:8025

cd Web.Client
npm install
npm run dev                                                    # http://localhost:65350, proxies /api to :5138
npm run lint; npm run build; npm run i18n:check
```

## 13. Git

Full guide with commands: [`docs/git.README.md`](docs/git.README.md). The essentials:

- Branches: `Master` (releases), `Development` (integration), work in `feature/LP-xxx-…`, `fix/LP-xxx-…` or `hotfix/vX.Y.Z-…`.
- **Every branch is created from `Development`** and merged back into `Development` (squash merge). Never branch from `Master`.
- Commit message: `LP-xxx: <what, imperative mood>` (e.g. `LP-131: Add arithmetic generator`).
- Small commits; migrations in a commit of their own.
- Never commit directly to `Master`.

## 14. How AI assistants should work

- **Read before you write**: look at the affected files, the ADRs and existing patterns, and follow them.
- **One ticket or story per task**; no unrequested refactorings or "improvements" on the side.
- Unclear, or in conflict with this file or an ADR? **Ask** – don't guess.
- No new dependencies, architectural patterns, projects or top-level folders without asking first.
- **Never delete or move files** unless explicitly asked – the maintainer cleans up himself.
- Build and test after every change; don't continue while builds or tests are red.
- Finish with a short summary: what changed, which tests, open points.
- Take extra care with – and flag for human review – permissions, tenant isolation, token refresh and migrations.
- When you change a rule here or in `sources/Web.Client/AGENTS.md`, update `.github/copilot-instructions.md` and
  `.github/instructions/*.instructions.md` in the same change.
