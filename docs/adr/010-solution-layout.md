# 010 – Solution layout with numbered solution folders

- Status: Accepted
- Date: 2026-10-02
- Ticket: LP-002

## Context

The developer wants a clear, layered solution in Visual Studio. Project name: Merkwerk.

## Decision

`sources/Merkwerk.slnx` with solution folders `01 Web` (Web, Web.Client), `02 Service` (Service), `03 Logic` (Logic, Logic.Shared), `04 Data` (Data.Database, Data.Accessor), `05 Shared` (Shared), `06 Tests`. Physical project folders are flat under `sources/`. Allowed references are listed in AGENTS.md §3 and checked by `Architecture.Tests`.

## Alternatives considered

- One project per module from the start – too many projects for a solo developer; split later if needed.
- Clean-architecture naming (Domain/Application/Infrastructure) – the developer prefers Web/Service/Logic/Data/Shared.

## Consequences

- `Web.Client` never references Logic or Data (no EF Core in the browser).
- Business logic starts as one `Logic` project with one folder per module.
