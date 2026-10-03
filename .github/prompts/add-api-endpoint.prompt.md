---
description: "Add a REST endpoint to Web.Core (controller, DTOs, logic, tests)"
---

Add the endpoint **${input:endpoint:POST /api/v1/module/action}** following [AGENTS.md](../../AGENTS.md) and
`.github/instructions/backend.instructions.md`:

1. Business logic in a service in the matching `Logic.*` project (interface `I…Service` + implementation, registered in
   its `DI/` extension). Data only through `Data.Accessor` / `IUnitOfWork`; explicit organisation check.
2. Controller action in `Web.Core/Services/ApiControllers/<Module>/<Module>Controller.cs` (derives from
   `ApiControllerBase`): `[Authorize(Policy = …)]` or a justified `[AllowAnonymous]`, `…Async(…, CancellationToken)`,
   `[ProducesResponseType]` for every status, `ProblemDetails` for errors.
3. DTOs as `sealed record` in `…/<Module>/Dtos/`, validation attributes on the record parameters.
4. Tests: unit tests for the service (mocked `IUnitOfWork`), integration test for the endpoint incl. permissions
   (allowed role succeeds, other role gets 403, other family gets 404/403).
5. Matching TypeScript types and `statelessApi` binding in `sources/Web.Client/src/lib/api/<module>/` if the UI uses it.
6. `dotnet build` without warnings, `dotnet test` green.
