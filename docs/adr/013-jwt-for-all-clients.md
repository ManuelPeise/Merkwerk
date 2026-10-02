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
- Refreshing the token during long Blazor Server sessions was the riskiest part – confirmed by spike LP-006 (see below).

## Spike results (LP-006, 2026-10-02)

Confirmed in `docs/spikes/LP-006-jwt-cookie.md`:

- `mw_access` (JWT, path `/`) and `mw_refresh` (path `/api/v1/auth`) are set as `HttpOnly`, `Secure`, `SameSite=Strict`
  cookies and are not readable from JavaScript.
- The JwtBearer handler takes the token from the cookie when no `Authorization` header is present.
- An open Blazor Server connection keeps the user it was established with; token expiry does not interrupt it.
- New page loads and reconnects stay authenticated because the browser refreshes the cookie in the background
  (at about 80 % of the access-token lifetime).
- The WASM client refreshes once on `401` and retries the request transparently.
- Logout revokes the refresh token and ends both areas.

Additions to the decision:

- **Revalidation:** because an open Server connection does not notice token expiry, a revalidating
  `AuthenticationStateProvider` checks the session every few minutes (revoked refresh token, blocked account)
  and signs the circuit out. To be implemented in LP-104.
- **Clock skew** for token validation is 5 seconds (the 5-minute default would hide short lifetimes).
- **Refresh tokens** move from the spike's in-memory store to a database table with hashed tokens (LP-104).

