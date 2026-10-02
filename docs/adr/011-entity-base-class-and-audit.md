# 011 – All entities derive from AEntityBase; audit fields set by an interceptor

- Status: Accepted
- Date: 2026-10-02
- Ticket: LP-002

## Context

Every table needs an ID and audit information; setting it by hand is error-prone.

## Decision

`AEntityBase` (abstract) provides `long Id`, `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`. `AOrganizationEntityBase` adds `OrganizationId`. An `AuditSaveChangesInterceptor` fills audit fields from `ICurrentUser` (`user:{id}`, `learner:{id}`, `system`). Times are UTC from `TimeProvider`.

## Alternatives considered

- Audit fields set in services – easy to forget.
- Database triggers – logic hidden from the code, harder to test.

## Consequences

- Audit can't be forgotten.
- A UTC value converter is needed because MySQL `DATETIME` has no time zone.
