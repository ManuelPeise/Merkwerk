---
description: "Add a page to Web.Client (route, layout, guard, texts)"
---

Add the page **${input:page:MyPage}** to `sources/Web.Client` following
[sources/Web.Client/AGENTS.md](../../sources/Web.Client/AGENTS.md):

1. `src/pages/<name>Page/<Name>Page.tsx` (or `src/pages/authentication/<name>Page/` for auth pages): `React.FC`,
   `export default`, texts via `getResource`.
2. Path constant in `src/navigation/routes.ts`; route in `src/navigation/router.tsx` under the right layout
   (`PublicLayout` / `AdminLayout` / `KidsLayout`) and guard (`PublicRoute` / `ProtectedRoute roles=[…]` / `DeviceRoute`).
3. Drawer entry in `navigationItems.ts` only for pages in the parents' area.
4. API calls via a `src/lib/api/<module>/<module>Api.ts` + `<module>Types.ts` (statelessApi).
5. New translation keys (`caption…`, `label…`, `notification…`) in `de` **and** `en`.
6. Loading, error and empty states; mobile layout (≤ 600 px) checked; `npm run lint`, `npm run build`, `npm run i18n:check`.
