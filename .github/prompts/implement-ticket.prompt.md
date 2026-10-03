---
description: "Implement a Merkwerk ticket (LP-xxx) following AGENTS.md"
---

Implement ticket **${input:ticket:LP-xxx}**.

1. Read the ticket in the planning folder (the developer pastes or attaches it) and the rules in
   [AGENTS.md](../../AGENTS.md), [sources/Web.Client/AGENTS.md](../../sources/Web.Client/AGENTS.md) (UI) and the matching
   `.github/instructions/*.instructions.md`.
2. Look at the affected files and existing patterns first. Then present a **short plan** (projects, files, API contract,
   translation keys, tests) and wait for approval – in German.
3. Implement only what the ticket asks for. No new packages, projects or folders without asking; never delete or move files.
4. Check: backend `dotnet build` + `dotnet test`; UI `npm run lint`, `npm run build`, `npm run i18n:check`.
5. Finish with a summary in German: changed files, tests, acceptance criteria met / open, suggested commit message
   `LP-xxx: …`.
