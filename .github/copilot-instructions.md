# Copilot instructions – Merkwerk

These instructions summarise the project rules for GitHub Copilot. The **authoritative** rules are in
[`AGENTS.md`](../AGENTS.md) (repository) and [`sources/Web.Client/AGENTS.md`](../sources/Web.Client/AGENTS.md) (UI);
Claude Code uses the same files. If something here contradicts them or an ADR in `docs/adr/`, AGENTS.md and the ADR win.
Path-specific rules: `.github/instructions/*.instructions.md`.

## Project

Merkwerk is an open-source learning platform (MIT) for children in grades 1–4; adults (parents, teachers) create and
assign exercises. Self-hosted, the family instance runs on a Raspberry Pi (ARM64) with Docker Compose.

| Part | Path | Stack |
| --- | --- | --- |
| UI | `sources/Web.Client` | React 19, TypeScript, Vite, MUI, react-router-dom, axios, i18next (npm) |
| API host | `sources/Web.Core` | ASP.NET Core (.NET 10), controllers under `/api/v1`, OpenAPI/Swagger |
| Business logic | `sources/Logic.*` | `Logic.Authentication`, `Logic.Shared`, later more `Logic.*` modules |
| Data | `sources/Data.Database`, `sources/Data.Accessor` | EF Core 10, MySQL 8.4 (MySql.EntityFrameworkCore, **no Pomelo**) |
| Operations | `deploy/` | Docker Compose, Caddy, MySQL (dev: plain HTTP, no certificate) |

## How to work

- Every change belongs to a ticket `LP-xxx`. Work on `feature/LP-xxx-…` or `fix/LP-xxx-…`, **always branched from `Development`**.
  Commit messages: `LP-xxx: <imperative summary>` (e.g. `LP-124: Add invitation page`).
- Read the affected files and follow existing patterns before writing code. One ticket per task, no unrequested refactorings.
- **Ask before** adding NuGet/npm packages, new projects, new top-level folders or new architectural patterns.
- **Never delete or move files** unless explicitly asked – the maintainer cleans up himself.
- Unclear or contradicting AGENTS.md/an ADR? Ask instead of guessing.
- Finish with a short summary: what changed, how it was tested, open points.
- Reply to the developer in **German**; code, identifiers, comments, commits and docs in **English**. UI texts come from
  translation resources (German is the source language).
- Shell commands in **PowerShell** syntax (Windows). PowerShell scripts (`*.ps1`): ASCII only or UTF-8 with BOM.
- Line endings: Git defaults (CRLF on Windows). Formatting: `.editorconfig` (4 spaces C#/TS, 2 spaces JSON/JS/HTML/CSS) and Prettier in the UI.

## Security and privacy (non-negotiable)

- No tracking, analytics, advertising or error-reporting SDKs; no external CDNs (fonts are self-hosted).
- No children's data to third parties (no external speech, translation or AI services). Children: first name/pseudonym,
  grade, avatar only – no e-mail, no date of birth, no photos.
- Never commit secrets: user secrets, `deploy/.env` (ignored), `*.crt`/`*.pfx`/`*.key` are never committed.
- Logs contain no personal data (no names, e-mail addresses, tokens, children's answers).
- Auth: JWT in HttpOnly cookies (`mw_access`, `mw_refresh`), refresh-token rotation (ADR 013). The browser client never
  reads or stores tokens. The server's `[Authorize]` and tenant checks protect data – UI guards only improve the UX.

## Definition of Done

- Acceptance criteria of the ticket met.
- Backend: `dotnet build` without warnings, `dotnet test` green; new logic has unit tests; repositories, query filters and
  controllers have integration tests; permissions tested (may X do this, may Y **not**).
- UI: `npm run lint`, `npm run build` and `npm run i18n:check` green (in `sources/Web.Client`); all new texts in `de` and `en`.

## Commands

```powershell
cd sources
dotnet build Merkwerk.slnx
dotnet test Merkwerk.slnx
dotnet run --project Web.Core --launch-profile http        # http://localhost:5138, Swagger /swagger

cd Web.Client
npm run dev            # http://localhost:65350, proxies /api to :5138
npm run lint; npm run build; npm run format
```
