# Spike LP-007 – Drag & drop and read-aloud on real devices

Goal: prove that children can drag cards with a finger (SortableJS via JS interop) and that German and English
text can be read aloud with the Web Speech API on the devices the family uses. Record known limitations
(voice selection, delay) and the load times left open in LP-005.

Pilot devices: **Android only** (smartphone and tablet, Chrome). iPad/Safari is not tested in this spike –
there is no iOS device in the pilot; revisit before schools use Merkwerk.

Spike code:

- `Web.Client/Pages/Ueben/TouchSpeechSpike.razor` – test page `/ueben/spike` (temporary texts and colours)
- `Web.Client/Services/DragDrop/` + `wwwroot/js/dragdrop.js` – SortableJS wrapper
- `Web.Client/Services/Speech/` + `wwwroot/js/speech.js` – Web Speech API wrapper
- `Web.Client/wwwroot/lib/sortablejs/` – vendored SortableJS 1.15.6 (MIT), no CDN

Design note: SortableJS moves DOM nodes, but Blazor owns the DOM. `dragdrop.js` therefore undoes every move
immediately and reports it to .NET; the component updates its model and Blazor re-renders (cards need `@key`).

## How to test

### Quick check without certificate (HTTP)

Drag & drop and read-aloud also work over plain HTTP, so the first round needs no certificate:

```powershell
cd D:\WorkBench\Merkwerk
dotnet run --project sources/Web --launch-profile lan
```

Allow the Windows firewall prompt for **private** networks. Find the PC's IP with `ipconfig` (IPv4 address).
On the phone/tablet (same Wi-Fi) open `http://<PC-IP>:5138/ueben/spike`.

### Installing the app (PWA) on Android during development

Android Chrome does **not** install the app from `https://<PC-IP>:8443` with Caddy's self-signed root certificate,
even when that certificate is installed on the device (it only offers "create shortcut"). What works:

1. Run with the `lan` launch profile – it sets `Pwa__EnableServiceWorker=true`, so the service worker is registered
   in Development too (it is off for the other profiles, see `App.razor`).
2. On the device open `chrome://flags/#unsafely-treat-insecure-origin-as-secure`, enter `http://<PC-IP>:5138`,
   set it to *Enabled* and relaunch Chrome.
3. Open `http://<PC-IP>:5138/ueben` → menu ⋮ → *Install app*.

### HTTPS via proxy-dev (optional)

Same as LP-005 "On the tablet": `DEV_HOST=<PC-IP>` in `deploy/.env`, start `proxy-dev`, install the root
certificate on the device (Android: Settings → Security → Encryption & credentials → Install a certificate →
CA certificate), open `https://<PC-IP>:8443/ueben/spike`.

### Troubleshooting (found in LP-007)

| Symptom | Cause | Fix |
| --- | --- | --- |
| Device: "site can't be reached" | Windows network is *Public*, inbound blocked | Admin PowerShell: `Set-NetConnectionProfile -InterfaceAlias "WLAN" -NetworkCategory Private` and `New-NetFirewallRule -DisplayName "Merkwerk dev" -Direction Inbound -Protocol TCP -LocalPort 5138,8443 -Action Allow -Profile Private` |
| `https://<PC-IP>:8443` shows a blank "not secure" page / certificate name mismatch | `DEV_HOST` missing in `deploy/.env` (files created before LP-005), proxy runs as `localhost` | Add `DEV_HOST=<PC-IP>`, then `docker compose -f deploy/docker-compose.yml --profile dev up -d --force-recreate proxy-dev`; check with `... exec proxy-dev printenv DEV_HOST` |
| Chrome only offers "create shortcut" | Service worker not registered (not `lan` profile) or self-signed HTTPS (see above) | Use the `lan` profile and the Chrome flag over HTTP |
| Not installable although service worker is registered | Empty ("no-op") fetch handler is ignored by Chrome | Fixed: `service-worker.js` now responds with `fetch(event.request)` |

### Checks

1. **Match:** drag `dog`, `cat`, `house` onto *Hund*, *Katze*, *Haus* → each zone turns green with ✓.
   Drag one into a wrong zone → orange with ↺. Drag cards back into the pool.
2. **Scroll vs. drag:** swipe quickly over a card → the page scrolls; rest the finger briefly → the card lifts.
3. **Tap vs. drag:** tap a card → it is read aloud. After a drag, check whether it is *also* read aloud (it should not be).
4. **Order:** drag the syllables into *Schmet – ter – ling* → ✓.
5. **German / English:** choose a voice, press 🔊 → note the voice, "Start nach … ms" and whether it sounds natural.
   Try tempo 0.7 / 0.85 / 1.0. Press ■ while it speaks.
6. **First call:** reload the page and press 🔊 immediately → does the first utterance start, and how late?
7. **Offline voice:** switch on flight mode (page stays open) → do the voices still work?
8. **App mode (HTTPS only):** start the installed app → repeat 1 and 5.
9. **Load times (from LP-005):** open `/ueben` → note "Bereit nach … ms" on the first visit and after a reload.

## Results

| Check | Android smartphone (Chrome) | Android tablet (Chrome) |
| --- | --- | --- |
| Device (model, Android version, screen from the page) | | |
| 1 Match by finger | ✓ | |
| 2 Scroll still works | ✓ | |
| 3 Tap reads aloud, no read-aloud after drag | ✓ (tap reads, drag does not) | |
| 4 Order by finger | ✓ | |
| 5 German voices (count, best voice) | ✓ sounds good (count not noted) | |
| 5 English voices (count, en-GB available?) | ✓ sounds good (count not noted) | |
| 5 Start delay (ms) German / English | no noticeable delay (not measured) | |
| 5 Stop works | | |
| 6 First utterance after reload | | |
| 7 Works offline | | |
| 8 Works as installed app | | |
| 9 `/ueben` first load / cached (ms) | | |

## Findings and decision

- **Android smartphone (Chrome, HTTP on the home network, 02.10.2026):** matching and ordering by finger work,
  the page still scrolls, a tap reads the word aloud and a drag does not trigger read-aloud. German and English
  read-aloud sound good with no noticeable delay. No numbers recorded.
- The Blazor pattern (SortableJS move undone in JS, model updated in .NET, re-render with `@key`) works without
  DOM glitches.
- **Android smartphone, installed app (02.10.2026):** the app is installable over HTTP with the Chrome flag;
  Chrome on the PC shows no installability errors (only the hint to add screenshots for the richer install UI).
- **Not installable with the self-signed certificate:** over `https://<PC-IP>:8443` (Caddy `tls internal`, root
  certificate installed on the phone) Android Chrome only offers a shortcut. Consequence for the Pi (LP-160): the
  Caddy root certificate on the tablets is not enough for installing the app – we need a host name with a
  certificate Android trusts out of the box (e.g. own domain + Let's Encrypt via DNS challenge). Decide in LP-160.
- **Still open:** load times of `/ueben` (first start / cached), read-aloud in flight mode, and the check on the
  children's tablet.
- Known limitations to keep in mind: voices load asynchronously (list can be empty for a moment); which voices
  exist depends on the device's speech engine (Android: Settings → System → Languages → Text-to-speech output);
  "(online)" voices need the internet and would send text to the speech provider – Merkwerk must prefer local voices.
- Decision: keep SortableJS + Web Speech API (AGENTS.md §7) / adjust because …
