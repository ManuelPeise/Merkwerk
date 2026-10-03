---
applyTo: "sources/**/*.cs,sources/**/*.csproj,sources/Directory.Packages.props"
description: "Rules for the .NET backend (Web.Core, Logic.*, Data.*)"
---

# Backend (.NET 10)

Authoritative: [`AGENTS.md`](../../AGENTS.md) and the ADRs in `docs/adr/`. Summary of the current structure:

## Projects and dependencies

| Project | Contains | May reference |
| --- | --- | --- |
| `Web.Core` | Startup project. `Bundels/` (service registration, pipeline), `Services/ApiControllers/<Module>/` (controller + `Dtos/`), `Services/Cookies/` | `Logic.*` (and `Data.*` for DI registration only) |
| `Logic.Authentication` | Login, token issuing, refresh-token rotation (`IAuthSessionService`, `TokenService`), `DI/` | `Logic.Shared`, `Data.Accessor` |
| `Logic.Shared` | Pure logic shared by modules (graders, generators); no I/O | – |
| `Data.Database` | Entities, `MerkwerkDbContext`, configurations, interceptors, migrations | – |
| `Data.Accessor` | Repositories, `IUnitOfWork`, `IUnitOfWorkFactory` – the only way to the database | `Data.Database` |

- **Controllers are transport only**: validate → call a `Logic.*` service → return a DTO. No business logic in `Web.Core`.
- `Logic.*` never references ASP.NET Core (no `HttpContext`, cookies, JwtBearer) and never uses `MerkwerkDbContext`
  directly – only `Data.Accessor`.
- Entities never leave the server; only DTOs (`sealed record`, in `Services/ApiControllers/<Module>/Dtos/`).

## API

- Controllers derive from `ApiControllerBase` (`[ApiController]`, route `api/v1/[controller]/[action]`), URLs lowercase
  kebab-case. Action methods end in `Async` and take a `CancellationToken`.
- Errors as `ProblemDetails` (never exception details). Validation via data annotations on DTO **parameters**
  (`[Required] string Email`), which `[ApiController]` turns into `ValidationProblemDetails`.
- Every endpoint has `[Authorize]` with a policy (`Learner`, `Member`, `OrgAdmin`, `InstanceAdmin`) except explicitly
  anonymous ones (login, setup, invitation details/accept, device pairing). Document responses with `[ProducesResponseType]`.
- Auth cookies only through `AuthCookieWriter` (HttpOnly, SameSite=Strict, Secure outside development).

## Data access

- Entities derive from `AEntityBase` / `AOrganizationEntityBase`; audit fields are set by the interceptor, never by hand.
- Time is UTC from `TimeProvider` – never `DateTime.Now`/`UtcNow`.
- One unit of work per business operation (`await using var uow = _uowFactory.Create();`). Reads: `AsNoTracking()` +
  `Select` into DTOs. Tenant isolation: global query filter **plus** an explicit organisation check in the service.
- Migrations: `dotnet ef migrations add <Name> -p Data.Database -s Web.Core`; small, schema and data never mixed,
  in a commit of their own.

## C# conventions

- File-scoped namespaces, one type per file, namespace = project + folder. Interfaces `I…`, abstract base classes `A…`.
- `sealed` by default, `record` for DTOs, primary constructors for DI; no service locator, no static state.
- Options classes with `SectionName`, bound with `ValidateOnStart`. Secrets from user secrets / environment only.
- NuGet versions only in `sources/Directory.Packages.props` (Central Package Management) – `<PackageReference>` without
  `Version`. Ask before adding a package.
- No warnings. Tests: xUnit, NSubstitute, Testcontainers (MySQL); names `Method_State_ExpectedResult`, Arrange/Act/Assert.
