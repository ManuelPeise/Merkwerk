# Web.Client

Merkwerk's React and TypeScript single-page application. It contains the shared public entry and adult account pages,
the `/admin` area, and the children's `/practice` flow. It communicates with the ASP.NET Core host in `Web.Core`
through `/api/v1`; the backend remains authoritative for grading, access control, and tenant isolation.

Contributor conventions are in [AGENTS.md](AGENTS.md); repository-wide architecture and security rules are in
[../../AGENTS.md](../../AGENTS.md).

## Stack

React 19 · TypeScript 6 · Vite 8 · MUI 9 · React Router 7 · Axios · i18next · ESLint · Prettier · npm

## Prerequisites

- Node.js LTS and npm
- .NET 10 SDK to run the backend
- The development JWT signing key in user secrets; see the root [README](../../README.md)

## Run locally

Start the backend in one PowerShell terminal:

```powershell
cd sources
dotnet run --project Web.Core --launch-profile http
```

It listens at `http://localhost:5138` and exposes Swagger at `/swagger`. Start the UI in another terminal:

```powershell
cd sources\Web.Client
npm ci
npm run dev
```

Vite serves the UI at `http://localhost:65350` and proxies `/api` requests to `Web.Core` on port 5138. Both use plain
HTTP during development. See [the infrastructure guide](../../docs/infrastructure.README.md) for local MySQL and Mailpit.

To open the development UI from a phone or tablet on the LAN:

```powershell
npm run dev -- --host
```

Open `http://<your-PC-IP>:65350` from the device. This is a development server, not a production deployment.

## E2E tests (Playwright)

Playwright tests are in `e2e/` and run against the local stack. Start prerequisites first:

1. backend API (`Web.Core`) on `http://localhost:5138`
2. MySQL
3. Mailpit on `http://localhost:8025`
4. UI dev server (Playwright starts this automatically via `webServer` if not running)

The tests need a **fresh database**: a global setup creates the E2E owner on first run (and stops with a clear message
if the instance was already set up by hand). The browser language is set to German (`de-DE`), because the tests use the
German texts.

```powershell
cd sources
dotnet ef database drop -p Data.Database -s Web.Core --force   # then restart Web.Core (it migrates at startup)
```

Run smoke tests:

```powershell
cd sources\Web.Client
npm run test:e2e
```

Interactive runner:

```powershell
npm run test:e2e:ui
```

## Scripts

| Command                | Purpose                                                   |
| ---------------------- | --------------------------------------------------------- |
| `npm run dev`          | Vite development server with hot reload                   |
| `npm run build`        | TypeScript project build and production bundle to `dist/` |
| `npm run preview`      | Serve the built bundle locally                            |
| `npm run lint`         | ESLint and translation-key parity/prefix checks           |
| `npm run lint:fix`     | ESLint with automatic fixes                               |
| `npm run format`       | Format UI files with Prettier                             |
| `npm run format:check` | Check UI formatting without writing                       |
| `npm run i18n:check`   | Check translation keys in German and English              |
| `npm run test:e2e`     | Run Playwright smoke tests (desktop + mobile)             |
| `npm run test:e2e:ui`  | Run Playwright in UI mode                                 |

## Structure

```text
src/
  assets/avatars/       built-in child profile avatar SVGs and IDs
  components/           shared input, layout, feedback, icon, and child UI
  hooks/                reusable hooks
  lib/api/              stateless API bindings, transport types, and error mapping
  lib/auth/             role constants and authentication validation
  lib/translations/     i18next setup, typed keys, German and English resources
  navigation/           route constants, route tree, and route guards
  pages/                route-level UI
scripts/                translation validation
```

## Conventions

- Application imports start with `src/`; do not use relative imports.
- UI text and accessible labels come from `getResource` in `src/hooks/useTranslation.ts`; add every key to both
  `src/lib/translations/resources/de/common.de.json` and `en/common.en.json`.
- Pages and components use the project conventions in `AGENTS.md`; presentation is built with MUI and the theme.
- API bindings use `statelessApi` in `src/lib/api`; pages and components do not call Axios or `fetch` directly.
- Browser authentication uses HttpOnly cookies. The UI never reads or stores access or refresh tokens.
- Children's screens use large touch targets and clear icon/text feedback. No tracking, third-party child-data services,
  or external CDNs.

## Current scope

The UI currently includes public landing and account flows, admin and practice shells, device pairing/profile-selection
screens, and reusable child input/learning-aid components. Some corresponding backend API endpoints and exercise flows
are still in development; the screens should not be treated as evidence those server features are available.

## Before a change is ready

```powershell
npm run lint
npm run format:check
npm run build
npm run i18n:check
```
