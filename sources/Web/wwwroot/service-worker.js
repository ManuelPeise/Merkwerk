// Merkwerk service worker.
// No caching yet - every request goes to the network. Offline support follows with LP-302.
// The fetch handler must really respond: Chrome ignores an empty ("no-op") handler and then
// does not treat the site as an installable app (found in LP-007).
self.addEventListener('install', () => self.skipWaiting());
self.addEventListener('activate', event => event.waitUntil(self.clients.claim()));
self.addEventListener('fetch', event => {
    event.respondWith(fetch(event.request));
});
