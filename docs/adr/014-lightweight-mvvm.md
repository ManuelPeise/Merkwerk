# 014 – Lightweight MVVM with CommunityToolkit.Mvvm

- Status: Superseded by [015](015-react-typescript-ui.md) (2026-10-03)
- Date: 2026-10-02
- Ticket: LP-002

## Context

Pages like the exercise editor and the practice player contain real logic that should be testable without a browser.

## Decision

View models with `[ObservableProperty]` and `[RelayCommand]` (CommunityToolkit.Mvvm) for pages with logic; components inherit `MvvmComponentBase<TViewModel>`, which re-renders on `PropertyChanged`. Simple pages remain plain Razor components. View models talk to services (adults) or the typed API client (children), never to repositories.

## Alternatives considered

- MVVM everywhere – boilerplate without benefit on simple pages.
- Logic in code-behind – hard to test.

## Consequences

- View models are tested with xUnit, components with bUnit.
- View models are not reusable in a later Expo app, but describe exactly what it must do.
