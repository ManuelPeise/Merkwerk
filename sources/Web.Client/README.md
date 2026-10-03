# Web.Client

React/TypeScript UI of Merkwerk: the parents' area (`/admin`) and the children's practice area (`/ueben`).
It talks only to the backend **Web.Core** over `/api/v1`.

Conventions for contributors and AI assistants: [`AGENTS.md`](AGENTS.md).

## Tech stack

React 19 · TypeScript 6 · Vite 8 · MUI · react-router-dom · axios · i18next · ESLint · Prettier · npm

## Prerequisites

- Node.js (current LTS) and npm
- .NET 10 SDK for the backend (`sources/Web.Core`)
- User secret `Auth:Jwt:SigningKey` (at least 32 characters) for Web.Core, see the root README

## Getting started

```powershell
# 1. Backend (terminal 1)
cd sources
dotnet run --project Web.Core --launch-profile http     # http://localhost:5138, Swagger at /swagger

# 2. Frontend (terminal 2)
cd sources\Web.Client
npm install
npm run dev                                             # http://localhost:65350
```

Vite proxies every request to `/api` to `http://localhost:5138`, so the browser sees a single origin and the auth
cookies work without CORS. Development runs over plain HTTP.

**Visual Studio:** set _Multiple startup projects_ → `Web.Core` and `Web.Client` (`Web.Client.esproj` runs `npm run dev`).

**Phone or tablet on the LAN:** `npm run dev -- --host`, then open `http://<your-PC-IP>:65350`
(Windows network profile "Private", firewall rule for port 65350).

## Scripts

| Command              | What it does                                                  |
| -------------------- | ------------------------------------------------------------- |
| `npm run dev`        | Dev server with hot reload                                    |
| `npm run build`      | Type check (`tsc -b`) and production build to `dist/`         |
| `npm run preview`    | Serves the production build locally                           |
| `npm run lint`       | ESLint + translation check                                    |
| `npm run lint:fix`   | ESLint with auto-fix                                          |
| `npm run format`     | Formats everything with Prettier (`format:check` only checks) |
| `npm run i18n:check` | Key prefixes and identical keys in `de` and `en`              |

## Project structure

```
src/
  main.tsx, App.tsx      Entry point, theme, router
  navigation/            routes.ts (all paths), router.tsx (route tree)
  pages/                 One component per route
  components/            Reusable UI (layout/, …)
  hooks/                 Reusable hooks
  lib/api/               apiClient (axios) + one folder per backend module
  lib/theme/             MUI theme (design direction A)
  lib/translations/      i18n setup + resources/<lang>/<namespace>.<lang>.json
scripts/                 check-translations.mjs
```

## Conventions in short

- Imports always start with `src/` – never `./` or `../` (ESLint error).
- No hard-coded texts: every string comes from `t('…')`. Keys start with `caption`, `label` or `notification`
  and exist in `de` **and** `en`.
- Colours, spacing and fonts only through the MUI theme; no CDNs, no tracking.
- Paths only from `routes.ts`; API calls only through `apiClient`.

## How to …

**… add a page**

1. Create `src/pages/myPage/MyPage.tsx` (`export default`, `React.FC<IProps>`, props destructured in the body).
2. Add the path to `src/navigation/routes.ts` and the route to `src/navigation/router.tsx`.
3. Add its texts to `common.de.json` and `common.en.json`.

**… add a text**

```json
// src/lib/translations/resources/de/common.de.json
"captionMyPage": "Meine Seite"
```

Add the same key to `common.en.json`, then use it: `const { t } = useTranslation(); t('captionMyPage')`.

**… call the backend**

```ts
// src/lib/api/exercises/exercisesApi.ts
import { apiClient } from 'src/lib/api/apiClient';
import type { ExerciseDto } from 'src/lib/api/exercises/exercisesTypes';

export const getExercises = async (): Promise<ExerciseDto[]> =>
    (await apiClient.get<ExerciseDto[]>('/exercises')).data;
```

Authentication is handled by HttpOnly cookies and the 401 refresh in `apiClient` – no token handling in components.

## Before you commit

```powershell
npm run lint; npm run format:check; npm run build
```
