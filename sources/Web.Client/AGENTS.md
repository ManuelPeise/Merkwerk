# AGENTS.md – Web.Client

Working agreement for AI assistants and contributors **inside `sources/Web.Client`** (the React UI).
The repository-wide rules in [`../../AGENTS.md`](../../AGENTS.md) still apply (tickets, Git, privacy, language);
for everything frontend-specific this file wins. `CLAUDE.md` in this folder only imports it.
Setup, commands and short how-tos for humans: [`README.md`](README.md).

## 1. What this is

Single-page app for both audiences of Merkwerk:

- `/admin` – adults (parents, teachers) create exercises and assign them.
- `/ueben` – children (grades 1–4, Android tablets/phones) practise; becomes a PWA later.

It talks **only** to the backend `Web.Core` over `/api/v1`. No server-side rendering, no business rules that the server
does not also enforce – the server is always authoritative (grading, permissions, tenant isolation).

## 2. Tech stack (fixed – change only after asking)

| Area | Choice |
| --- | --- |
| Language | TypeScript 6, `strict`, `verbatimModuleSyntax` |
| Framework | React 19, function components and hooks only |
| Build / dev server | Vite 8 (`@vitejs/plugin-react`), npm, `package-lock.json` is committed |
| UI | MUI (`@mui/material`) with Emotion, theme in `src/lib/theme/theme.ts` |
| Routing | `react-router-dom` (data router, `createBrowserRouter`) |
| HTTP | `axios`, one shared instance in `src/lib/api/apiClient.ts` |
| Translations | `i18next` + `react-i18next` |
| Quality | ESLint (typescript-eslint, react-hooks, react-refresh), Prettier, `scripts/check-translations.mjs` |

**Ask before adding any npm package.** Check the installed major version in `package.json` before using an API
(MUI and react-router change between majors) – don't rely on memory.

## 3. Folder layout

```
src/
  main.tsx                 Entry: loads i18n, renders <App />
  App.tsx                  ThemeProvider, CssBaseline, RouterProvider – nothing else
  navigation/
    routes.ts              All paths as constants (routes.start, routes.admin, routes.practice)
    router.tsx             Route tree
  pages/                   One component per route (StartPage, AdminPage, PracticePage, …)
  components/              Reusable UI, grouped by topic (layout/, practice/, admin/, …)
  hooks/                   Reusable hooks (useXyz.ts)
  lib/
    api/                   apiClient.ts + one folder per backend module (<module>Api.ts, <module>Types.ts)
    theme/                 MUI theme (design direction A, LP-008)
    translations/          i18n.ts, i18next.d.ts, resources/<lang>/<namespace>.<lang>.json
scripts/                   Node scripts used by npm scripts
```

New top-level folders under `src/` only after asking.

## 4. Imports

- **Every import inside the app starts with `src/`** – never `./` or `../` (ESLint `no-restricted-imports`, also for CSS, JSON and assets).
  The alias is defined twice and must stay in sync: `paths` in `tsconfig.app.json` and `resolve.alias` in `vite.config.ts`.
- Types are imported with `import type { … }`.
- Order: external packages first, then `src/…` imports.

## 5. Components and code style

- Function components typed as `FC<Props>`; props type named `<Component>Props`.
- **Never destructure props in the parameter list.** Take `props` and destructure in the first line of the body:

  ```tsx
  // never
  const FormNumberField: React.FC<IProps> = ({ label, value, disabled, onChange }) => {

  // always
  const FormNumberField: React.FC<IProps> = (props) => {
      const { label, value, disabled, onChange } = props;
  ```

- **Named exports**, one component per file, file name = component name in PascalCase (`StartPage.tsx`).
  Only `App.tsx` uses a default export.
- Components render; logic (data loading, state machines, grading) lives in hooks or `src/lib`.
- No `any`. No non-null assertions except the root element in `main.tsx`.
- No `console.log` in committed code.
- Formatting is Prettier's job (`.prettierrc.json`); indentation comes from the root `.editorconfig`
  (4 spaces for `ts`/`tsx`, 2 for `json`/`js`/`html`/`css`). Line endings: Git default (CRLF on Windows).

## 6. Styling and UI

- Use MUI components and the theme. Colours, spacing, radii and fonts only via the theme
  (`sx={{ color: 'primary.main', p: 2 }}`) – **no hex colours outside `theme.ts`**, no new CSS files.
- Fonts are self-hosted (Andika); **no font or script CDNs**.
- Children's area (`/ueben`):
  - touch targets ≥ 64×64 px;
  - little text, icons plus read-aloud;
  - feedback never by colour alone (always an icon as well);
  - mistakes are friendly ("Probier es nochmal"), never harsh red;
  - number input via the on-screen keypad.
