# 007 – Tenant isolation via OrganizationId, global query filter and service checks

- Status: Proposed
- Date: 2026-10-02
- Ticket: LP-002

## Context

Families and schools share one database schema. IDs are sequential `long` values and therefore guessable.

## Decision

Everything owned by a family or school derives from `AOrganizationEntityBase` (`OrganizationId`). `MerkwerkDbContext` applies a global query filter on the current organization. **In addition**, every service checks that requested objects belong to the caller's organization.

## Alternatives considered

- One database per organization – too heavy for self-hosting on a Pi.
- Query filter only – a single `IgnoreQueryFilters()` or bug would leak data.

## Consequences

- `IgnoreQueryFilters()` only in the `Administration` module, with a comment.
- Integration tests always use two organizations.
