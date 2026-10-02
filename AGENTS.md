# AGENTS.md – Merkwerk

Working agreement for AI assistants (Claude Code, Copilot, Codex, …) and human contributors.
This file is the **single source of truth** for conventions. `CLAUDE.md` only imports it.

## 1. What this is

Merkwerk is an open-source learning platform (MIT). Adults (parents, teachers) create exercises in German,
English and maths and assign them to children; children (initially aged 6–10, older age groups later)
practise on a tablet. Deployment: self-hosted, the family instance runs on a Raspberry Pi (ARM64).

- Product concept, architecture, exercise types and tickets live **outside** this repo (planning folder).
  Tickets are named `LP-xxx`; every change belongs to a ticket.
- Architecture decisions: `docs/adr/` (ADR 001–014). If something contradicts an ADR, the ADR wins – not your assumption.
- Further documentation:
  - [`docs/infrastructure.README.md`](docs/infrastructure.README.md) – where things live inside each project, runtime setup, configuration, CI/CD.
  - [`docs/git.README.md`](docs/git.README.md) – branch model, commands, pull requests, releases, migration conflicts.

## 2. Tech stack (fixed – change only via ADR)

| Area | Choice |
| --- | --- |
| Runtime | .NET 10, C# (latest language version), `Nullable` enabled, `ImplicitUsings` enabled |
| Adult UI | Blazor Web App, render mode **Interactive Server**, routes `/admin/...` |
| Children's UI | Blazor **Interactive WebAssembly** as a PWA, routes `/ueben/...` |
| API | ASP.NET Core **controllers** under `/api/v1`, OpenAPI, errors as `ProblemDetails` |
| Database | MySQL 8.4 LTS, EF Core 10, provider **MySql.EntityFrameworkCore** (Oracle). **No Pomelo.** |
| Auth | ASP.NET Core Identity + **JWT** (access token 15 min, rotating refresh token stored in the DB). In browsers the token travels in an `HttpOnly` cookie. |
| MVVM | CommunityToolkit.Mvvm |
| Tests | xUnit, NSubstitute, bUnit, Testcontainers (MySQL), NetArchTest, Playwright |
| Operations | Docker Compose (Caddy, app, MySQL), multi-arch images (amd64 + arm64) |

Ask before adding any NuGet package; versions are managed centrally in `Directory.Packages.props`.

## 3. Solution layout and dependency rules

```
sources/Merkwerk.slnx
  01 Web     Web            Host process, Blazor Server (adults), adult view models
             Web.Client     Blazor WebAssembly (children, PWA), children's view models
  02 Service Service        API controllers, auth cookies, JWT validation, OpenAPI – transport only, no business logic
  03 Logic   Logic          Business logic, one folder per module
             Logic.Authentication  Login, token issuing, refresh-token rotation (no ASP.NET Core)
             Logic.Shared   Graders (IGrader), generators – also runs in the browser
  04 Data    Data.Database   Entities, DbContext, configurations, interceptors, migrations
             Data.Accessor  Repositories and unit of work – the only way to reach the database
  05 Shared  Shared         DTOs, enums, constants
  06 Tests   *.Tests
```

| Project | May reference |
| --- | --- |
| Shared | – |
| Logic.Shared | Shared |
| Data.Database | Shared |
| Data.Accessor | Data.Database, Shared |
| Logic | Logic.Shared, Shared, Data.Accessor |
| Logic.Authentication | Logic.Shared, Shared, Data.Accessor |
| Service | Logic, Logic.Authentication, Shared |
| Web.Client | Logic.Shared, Shared |
| Web | Web.Client, Service, Logic, Data.Accessor, Data.Database (both for DI registration only) |

**Hard rules** (enforced by `Architecture.Tests`):

