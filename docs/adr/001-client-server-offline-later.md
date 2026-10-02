# 001 – Client/server architecture; offline mode in a later stage

- Status: Accepted
- Date: 2026-10-02
- Ticket: LP-002

## Context

Adults create exercises on a laptop, children practise on tablets, and progress and sharing need one consistent data set. Practising away from home is not needed at first.

## Decision

Merkwerk is a client/server application. The server owns all data and grades authoritatively. An offline mode (download assigned exercises, practise locally, sync attempts later) is planned for a later stage; the MVP must not block it.

## Alternatives considered

- Pure offline app per device – no shared state between adults and children, no central progress.
- Offline-first from day one – doubles the complexity of the MVP for a need that does not exist yet.

## Consequences

- Grading rules are data-driven and live in `Logic.Shared` so they can later run in the browser.
- `Attempt` and `Answer` carry a client-generated `Guid ClientId` from the start (see ADR 008).
- Submitting an answer is an idempotent `PUT`.
