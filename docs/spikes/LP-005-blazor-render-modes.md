# Spike LP-005 – Blazor Server and WebAssembly in one app

Goal: prove that `/admin` runs in **Interactive Server** mode and `/ueben` in **Interactive WebAssembly** mode in
the same Blazor Web App, that the children's area can be installed as a PWA on a tablet, and measure the first load.

Spike code: `Web/Components/Pages/Admin/AdminHome.razor`, `Web.Client/Pages/Ueben/UebenHome.razor`,
`Web/wwwroot/manifest.webmanifest`, `Web/wwwroot/service-worker.js`. Texts are temporary (not localised).

## How to test

### On the PC

```powershell
cd sources
dotnet run --project Web --launch-profile https
```

1. Open `https://localhost:7026/admin` → renderer shows **Server**, clicks count up.
2. Open `https://localhost:7026/ueben` directly (not via a link) → renderer shows **WebAssembly**,
   "Bereit nach … ms" shows the load time, the assembly check is green.
3. DevTools → Network → "Disable cache" → reload `/ueben` → note time and transferred MB (first visit).
4. Reload again without "Disable cache" → note time (cached visit).
5. Address bar → "Install app" icon → Merkwerk opens in its own window at `/ueben`.

### On the tablet (home network)

1. In `deploy/.env` set `DEV_HOST=<IP of the PC>`, e.g. `DEV_HOST=192.168.178.20`.
2. Start the app reachable on the network and the HTTPS proxy:
   ```powershell
   dotnet run --project sources/Web --launch-profile lan
   docker compose -f deploy/docker-compose.yml --profile dev up -d proxy-dev
   ```
   Allow the Windows firewall prompt for private networks.
3. Export the root certificate and install it on the tablet:
   ```powershell
   docker compose -f deploy/docker-compose.yml cp proxy-dev:/data/caddy/pki/authorities/local/root.crt ./merkwerk-dev-root.crt
   ```
   - **iPad:** send the file (AirDrop/mail) → install profile → Settings → General → About → Certificate Trust Settings → enable.
   - **Android:** Settings → Security → Encryption & credentials → Install a certificate → CA certificate.
   - Delete `merkwerk-dev-root.crt` from the repo folder afterwards (never commit it).
4. Open `https://<DEV_HOST>:8443/ueben` → same checks as on the PC.
5. Add to home screen (iPad: Share → "Add to Home Screen"; Android: menu → "Install app") → start from the icon.

## Results

| Check | PC (Chrome/Edge) | iPad (Safari) | Android tablet (Chrome) |
| --- | --- | --- | --- |
| `/admin` renderer = Server | | | |
| `/ueben` renderer = WebAssembly | | | |
| No EF Core / Logic / Data.* loaded | | | |
| First load (no cache): time / MB | | | |
| Cached load: time | | | |
| Installable as app, starts in full screen | | | |
| Tap button reacts immediately | | | |

## Findings and decision

- PC (Chrome/Edge): `/admin` renders as Server, `/ueben` as WebAssembly, no EF Core/Logic/Data.* in the browser, installable as app.
- In Development a cached service worker could make the page hang – it is now only registered outside Development.
- Load times and the tablet test are recorded together with LP-007.
- …
- Decision: keep ADR 003 as is / adjust because …
