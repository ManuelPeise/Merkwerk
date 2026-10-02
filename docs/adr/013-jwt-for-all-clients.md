# 013 – JWT for all clients; HttpOnly cookie in browsers; refresh tokens in the database

- Status: Accepted
- Date: 2026-10-02
- Ticket: LP-002

## Context

The developer wants JWT. Browsers must not expose tokens to JavaScript; a later native app needs a header-based token.

## Decision

ASP.NET Core Identity manages adult accounts. The server issues a JWT access token (15 min) and a rotating refresh token (adults 14 days, devices 180 days, stored hashed, revocable). Browsers receive the token in an `HttpOnly`, `Secure`, `SameSite=Strict` cookie; native clients send it in the `Authorization` header. The JwtBearer handler accepts both.

## Alternatives considered

- Cookie authentication only – not the developer's choice; a native app would need a second scheme.
- JWT in localStorage – readable by XSS.

## Consequences

- One token format for all clients.
- Refreshing the token during long Blazor Server sessions is the riskiest part – it is covered by the spike LP-006.
