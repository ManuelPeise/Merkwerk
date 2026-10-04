---
applyTo: "sources/**/*.cs,sources/**/*.csproj,sources/Directory.Packages.props"
description: "Rules for the .NET backend (Web.Core, Logic.*, Data.*)"
---

# Backend (.NET 10)

Authoritative: [`AGENTS.md`](../../AGENTS.md) and the ADRs in `docs/adr/`. Summary of the current structure:

## Projects and dependencies

| Project | Contains | May reference |
| --- | --- | --- |
| `Web.Core` | Startup project. `Bundles/` (service registration, pipeline, startup migration), `Services/ApiControllers/<Module>/` (controller + `Dtos/`), `Services/Cookies/` | `Logic.*`, `Shared` (and `Data.*` for DI registration only) |
| `Logic.Authentication` | Login, token issuing, refresh-token rotation (`TokenService`), accounts, `DI/` | `Logic.Shared`, `Logic.Notifications`, `Data.Accessor`, `Shared` |
| `Logic.Organizations`, `Logic.Devices` (other `Logic.*`) | Setup, families, invitations, child profiles (LP-105); device pairing, children's sessions and the combined refresh/logout `ISessionService` (LP-106), `DI/` | `Logic.Shared`, `Logic.Notifications`, `Logic.Authentication`, `Data.Accessor`, `Shared` |
| `Logic.Notifications` | Mail templates, SMTP, public links | `Logic.Shared`, `Shared` |
| `Logic.Shared` | Service interfaces (`Logic.Shared.Interfaces`); later pure graders and generators, no I/O | `Shared` |
| `Shared` | Enums (`Shared.Enums`) and service models (`Shared.Models.<Module>`) | – |
| `Data.Database` | Entities (`<Name>Entity`), `MerkwerkDbContext`, configurations, interceptors, migrations | `Shared` |
| `Data.Accessor` | Repositories, `IUnitOfWork`, `IUnitOfWorkFactory` – the only way to the database | `Data.Database`, `Shared` |

- **Controllers are transport only**: validate → call a `Logic.*` service → return a DTO. No business logic in `Web.Core`.
- `Logic.*` never references ASP.NET Core (no `HttpContext`, cookies, JwtBearer) and never uses `MerkwerkDbContext`
  directly – only `Data.Accessor`.
- Entities never leave the server; only DTOs (`sealed record`, in `Services/ApiControllers/<Module>/Dtos/`).
- Where types live: service interfaces in `Logic.Shared.Interfaces`, enums in `Shared.Enums`, service models in
  `Shared.Models`; API DTOs stay in `Web.Core` (enforced by `Architecture.Tests`).

## API

- Controllers derive from `ApiControllerBase` (`[ApiController]`, route `api/v1/[controller]/[action]`), URLs lowercase
  kebab-case. Action methods end in `Async` and take a `CancellationToken`.
- Errors as `ProblemDetails` (never exception details). Validation via data annotations on DTO **parameters**
  (`[Required] string Email`), which `[ApiController]` turns into `ValidationProblemDetails`.
- Every action has exactly one `[Authorize(Policy = …)]` (`Member`, `OrgAdmin`, `Learner`, `AnySession`,
  `PasswordChangeAllowed`) or `[AllowAnonymous]` (login, setup, invitation details/accept, device endpoints) – never a bare
  `[Authorize]`, never `Roles = …`. Permission matrix: AGENTS.md §9; `Architecture.Tests/EndpointPolicyTests` must be
  updated with every new endpoint. Services check membership and role in the database again. Document responses with
  `[ProducesResponseType]`.
- Auth cookies only through `AuthCookieWriter` (HttpOnly, SameSite=Strict, Secure outside development).
- Children's tokens (role `Learner`, `sub` = learner id, claims `learner_id`, `device_id`) pass only the policies
  `Learner` and `AnySession` (me); `CurrentUserId` is `null` for them (LP-106). Device endpoints identify the device by
  the cookie `mw_device` and answer `403` (not `401`) for unpaired devices.

## Data access

- Entities derive from `AEntityBase` / `AOrganizationEntityBase`; audit fields are set by the interceptor, never by hand.
- Time is UTC from `TimeProvider` – never `DateTime.Now`/`UtcNow`.
- One unit of work per business operation (`await using var uow = _uowFactory.Create();`). Reads: `AsNoTracking()` +
  `Select` into DTOs. Tenant isolation: global query filter **plus** an explicit organisation check in the service.
  `IgnoreQueryFilters()` only in `Administration` and in the named lookups listed in AGENTS.md §5 (LP-105 login/invitations,
  LP-106 anonymous device requests – filtered explicitly by the device's organization).
- Migrations: `dotnet ef migrations add <Name> -p Data.Database -s Web.Core`; small, schema and data never mixed,
  in a commit of their own. The app applies pending migrations at startup (ADR 016).

## C# conventions

- File-scoped namespaces, one type per file, namespace = project + folder. Interfaces `I…`, abstract base classes `A…`.
- `sealed` by default, `record` for DTOs. Classes with DI use a constructor that assigns `private readonly` fields
  (`private readonly IMyService _myService;`) – no primary constructors for DI (records, DTOs and tests may keep them).
  No service locator, no static state.
- Options classes with `SectionName`, bound with `ValidateOnStart`. Secrets from user secrets / environment only.
- NuGet versions only in `sources/Directory.Packages.props` (Central Package Management) – `<PackageReference>` without
  `Version`. Ask before adding a package.
- No warnings. Tests: xUnit, NSubstitute, Testcontainers (MySQL); names `Method_State_ExpectedResult`, Arrange/Act/Assert.
