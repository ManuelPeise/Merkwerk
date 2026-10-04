# 016 – The app applies pending migrations at startup

- Status: Accepted
- Date: 2026-10-04
- Ticket: LP-164
- Supersedes: the migration part of [009](009-docker-compose-caddy-multiarch.md)

## Context

ADR 009 applied migrations with a separate one-shot bundle image (`docker compose run --rm migrate`) before the new app
version starts. That is an extra manual step on every update of a self-hosted instance, and forgetting it leaves the app
running against an old schema. Merkwerk runs as a single process (ADR 002), so there is never more than one instance
that could migrate at the same time.

## Decision

`Web.Core` applies pending EF Core migrations on every start (`MigrateDatabaseAsync` in `Web.Core/Bundles`), in every
environment, before it accepts requests. Without pending migrations nothing happens. Existing data stays: migrations are
incremental schema changes, the database is never dropped or recreated by the app. If a migration fails, the start fails.

## Alternatives considered

- Keep the bundle image only (ADR 009) – safe, but an easy-to-forget manual step for families who self-host.
- Migrate only in development – development and production would behave differently.
- `EnsureCreated` – cannot evolve a schema and ignores migrations.

## Consequences

- Updating an instance is `docker compose pull && docker compose up -d`.
- MySQL does not run DDL transactionally: a failed migration can leave a half-applied schema. Migrations stay small and
  never mix schema and data changes (AGENTS.md §5); back up the database before an update (nightly `mysqldump`, ADR 009).
- Only one app instance may run against a database (ADR 002 already requires that).
- The migration bundle image stays available for manual use (e.g. migrating before the start); removing it is a
  separate decision.
