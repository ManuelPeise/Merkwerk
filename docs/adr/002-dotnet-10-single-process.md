# 002 – Backend on .NET 10 / ASP.NET Core as a single process

- Status: Accepted
- Date: 2026-10-02
- Ticket: LP-002

## Context

Solo development (10–20 h/week, AI-assisted), existing C# expertise, deployment on a Raspberry Pi.

## Decision

The backend is written in C# on .NET 10 with ASP.NET Core. Blazor UI, API and static files run in **one process** and one container (modular monolith).

## Alternatives considered

- Node.js/NestJS – a second language without benefit for this developer.
- Separate API and UI services – more containers, more memory on the Pi, more deployment work.

## Consequences

- One image, one deployment; low memory footprint.
- Module boundaries are kept by projects and folders, not by network calls.
