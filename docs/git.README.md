# Git guide – Merkwerk

How we work with Git in this repository. Applies to humans and AI assistants alike.
Short rules live in [AGENTS.md](../AGENTS.md#13-git); this file has the details and the commands.

## 1. Branch model

```
Master ────●──────────────────────●──────────►   releases only (tagged vX.Y.Z)
            \                    / merge
Development ─●────●────●────●───●────────────►   integration, always buildable
                 / squash  \ squash
feature/LP-131-…●           ●  feature/LP-142-…
```

| Branch | Purpose | Who writes to it |
| --- | --- | --- |
| `Master` | Released versions. Every commit is a release and carries a tag. | Merge from `Development` only |
| `Development` | Integration. Must always build and pass all tests. | Squash merges of feature/fix branches only |
| `feature/LP-xxx-short-description` | Work on one ticket | You |
| `fix/LP-xxx-short-description` | Bug fix found during development | You |
| `hotfix/vX.Y.Z-short-description` | Urgent fix for a released version | You |

**Every branch is created from `Development`** – features, fixes and hotfixes alike. Nothing is ever branched from `Master`.

Branch names: lower case, words separated by `-`, ticket number first, at most ~50 characters.
Example: `feature/LP-131-arithmetic-generator`.

**Never** commit directly to `Master` or `Development`.

## 2. Daily workflow

```powershell
# 1. Start from an up-to-date Development
git switch Development
git pull --ff-only

# 2. Create the ticket branch
git switch -c feature/LP-131-arithmetic-generator

# 3. Work in small steps; build and test before every commit
dotnet build sources/Merkwerk.slnx
dotnet test  sources/Merkwerk.slnx
git add -p                      # review what you stage, hunk by hunk
git commit -m "LP-131: Add arithmetic generator with number range"

# 4. Keep the branch current (before opening the PR and when Development moved on)
git fetch origin
git rebase origin/Development
#    on conflicts: fix files, then  git add <file>  and  git rebase --continue

# 5. Push (first push sets the upstream)
git push -u origin feature/LP-131-arithmetic-generator
#    after a rebase of an already pushed branch:
git push --force-with-lease
```

Then open a pull request against `Development` (see §5).

## 3. Commit messages

Format:

```
LP-xxx: <what changed, imperative mood, max. ~72 characters>

<optional body: why, not how. Wrap at ~72 characters.>
```

| Good | Bad |
| --- | --- |
| `LP-131: Add arithmetic generator with number range` | `generator` |
| `LP-104: Rotate refresh token on every use` | `fixed stuff` |
| `LP-101: Add migration InitialSchema` | `WIP` |

Rules:

- English, imperative mood ("Add", "Fix", "Remove", not "Added", "Fixes").
- One logical change per commit. Formatting-only changes in their own commit.
- **EF Core migrations always in a commit of their own.**
- Without a ticket (rare, e.g. typo in README): prefix `chore:` – `chore: Fix typo in README`.

## 4. What never goes into Git

- Secrets: `.env`, `appsettings.*.local.json`, certificates, keys. Only `.env.example` with placeholder values.
- Build output (`bin/`, `obj/`), IDE files (`.vs/`, `.idea/`), database dumps, uploaded media.
- Generated files that are rebuilt (e.g. CSS generated from design tokens).
- Large binaries (> 1 MB). Images for the UI are fine if small; ML models etc. are not committed.

The `.gitignore` covers these cases. If you accidentally committed a secret: **rotate the secret immediately**
(change the password/key) – removing it from history alone is not enough.

## 5. Pull requests

1. Target branch: always `Development` (hotfixes too, see §7).
2. Title: `LP-xxx: <ticket title>`.
3. Description: link/summary of the ticket, what changed, how it was tested, what reviewers should look at
   (permissions, tenant isolation, token handling, migrations).
4. Checklist before merging:
   - [ ] CI green (build, tests, architecture test)
   - [ ] Branch rebased on current `Development`
   - [ ] Acceptance criteria of the ticket met
   - [ ] Migration reviewed (if any)
5. Merge with **Squash and merge**. The squash commit message is `LP-xxx: <ticket title>`.
6. Delete the feature branch after the merge.

Solo development: open the PR anyway – it triggers CI and is the place to review the full diff once more.

## 6. Releases and tags

Versioning: [Semantic Versioning](https://semver.org) `MAJOR.MINOR.PATCH`. Until the first stable release: `0.x.y`.

```powershell
git switch Master
git pull --ff-only
git merge --no-ff Development -m "Release v0.3.0"
git tag -a v0.3.0 -m "v0.3.0 – delivery 1a (maths & vocabulary)"
git push origin Master --follow-tags
```

Pushing a tag `v*` makes CI build and publish the multi-arch Docker images.

## 7. Hotfixes

Hotfixes follow the same path as every other change – branched from `Development`, merged into `Development` –
and are then released right away as a patch version.

```powershell
git switch Development
git pull --ff-only
git switch -c hotfix/v0.3.1-fix-login-on-ipad
# fix, test, commit
git push -u origin hotfix/v0.3.1-fix-login-on-ipad
# PR into Development → squash and merge
# then release immediately as a patch version (see §6):
git switch Master
git pull --ff-only
git merge --no-ff Development -m "Release v0.3.1"
git tag -a v0.3.1 -m "v0.3.1 – fix login on iPad"
git push origin Master --follow-tags
```

If `Development` already contains unfinished work that must not be released yet, finish or disable it
(feature flag) before the patch release – do **not** branch from `Master` instead.

## 8. EF Core migrations and conflicts

Two branches that both add a migration will conflict in `MerkwerkDbContextModelSnapshot.cs`.
Do **not** merge the snapshot by hand:

```powershell
# on your feature branch, after rebasing on Development
dotnet ef migrations remove -p sources/Data.Database -s sources/Web    # removes YOUR last migration
dotnet ef migrations add <SameName> -p sources/Data.Database -s sources/Web
```

Never change a migration that is already on `Development` or `Master` – add a new one instead.

## 9. Undoing things

| Situation | Command |
| --- | --- |
| Discard local changes to a file | `git restore <file>` |
| Unstage a file | `git restore --staged <file>` |
| Fix the last commit (not pushed yet) | `git commit --amend` |
| Undo a commit that is already pushed | `git revert <sha>` (creates a new commit) |
| Throw away local commits (not pushed) | `git reset --hard origin/<branch>` – careful, irreversible |
| Find a lost commit | `git reflog` |

Rule of thumb: rewrite history (`amend`, `rebase`, `reset`) **only** on your own, unmerged feature branch.

## 10. Line endings and settings

Line endings use Git's defaults: Git for Windows checks files out with CRLF and stores them normalised
(`core.autocrlf true`, set by the installer). There are no line-ending rules in `.gitattributes`;
it only marks binary files.

Optional one-time settings on Windows:

```powershell
git config --global pull.ff only               # a pull never creates a hidden merge commit
git config --global push.autoSetupRemote true  # first push links the branch to origin
git config --global init.defaultBranch Master
```

## 11. Rules for AI assistants

- Work only on a `feature/` or `fix/` branch; never commit to or push `Master`/`Development`.
- Never use `git push --force` – only `--force-with-lease`, and only on the current feature branch.
- Never rewrite history that has been pushed to a shared branch.
- Don't push or open PRs unless the developer asks for it.
- Show `git status` / `git diff --stat` before committing; stage only files that belong to the ticket.
- Use the commit format from §3; put migrations in their own commit.
- Never stage `.env`, `*.local.json`, certificates or keys – stop and warn if one shows up in `git status`.
