# CLAUDE.md – Web.Client

All frontend rules live in AGENTS.md (one source for every AI tool):

@AGENTS.md

## Claude Code specifics

- The repository-wide `CLAUDE.md` / `AGENTS.md` in the repo root are loaded as well; this folder's rules win for frontend topics.
- Before implementing a ticket, present a short plan (routes, pages/components, API functions, translation keys) and wait for approval.
- Check installed versions in `package.json` and follow the patterns already in `src/` instead of guessing APIs.
- After every change run `npm run lint` and `npm run build` in `sources\Web.Client`; don't continue while they fail.
- Add new texts to `de` **and** `en` in the same change.
- Give shell commands in PowerShell syntax (Windows).
- Reply to the developer in German; write code, comments, commits and docs in English.
