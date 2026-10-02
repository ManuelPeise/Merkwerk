# 006 – Children sign in on paired devices without passwords

- Status: Proposed
- Date: 2026-10-02
- Ticket: LP-002

## Context

Children aged 6–10 cannot manage passwords or email addresses, and we want to store as little data about them as possible.

## Decision

An adult pairs a tablet with a short-lived code (QR, 10 minutes). The device receives a long-lived, revocable device refresh token. A child signs in by choosing their profile picture, optionally with a 3-symbol picture PIN, and receives an access token bound to device and child.

## Alternatives considered

- Username/password for children – unsuitable for the age group.
- No authentication for children – any device could read a child's data.

## Consequences

- No email or password stored for children.
- Losing a device = revoking its token in the adult area.
