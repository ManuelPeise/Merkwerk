# Spike LP-006 – JWT in an HttpOnly cookie for Blazor Server and WebAssembly

Goal: prove that one JWT, transported in an HttpOnly cookie, authenticates both the Blazor Server connection
(`/admin`, SignalR) and API calls from the WebAssembly client (`/ueben`), and that it can be refreshed during a
long session without signing in again (ADR 013).

## What the spike contains

| Part | File |
| --- | --- |
| Token creation (HMAC-SHA256, 1 min in Development) | `Service/Auth/TokenService.cs`, `JwtOptions.cs` |
| Refresh tokens, rotated on every use (in memory, DB in LP-104) | `Service/Auth/InMemoryRefreshTokenStore.cs` |
| Login / refresh / logout / me | `Service/Controllers/V1/AuthController.cs` |
| JwtBearer reads the token from the cookie (or the `Authorization` header) | `Service/ServiceCollectionExtensions.cs` |
| Demo login and background refresh every 40 s | `Web/wwwroot/js/auth-spike.js`, `Web/Components/Pages/Home.razor` |
| Signed-in user in the Server area | `Web/Components/Pages/Admin/AdminHome.razor` |
| 401 → refresh → retry in the WASM client | `Web.Client/Services/RefreshingHandler.cs`, `UebenHome.razor` |

Cookies: `mw_access` (JWT, path `/`), `mw_refresh` (path `/api/v1/auth`); both `HttpOnly`, `Secure`, `SameSite=Strict`.
Demo user (Development only): `eltern@merkwerk.local` / `demo`. Signing key: user secret `Auth:Jwt:SigningKey`
(created by `deploy/setup-local.ps1`).

## How to test

Start with the `https` profile and open `https://localhost:7026/`.

1. **Login:** click "Anmelden" → "Angemeldet.". DevTools → Application → Cookies: `mw_access` and `mw_refresh`
   are present and marked HttpOnly. `document.cookie` in the console does **not** show them.
2. **Server area:** open `/admin` → shows the user and "Access-Token gültig bis" (about 1 minute ahead).
3. **Token expiry in the open connection:** wait more than 1 minute on `/admin`, click the button → still works,
   still signed in (the connection keeps the user it was opened with).
4. **Background refresh:** reload `/admin` after 2–3 minutes → still signed in, new expiry time.
   Console shows `[auth] refresh -> 200` every 40 s.
5. **WASM API call:** open `/ueben`, tap "Wer bin ich?" → name and expiry.
6. **Transparent refresh in WASM:** in DevTools delete only the cookie `mw_access` (or wait > 1 min with the
   background refresh paused via a breakpoint), tap again → still works, "Token erneuert" counts up.
7. **Logout:** "Abmelden" on the start page → `/ueben` "Wer bin ich?" shows 401, `/admin` shows "Nicht angemeldet" after reload.
8. **Refresh-token rotation:** copy the `mw_refresh` value, refresh once, then try to use the old value again
   (e.g. set it back in DevTools and call refresh) → 401.

## Results

| Check | Result |
| --- | --- |
| Cookies set, HttpOnly, not readable by JS | ✓ (PC, Chrome) |
| `/admin` shows user | ✓ (PC, Chrome) |
| Open Server connection survives token expiry | ✓ (PC, Chrome) |
| Reload after expiry still signed in (background refresh) | ✓ (PC, Chrome) |
| WASM call works, refresh on 401 transparent | ✓ (PC, Chrome) |
| Logout ends both areas | ✓ (PC, Chrome) |
| Old refresh token rejected after rotation | not tested manually – covered by unit tests in LP-104 |

## Findings

- An open Blazor Server connection keeps the user it was established with; token expiry does not interrupt it.
  Revocation (logout on another device, blocked account) therefore needs a revalidating
  `AuthenticationStateProvider` that checks the session periodically → to do in LP-104.
- New connections (page load, reconnect) need a valid access cookie → background refresh from the browser.
- Development hiccup: after LP-005 the page sometimes kept loading until the browser was refreshed / a private
  window was used – most likely the cached service worker. The service worker is now only registered outside Development.
- Decision: **ADR 013 confirmed** – JWT in HttpOnly cookie for browsers, background refresh, transparent refresh in
  the WASM client. Additions (revalidation, clock skew, DB refresh tokens) recorded in ADR 013 for LP-104.
