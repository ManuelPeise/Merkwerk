# Andika (self-hosted)

- Font: Andika by SIL International, designed for beginning readers (clear a/ɑ, l/I, distinct b/d/p/q)
- License: SIL Open Font License 1.1, see `LICENSE` in this folder
- Files: Latin subset (covers ä, ö, ü, ß), weights 400 and 700, woff2 – via the Fontsource npm package `@fontsource/andika`

Self-hosted instead of Google Fonts: the children's client must not load anything from third parties (AGENTS.md §9).

Download / update (PowerShell, from the repo root):

```powershell
$dir = "sources/Web.Client/wwwroot/fonts/andika"
$base = "https://cdn.jsdelivr.net/npm/@fontsource/andika@5"
Invoke-WebRequest "$base/files/andika-latin-400-normal.woff2" -OutFile "$dir/andika-latin-400-normal.woff2"
Invoke-WebRequest "$base/files/andika-latin-700-normal.woff2" -OutFile "$dir/andika-latin-700-normal.woff2"
Invoke-WebRequest "$base/LICENSE" -OutFile "$dir/LICENSE"
```
