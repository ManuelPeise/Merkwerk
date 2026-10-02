# 012 – Repository and unit of work in a separate accessor project

- Status: Accepted
- Date: 2026-10-02
- Ticket: LP-002

## Context

The developer wants repositories and units of work. With Blazor Server, a DbContext must not live as long as a user's circuit.

## Decision

`Data.Database` holds the EF model (entities, `MerkwerkDbContext`, configurations, interceptors, migrations). `Data.Accessor` holds `IRepository<T>`, specialised repositories, `IUnitOfWork` and `IUnitOfWorkFactory`. **One unit of work per business operation**, created via `IDbContextFactory`. `Logic` references only the accessor project and never uses `MerkwerkDbContext` or `DbSet<T>` directly.

## Alternatives considered

- Using DbContext directly in services – simpler, but not the developer's preferred pattern.
- DbContext scoped to the Blazor circuit – stale data and concurrency errors.

## Consequences

- `IRepository<T>.Query()` returns `IQueryable` for DTO projections – EF knowledge leaks into Logic, accepted as pragmatic.
- Services are unit-testable with a mocked `IUnitOfWork`.
