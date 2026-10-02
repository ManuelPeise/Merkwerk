# Design tokens

`tokens.json` is the single source for colours, fonts, spacing, sizes and radii of every Merkwerk UI
(Blazor adult area, Blazor WebAssembly children's area, a later Expo app). Format: W3C Design Tokens (DTCG).

## Decision (LP-008, 02.10.2026)

Direction **A – calm learning space** was chosen together with the children, over B (colourful workshop)
and C (learning islands): warm off-white background, petrol as the main colour, white cards, plenty of space,
subject colours only as small accents (icon chips, progress bars). Andika for everything children read;
system fonts for the adult area.

## How tokens reach the UI

`sources/Web.Client/wwwroot/css/tokens.css` holds one CSS custom property per token
(`color.subject.math` → `--color-subject-math`). It is **generated and committed**:

- Generator: `sources/Architecture.Tests/DesignTokenCss.cs` – no Node/Style Dictionary needed.
- `DesignTokenTests` fails when `tokens.css` does not match `tokens.json`, and checks the WCAG AA contrast of
  the colour pairs we rely on.
- Regenerate after changing `tokens.json` (PowerShell, repo root):

  ```powershell
  $env:MERKWERK_UPDATE_TOKENS = "1"; dotnet test sources/Merkwerk.slnx --filter DesignTokenTests; Remove-Item Env:MERKWERK_UPDATE_TOKENS
  ```

`tokens.css` and `fonts.css` (Andika) live in `Web.Client`, whose static files are served at the site root, and
are linked once in `Web/Components/App.razor` – so the adult area and the children's area use the same variables.
A later Expo app gets its own generator output (e.g. `theme.ts`) from the same `tokens.json`.

## Rules

- No hex colours or pixel values in Razor/CSS – always `var(--…)`. (Spike pages from LP-005/LP-007 are exempt
  and will be replaced.)
- Feedback colours are always paired with an icon (colour-blind safe): ✓ for correct, ↺ for "try again".
- Contrast at least WCAG AA: add every new text/background pair to `DesignTokenTests`.
- Children's text: `var(--font-family-kids)`, at least `var(--font-size-kids-small)` (20px); touch targets at least
  `var(--size-touch-target-min)` (64px).
- Age-group themes (e.g. `kids`, `teens`) are added as token sets later, not as separate stylesheets.