- `Web.Client` **never** references `Logic`, `Logic.Authentication` or any `Data.*` project (otherwise EF Core ends up in the children's download).
- `Logic` and `Logic.Authentication` reference **only** `Data.Accessor`, never `Data.Database` directly. Logic may use entity types and EF Core's async query
  extensions (`ToListAsync`, `AnyAsync`, …) but never `MerkwerkDbContext`, `DbSet<T>` or `DbContextOptions`.
- `Logic.Shared` has **no** dependency on EF Core, ASP.NET Core or I/O – pure logic only.
- `Service` contains **no business logic**: controllers, cookie transport and middleware setup only. Logic goes into a `Logic.*` project.
- `Logic.*` projects do not reference ASP.NET Core (`Microsoft.AspNetCore.App`); cookies, `HttpContext` and JwtBearer stay in `Service`.
- **Entities never leave the server.** Only DTOs from `Shared` go over the wire.
- View models and controllers talk to **services**, never directly to repositories or the DbContext.

## 4. Modules (folders in `Logic`)

`Organizations`, `Learners`, `Exercises`, `Assignments`, `Practice` (attempts, answers, learning state),
`Progress`, `WordLists`, later `Sharing`, `Administration`.
Per module: `I<Name>Service` + implementation, validators (FluentValidation), module-internal types.
Authentication is its own project, `Logic.Authentication` (`IAuthSessionService`, `TokenService`, options), so it can
grow (Identity, device pairing, refresh tokens in the DB – LP-104) without touching the other modules.

## 5. Data access (ADR 004, 007, 008, 011, 012)

- Every entity derives from `AEntityBase` (`long Id`, `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`);
  everything owned by a family/school derives from `AOrganizationEntityBase` (`long OrganizationId`).
- **Never set audit fields by hand** – the `AuditSaveChangesInterceptor` does that.
- Time is always **UTC** and always comes from `TimeProvider`; never call `DateTime.Now`/`UtcNow` directly.
- IDs: `long` (AUTO_INCREMENT). `Attempt` and `Answer` additionally have a unique `Guid ClientId` for idempotency/offline use.
- Split: `Data.Database` owns the EF model (entities, `MerkwerkDbContext`, configurations, interceptors, converters, migrations);
  `Data.Accessor` owns `IRepository<T>`, specialised repositories, `IUnitOfWork` and `IUnitOfWorkFactory`.
- Access data only through an `IUnitOfWork` from `IUnitOfWorkFactory`; **one unit of work per business operation**
  (`await using var uow = _uowFactory.Create();`). Never hold a DbContext in Blazor components or long-lived services.
- Read queries: `Query()` + `AsNoTracking()` + `Select(...)` into DTOs. Don't load whole entity graphs just to build DTOs.
- Tenant isolation: global query filter on `OrganizationId` **plus** an explicit check in the service
  (IDs are sequential and guessable). Use `IgnoreQueryFilters()` only in the `Administration` module, with a comment explaining why.
- Exercise content (`Question.Payload`, `Question.Solution`, generator parameters) is stored in JSON columns with polymorphic types
  (`System.Text.Json`, type discriminator). A new question type = a new class, **no** migration.
- Migrations: `dotnet ef migrations add <Name> -p Data.Database -s Web`. Names in English, PascalCase.
  MySQL does not run DDL transactionally → keep migrations small, never mix schema and data changes.
- Character set `utf8mb4`, collation `utf8mb4_0900_ai_ci`.

## 6. Graders, generators, learning state

- Every question type has exactly one `IGrader` in `Logic.Shared` and one Razor component in the children's client.
- The client grades for instant feedback; **the server's grading is authoritative**. Solutions are never sent to the client –
  only the grading rules the grader needs.
- Generators (`IExerciseGenerator`) are **deterministic**: same seed → same exercises. No `Random.Shared`.
- Every new or changed grader needs test cases in `shared/grading-cases/<type>/*.json`.
- Learning state: one generic `LearningState` (Leitner boxes 1–5) keyed by item, e.g. `word:{id}:write`, `math:mul:7x8`.

## 7. Blazor, MVVM and UI

- Render mode per area: `/admin` = `InteractiveServer`, `/ueben` = `InteractiveWebAssembly`. Do not set it globally.
- Use MVVM only for pages with real logic (editor, practice player, progress): view model with `[ObservableProperty]` /
  `[RelayCommand]`, component inherits `MvvmComponentBase<TViewModel>`. Simple pages stay plain components.
- No business logic in code-behind; Razor files bind only to the view model or parameters.
- **Design tokens**: colours, fonts and spacing only via CSS variables generated from `shared/design-tokens`. No hex colours in markup.
- **Children's UI**: touch targets ≥ 64×64 px; little text, icons plus read-aloud; feedback never by colour alone
  (always add an icon); mistakes are friendly ("Try again"), never harsh red; number input via the on-screen keypad.
- Accessibility: WCAG AA contrast, every control reachable by keyboard, `aria-label` on icon buttons.
- **UI language is German.** UI strings are never hard-coded in markup but come from resources via `IStringLocalizer`
  (more languages and older age groups later).
- JS interop only for read-aloud (Web Speech API) and drag & drop (SortableJS), each wrapped in its own service.

## 8. API

- Controllers live in `Service`, route `api/v1/[controller]`, and stay thin: validate → call service → return DTO.
- Errors as `ProblemDetails`; never leak exception details.
- Submitting an answer is an idempotent `PUT`. Every endpoint has `[Authorize]` with a matching policy
  (`Learner`, `Member`, `OrgAdmin`, `InstanceAdmin`), except login and device pairing.

## 9. Security and privacy (non-negotiable)

- No tracking, analytics or advertising SDKs. No external CDNs in the children's client.
- No children's data to third parties (including speech recognition, translation or AI services).
- Store only first name/pseudonym, grade and avatar for children. No email, no date of birth, no photos.
- Never commit secrets: use `.env`, `appsettings.*.local.json` or user secrets. Sample values only in `.env.example`.
- Passwords only through ASP.NET Core Identity; store refresh tokens hashed only.
- Logs contain no personal data (no names, no children's answers).

## 10. C# conventions

- Code, identifiers, comments, commit messages and documentation in **English**; only UI strings are German.
- File-scoped namespaces; one type per file; namespace = project + folder.
- Prefixes: interfaces `I…`, abstract base classes `A…` (e.g. `AEntityBase`).
- Async methods end in `Async` and take a `CancellationToken` as the last parameter.
- `sealed` for classes not designed for inheritance; `record` for DTOs.
- Primary constructors for DI are fine. No service locator, no static state.
- No warnings in commits (`TreatWarningsAsErrors` in Release).
- PowerShell scripts (`*.ps1`): ASCII only, or save as UTF-8 **with BOM**. Windows PowerShell 5.1 reads UTF-8 without BOM
  as ANSI, and characters like `–` turn into quote marks that break the script.

## 11. Tests – Definition of Done

A ticket is done when:

1. all acceptance criteria of the ticket are met,
2. `dotnet build` has no warnings and `dotnet test` is green,
3. new logic has unit tests (services with a mocked `IUnitOfWork`, view models without UI),
4. changes to repositories, query filters or controllers have an integration test against MySQL (Testcontainers),
5. grader changes have JSON test cases,
6. the architecture test is green,
7. permissions are tested (may X do this? may Y **not** do this?).

Test names: `Method_State_ExpectedResult`. Structure tests as Arrange/Act/Assert.

## 12. Commands

```powershell
cd sources
dotnet build Merkwerk.slnx
dotnet test Merkwerk.slnx
dotnet run --project Web
dotnet ef migrations add <Name> -p Data.Database -s Web
docker compose -f ../deploy/docker-compose.yml up -d db
```

## 13. Git

Full guide with commands: [`docs/git.README.md`](docs/git.README.md). The essentials:

- Branches: `Master` (releases), `Development` (integration), work in `feature/LP-xxx-…`, `fix/LP-xxx-…` or `hotfix/vX.Y.Z-…`.
- **Every branch is created from `Development`** and merged back into `Development`. Never branch from `Master`.
- Commit message: `LP-xxx: <what, imperative mood>` (e.g. `LP-131: Add arithmetic generator`).
- Small commits; migrations in a commit of their own.
- Never commit directly to `Master`.

## 14. How AI assistants should work

- **Read before you write**: look at the affected files, the ADRs and existing patterns, and follow them.
- **One ticket or story per task**; no unrequested refactorings or "improvements" on the side.
- Unclear, or in conflict with this file or an ADR? **Ask** – don't guess.
- No new dependencies, architectural patterns or projects without asking first.
- Build and test after every change; don't continue while tests are red.
- Finish with a short summary: what changed, which tests, open points.
- Take extra care with – and flag for human review – permissions, tenant isolation, token refresh and migrations.
