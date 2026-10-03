---
applyTo: "sources/Web.Client/**"
description: "Rules for the React/TypeScript UI (Web.Client)"
---

# Web.Client (React UI)

Authoritative: [`sources/Web.Client/AGENTS.md`](../../sources/Web.Client/AGENTS.md). Summary:

## Code style

- Imports **always** start with `src/` – never `./` or `../` (ESLint error). Types via `import type`.
- Components: one per file, PascalCase file name, `React.FC<IProps>`, props interface named `IProps`, **`export default`**.
  Everything else (hooks, functions, constants, types, contexts) uses **named exports**.
- **Never destructure props in the parameter list** – take `props` and destructure in the first line:

  ```tsx
  interface IProps {
      label: string;
      onChange: (value: string) => void;
  }

  const MyField: React.FC<IProps> = (props) => {
      const { label, onChange } = props;
      // …
  };

  export default MyField;
  ```

- A component file exports only its component (react-refresh). Contexts live in their own `.ts` file in `components/providers`.
- File names: components/classes PascalCase (`LoginPage.tsx`), everything else camelCase (`apiClient.ts`, `useForm.ts`).
- No `any`, no `console.log`, no non-null assertions (except the root element in `main.tsx`).
- Components render; logic lives in hooks (`src/hooks`) or `src/lib`.

## UI and styling

- MUI only, styled through the theme (`src/lib/theme/theme.ts`). No hex colours outside the theme, no new CSS files.
- **MUI 9 has no system props**: `<Stack alignItems="center">` or `<Box mt={2}>` do not compile – use `sx`.
- Form fields: use the existing components in `src/components/input` (`FormTextField`, `FormPasswordField`, `FormNumberField`,
  `FormPinField`, `FormCheckbox`, `FormButton`, `FormLink`) – they share `FormFieldContainer` (label above, 64 px inputs).
  Spacing between fields comes from the form (`<Stack spacing={2}>`).
- Icons only via `src/components/icons/AppIcons.tsx` (the only file importing `@mui/icons-material`).
- Children's area (`/practice`): touch targets ≥ 64×64 px, icons plus text, feedback never by colour alone, friendly
  wording, number input via `NumberKeypad`.
- Accessibility: WCAG AA, keyboard reachable, `aria-label` on icon buttons (text from translations).

## Texts (i18n)

- No hard-coded UI strings. Use `const { getResource } = useTranslation()` from **`src/hooks/useTranslation`** –
  never `useTranslation` from `react-i18next` (ESLint error).
- Keys are flat camelCase with a prefix: `caption…` (headings), `label…` (buttons, links, form labels, aria-labels),
  `notification…` (alerts, errors, success). Add every key to **both** `resources/de/common.de.json` and `resources/en/common.en.json`.

## Routing and auth

- Paths only from `src/navigation/routes.ts`. Every route belongs to one layout (`PublicLayout`, `AdminLayout`, `KidsLayout`)
  and one guard group in `router.tsx`: everyone, `PublicRoute` (only signed out), `ProtectedRoute` (signed in, optional
  `roles` from `src/lib/auth/roles.ts`), `DeviceRoute` (paired device for `/practice/*`).
- Auth state only via `useAuthentication()`; setup state via `SetupGate`/`useSetupCompletion`.

## API access

- Only through `statelessApi` (`src/lib/api/StatelessApi.ts`): one folder per backend module with `<module>Api.ts`
  (`statelessApi.create<TResponse, TRequest>({ serviceUrl: '/module/action' })`) and `<module>Types.ts` mirroring the C# DTOs.
- No `fetch`, no second axios instance, no direct `apiClient` calls outside `src/lib/api`. Paths are lowercase kebab-case.
- Requests never throw: they resolve to `{ data }` or `{ error }`. Show `getResource(error.messageKey)`, never raw server
  messages; ignore `error.kind === 'canceled'`. Field errors from `ProblemDetails` via `getFieldErrors`.
- Pass an `AbortSignal` from `useEffect` cleanups.

## Before you finish

`npm run lint`, `npm run build`, `npm run i18n:check` (in `sources/Web.Client`) must be green.
