---
description: "Review the current changes against the Merkwerk rules"
---

Review the current changes (open files, selection or `git diff Development...HEAD`) against
[AGENTS.md](../../AGENTS.md), [sources/Web.Client/AGENTS.md](../../sources/Web.Client/AGENTS.md) and
`.github/instructions/*.instructions.md`.

Check in this order and report in German, most severe first, each with file, line and a concrete fix:

1. **Bugs**: wrong logic, race conditions, missing error/loading states, effects without cleanup, unhandled API errors.
2. **Security/privacy**: secrets, tokens or personal data in code, logs, URLs or `localStorage`; missing `[Authorize]` or
   tenant check; children's data leaving the server.
3. **Architecture**: business logic in controllers or components, `Logic.*` using ASP.NET Core or the DbContext,
   API calls outside `statelessApi`, entities instead of DTOs.
4. **Conventions**: `src/…` imports, `React.FC<IProps>` + props destructured in the body, `export default` for components,
   no MUI system props, texts via `getResource` with keys in `de` and `en`, paths from `routes.ts`, C# naming.
5. **Clean-up**: unused files, exports, translation keys, icons; stale comments; AGENTS.md out of date.

Do not change code during the review – list findings only and ask which to fix.
