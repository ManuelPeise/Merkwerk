# 004 – MySQL 8.4 LTS with EF Core 10 and the Oracle provider (no Pomelo)

- Status: Accepted
- Date: 2026-10-02
- Ticket: LP-002

## Context

The developer prefers MySQL. The widely used community provider Pomelo.EntityFrameworkCore.MySql had no release for EF Core 10 (latest 9.0.0) at decision time; the official Oracle provider MySql.EntityFrameworkCore 10.0.x supports EF Core 8–10.

## Decision

Use MySQL 8.4 LTS with EF Core 10 and **MySql.EntityFrameworkCore** (Oracle). Pomelo is not used. If the Oracle provider causes problems, fall back temporarily to EF Core 9 with provider 9.0.x and upgrade later.

## Alternatives considered

- Pomelo – no EF Core 10 support; explicitly not wanted.
- PostgreSQL – good fit technically, but not the developer's choice.
- MariaDB – close, but not fully compatible in details.

## Consequences

- Character set `utf8mb4`, collation `utf8mb4_0900_ai_ci`.
- DDL is not transactional in MySQL: keep migrations small, back up before migrating.
- The provider is confined to `Data.Database`, so a switch is a local change.
