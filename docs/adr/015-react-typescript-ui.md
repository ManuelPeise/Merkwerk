# 015 – React/TypeScript UI, backend as pure REST API

- Status: Accepted
- Date: 2026-10-03
- Ticket: LP-011
- Supersedes: [003](003-blazor-for-all-expo-later.md), [014](014-lightweight-mvvm.md)

## Context

The Blazor spikes (LP-005 to LP-007) worked, but the developer prefers React/TypeScript for the UI: larger ecosystem,
better touch/PWA tooling, and a later Expo app can reuse TypeScript code (API types, graders). The backend stays .NET.

## Decision

- **UI:** one React 19 + TypeScript app (`sources/Web.Client`, Vite, MUI, react-router-dom, axios, i18next) for adults
  (`/admin`) and children (`/practice`, installable as PWA). Rules in `sources/Web.Client/AGENTS.md`.
- **Backend:** `sources/Web.Core` hosts only the REST API (`/api/v1`, OpenAPI). It does not render or serve the UI.
- **Delivery:** in production Caddy serves the built UI as static files and proxies `/api/*` to `Web.Core`.
  In development Vite serves the UI and proxies `/api` to `http://localhost:5138` (plain HTTP).
- **Auth:** unchanged JWT in HttpOnly cookies (ADR 013); the browser never handles tokens.
- **Graders:** C# in `Logic.Shared` is authoritative; a TypeScript grader in the UI gives instant feedback. Both pass
  `shared/grading-cases`.

## Alternatives considered

- Keep Blazor (ADR 003) – works, but a smaller ecosystem and C#-only UI code that a later Expo app cannot reuse.
- React UI served by ASP.NET – couples UI deployment to the API process; Caddy already runs in front.

## Consequences

- Two languages in the repo (C#, TypeScript) and two toolchains (dotnet, npm).
- MVVM with CommunityToolkit.Mvvm (ADR 014) is obsolete; UI state lives in React hooks.
- Tickets written for Blazor (LP-112, LP-116 ff.) are implemented in React.
- API DTOs are mirrored as TypeScript types in `Web.Client/src/lib/api/<module>/`.
