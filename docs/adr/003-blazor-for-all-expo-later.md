# 003 – Blazor for all users: adults in Server mode, children in WebAssembly mode as PWA; Expo app later

- Status: Accepted
- Date: 2026-10-02
- Ticket: LP-002

## Context

Children practise on tablets and need instant feedback; adults work on laptops with forms. A native app is not required yet. One language for the MVP keeps a solo project fast.

## Decision

One Blazor Web App. Adult area (`/admin`) uses **Interactive Server**, children's area (`/ueben`) uses **Interactive WebAssembly** and is installable as a PWA. A native Expo (React Native) app is only built in a later stage if the PWA proves insufficient.

## Alternatives considered

- React web + Expo – two UI stacks and a second language in the MVP.
- Blazor Server for children as well – feedback depends on the network round trip; no offline option.
- Expo for children from the start – UI built twice if the web version is also needed.

## Consequences

- The MVP is C# only.
- Graders run in the browser via WebAssembly (shared `Logic.Shared`).
- Risks to check in the spike: first load size of the WASM client, touch drag & drop, JWT handling with Blazor Server.
