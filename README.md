# Merkwerk

**The open learning platform for practice – for children, parents and teachers.**

Adults create exercises in German, English and maths and assign them to individual children or groups.
Children practise on a tablet and get instant feedback; adults see where help is needed.
Merkwerk is self-hosted – at home on a Raspberry Pi or on a school's own server.
Children's data never leaves your own installation.

> **Status:** in development (phase 0 – foundations). Not ready for production use yet.

## Features (planned for the MVP)

- **Create exercises** with multiple-choice, free-text, cloze, matching and flashcard questions
- **Maths from generators**: basic arithmetic with a configurable number range, times tables – fresh exercises every time
- **Vocabulary from word lists**: enter a word once, practise it in many modes (reading, writing, listening, dictation)
- **German**: mark parts of speech, split and count syllables, reading texts with comprehension questions
- **Learning aids** such as a dot field and a times-table grid – adults see whether a task was solved with or without help
- **Spaced repetition** (Leitner system): what's mastered comes up less often
- **Child-friendly**: large touch targets, read-aloud, password-free sign-in on paired tablets
- **Privacy first**: no tracking, no third parties, minimal data about children

The user interface is in German; more languages may follow.

## Tech stack

| Area | Technology |
| --- | --- |
| Backend | .NET 10, ASP.NET Core (controllers, OpenAPI) |
| Adult UI | Blazor (Interactive Server) |
| Children's UI | Blazor WebAssembly as an installable PWA |
| Database | MySQL 8.4 with Entity Framework Core 10 |
| Authentication | ASP.NET Core Identity + JWT |
| Operations | Docker Compose (Caddy, app, MySQL), runs on amd64 and ARM64 (Raspberry Pi) |

## Repository layout

```
Merkwerk/
├─ sources/
│  ├─ Merkwerk.slnx
│  ├─ 01 Web      Web (host, adults), Web.Client (children, WASM)
│  ├─ 02 Service  Service (API controllers)
│  ├─ 03 Logic    Logic, Logic.Shared (graders, generators)
│  ├─ 04 Data     Data.Database (entities, DbContext, migrations),
│  │              Data.Accessor (repositories, unit of work)
│  ├─ 05 Shared   Shared (DTOs, enums)
│  └─ 06 Tests    *.Tests
├─ shared/        grading-cases (grader test cases), design-tokens
├─ deploy/        docker-compose.yml, Caddyfile
├─ docs/         adr/ (architecture decisions), git.README.md, infrastructure.README.md
├─ .claude/skills/ workflows for AI-assisted development
├─ AGENTS.md      conventions for contributors and AI assistants
└─ CLAUDE.md
```

Details per project, runtime setup and configuration: [docs/infrastructure.README.md](docs/infrastructure.README.md).
The numbers are solution folders in Visual Studio. The rules for which project may reference which are in
[AGENTS.md](AGENTS.md#3-solution-layout-and-dependency-rules) and are checked automatically by an architecture test.

## Getting started (development)

Prerequisites: [.NET 10 SDK](https://dotnet.microsoft.com/download), Docker Desktop, optionally Visual Studio 2026 or Rider.

```powershell
git clone <repo-url> Merkwerk
cd Merkwerk

# Start the database
copy deploy\.env.example deploy\.env      # set your own passwords
docker compose -f deploy/docker-compose.yml up -d db

# Build, create the database, run
cd sources
dotnet build Merkwerk.slnx
dotnet ef database update -p Data.Database -s Web
dotnet run --project Web
```

Then open `https://localhost:<port>/admin` (adults) and `https://localhost:<port>/ueben` (children).

Run the tests:

```powershell
dotnet test sources/Merkwerk.slnx
```

> `deploy/` and the database migrations are created during phases 0/1 – until then not every command works yet.

## Running on a Raspberry Pi

Short version (a full guide will follow in `docs/self-hosting.md`):

1. Raspberry Pi 4/5 with at least 4 GB RAM, **boot from an SSD**, install Docker
2. Copy `deploy/` to the Pi and create `.env`
3. `docker compose pull && docker compose up -d`
4. Install Caddy's root certificate on the tablets once (HTTPS on the home network)
5. Set up a nightly backup with `mysqldump`

## Contributing

Contributions are welcome. Please read [AGENTS.md](AGENTS.md) first – it covers the architecture, conventions
and the definition of done. Please discuss larger changes in an issue before starting.

- Git workflow in detail: [docs/git.README.md](docs/git.README.md)
- Branches: `feature/LP-xxx-short-description`, branched from `Development`
- Commits: `LP-xxx: <what, imperative mood>`
- Pull requests target `Development`

## Privacy

For children, Merkwerk stores only a first name or pseudonym, the school grade and an avatar. There are no
tracking or advertising SDKs and no data is sent to third parties. If you run Merkwerk in a school, you are the
data controller under the GDPR and should agree its use with the responsible authority.

## License

Code: [MIT](LICENSE). Shared exercises and word lists: CC BY-SA 4.0.
