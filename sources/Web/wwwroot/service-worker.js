// Merkwerk service worker.
// LP-005 spike: only makes the app installable. No caching yet - every request goes to the network.
// Offline support follows with LP-302.
self.addEventListener('install', () => self.skipWaiting());
self.addEventListener('activate', event => event.waitUntil(self.clients.claim()));
self.addEventListener('fetch', () => {
    // network only
});