- Accessibility: WCAG AA contrast, everything reachable by keyboard, `aria-label` on icon buttons (text from translations).

## 7. Translations (i18n)

- **No hard-coded UI strings** – every visible text, `aria-label` and notification comes from
  `getResource(...)` of **`useTranslation` in `src/hooks/useTranslation.ts`**. Never import `useTranslation`
  from `react-i18next` (ESLint error); `toggleLanguage(...)` switches the language and remembers it on the device.
- Resources: `src/lib/translations/resources/<lang>/<namespace>.<lang>.json` (`de`, `en`). German is the source language.
- Keys are flat camelCase and start with a prefix:

  | Prefix | Used for | Example |
  | --- | --- | --- |
  | `caption` | headings, titles, captions | `captionStartPage` |
  | `label` | buttons, links, form labels, `aria-label` | `labelBackToStart` |
  | `notification` | snackbars, alerts, error and success messages | `notificationNetworkError` |

- Every key exists in **every** language; `npm run i18n:check` enforces prefixes and parity.
- Keys are typed (`i18next.d.ts`, `TranslationKey`): a typo in `getResource('…')` is a compile error.
- New namespace (e.g. `admin`, `practice`): add the files for every language, register them in `i18n.ts` and `i18next.d.ts`.

## 8. Routing

- Paths only from `src/navigation/routes.ts`, never as string literals in components.
- Navigate with `<Link>` / `useNavigate`; MUI buttons via `component={Link}`.
- Every route belongs to exactly one group in `src/navigation/router.tsx`:

  | Group | Guard | Example |
  | --- | --- | --- |
  | Everyone | none | landing page |
  | Only when **not** signed in | `<PublicRoute />` – signed-in users are sent on | login |
  | Only when signed in | `<ProtectedRoute />`, optionally `roles={['…']}` – others go to the login and come back afterwards | `/admin` |

- The guards only improve the UI – **the server's `[Authorize]` is what protects data.**
- Auth state only via `useAuthentication()` (`status`, `isAuthenticated`, `user`, `login`, `logout`) from
  `AuthenticationProvider` (`src/components/providers`); never call the authentication endpoints from pages.
- Navigation entries (`navigationItems.ts`) carry a `visibility` that must match their route group.
- Unknown paths redirect to `routes.start`.

## 9. API access

- Components and hooks call the backend **only through `statelessApi`** (`src/lib/api/StatelessApi.ts`).
  It sits on top of `apiClient` (axios, base URL `/api/v1`, `withCredentials`). **No `fetch`, no second axios instance,
  no direct `apiClient` calls outside `src/lib/api`.**
- Per backend module one file `src/lib/api/<module>/<module>Api.ts` that binds the endpoints with
  `statelessApi.create<TResponse, TRequest>({ serviceUrl })`, plus its DTO types mirroring the C# DTOs.
  Components never build URLs themselves.
- Requests **never throw**: they resolve to `{ data }` or `{ error }`. `error.messageKey` is a `notification…` key –
  show `t(error.messageKey)`, never the raw server message. Ignore `error.kind === 'canceled'`.
- Pass an `AbortSignal` (`signal`) from `useEffect` cleanups so unmounted components don't process late responses.
- Auth uses HttpOnly cookies (`mw_access`, `mw_refresh`). The client **never reads, stores or sends tokens itself**;
  `apiClient` refreshes once on 401 (single flight).
- In development Vite proxies `/api` to `http://localhost:5138` (Web.Core, launch profile `http`). Dev runs over HTTP.

## 10. Privacy and security (non-negotiable)

- No tracking, analytics, advertising or error-reporting SDKs.
- No children's data to third parties (no external speech, translation or AI services).
- Don't put personal data in `localStorage`, URLs or logs. Only UI preferences may go to `localStorage`.
- No secrets in the client – everything in the bundle is public.

## 11. Commands

```powershell
cd sources\Web.Client
npm install            # after pulling changes to package.json
npm run dev            # http://localhost:65350 (backend must run on :5138)
npm run build          # type check (tsc -b) + production build to dist/
npm run lint           # ESLint + translation check
npm run lint:fix
npm run format         # Prettier write; format:check for CI
npm run i18n:check
```

## 12. Definition of Done (frontend)

1. Acceptance criteria of the ticket are met.
2. `npm run build` without errors, `npm run lint` without errors or warnings, `npm run format:check` clean.
3. All new texts in `de` and `en`, `npm run i18n:check` green.
4. Imports only via `src/…`.
5. Children's screens checked on a phone/tablet (touch size, read-aloud, no colour-only feedback).
6. Tests (Vitest, planned): once present, new logic in hooks/lib has tests and all tests are green.
