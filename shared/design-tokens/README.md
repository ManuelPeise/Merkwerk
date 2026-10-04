# Design tokens

[`tokens.json`](tokens.json) records the project's initial design direction and reference values in W3C Design Tokens
(DTCG)-style JSON. It is a design reference, not currently a generated runtime asset.

## Design direction

LP-008 selected direction **A – calm learning space**: warm off-white surfaces, petrol as the primary colour, white
cards, spacious layouts, and subject colours as accents. The token set also records Andika as the intended typeface for
children's screens and system fonts for adult-facing UI.

## Current UI implementation

The React application's active MUI theme is
[`sources/Web.Client/src/lib/theme/theme.ts`](../../sources/Web.Client/src/lib/theme/theme.ts). Its current palette and
component styles implement the design direction directly. The app does not currently import `tokens.json`, and there is
no generated `tokens.css`, token-to-theme generator, or design-token synchronization test in the repository.

Exception: the subject colors (`color.subject.*`, LP-109) are mirrored as `theme.palette.subject.*`, and
`Architecture.Tests/SubjectPaletteTests` reads `tokens.json` to check that every color of the fixed subject choice exists
and has at least 4.5:1 contrast to white.

When changing design values, check the active React theme and this reference file. Keep them aligned when the reference
values are intended to apply to the web client; do not assume changing `tokens.json` alone changes the application.

## Accessibility and child UI

- Maintain WCAG AA contrast for text and interactive controls.
- Never rely on colour alone to communicate correctness, state, or subject; pair colour with text or an icon.
- Child-facing touch targets should be at least 64 × 64 CSS pixels.
- Fonts and other assets must be self-hosted; do not add external CDNs.
- Keep the token file valid JSON and retain the DTCG-style `$type`, `$value`, and optional `$description` metadata.
