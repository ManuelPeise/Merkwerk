# Design tokens

`tokens.json` is the single source for colours, fonts, spacing and radii of every Merkwerk UI
(Blazor adult area, Blazor WebAssembly children's area, a later Expo app). Format: W3C Design Tokens (DTCG).

**Status: draft.** The values are placeholders until the design direction is decided (ticket LP-008).

## How tokens reach the UI

A build step (Style Dictionary, planned in LP-008) generates

- `wwwroot/css/tokens.generated.css` with CSS custom properties (`--color-subject-math`, `--space-m`, …) for
  `Web` and `Web.Client`,
- later a `theme.ts` for the Expo app.

Generated files are not committed (see `.gitignore`).

## Rules

- No hex colours or pixel values in Razor/CSS – always `var(--…)`.
- Feedback colours are always paired with an icon (colour-blind safe).
- Contrast at least WCAG AA; check every new colour pair.
- Age-group themes (e.g. `kids`, `teens`) are added as token sets later, not as separate stylesheets.
