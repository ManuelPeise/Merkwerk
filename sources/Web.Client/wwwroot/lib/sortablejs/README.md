# SortableJS (vendored)

- Version: 1.15.6
- Source: https://github.com/SortableJS/Sortable (npm package `sortablejs`, file `modular/sortable.esm.js`)
- License: MIT, see `LICENSE` in this folder

Vendored instead of loaded from a CDN: the children's client must not use external CDNs (AGENTS.md §9).
Only used through `js/dragdrop.js` and `Services/DragDrop/DragDropService.cs`.

Update: download both files of the new version and change the version above.

```powershell
$dir = "sources/Web.Client/wwwroot/lib/sortablejs"
Invoke-WebRequest https://cdn.jsdelivr.net/npm/sortablejs@1.15.6/modular/sortable.esm.js -OutFile "$dir/sortable.esm.js"
Invoke-WebRequest https://cdn.jsdelivr.net/npm/sortablejs@1.15.6/LICENSE -OutFile "$dir/LICENSE"
```
