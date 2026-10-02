// LP-006 spike: login/logout from the start page and background refresh of the JWT cookie.
// The access token lives only 1 minute in Development (appsettings.Development.json) so expiry is easy to observe.
// In production the refresh runs at about 80 % of the token lifetime.
(function () {
    const refreshIntervalMs = 40 * 1000;

    async function post(url, body) {
        return fetch(url, {
            method: 'POST',
            credentials: 'same-origin',
            headers: body ? { 'Content-Type': 'application/json' } : {},
            body: body ? JSON.stringify(body) : undefined
        });
    }

    async function refresh() {
        const response = await post('/api/v1/auth/refresh');
        console.info('[auth] refresh ->', response.status);
        return response.ok;
    }

    window.merkwerkAuth = {
        async login() {
            const email = document.getElementById('login-email').value;
            const password = document.getElementById('login-password').value;
            const response = await post('/api/v1/auth/login', { email, password });
            document.getElementById('login-result').textContent =
                response.ok ? 'Angemeldet.' : 'Anmeldung fehlgeschlagen (' + response.status + ').';
        },
        async logout() {
            await post('/api/v1/auth/logout');
            document.getElementById('login-result').textContent = 'Abgemeldet.';
        },
        refresh
    };

    // Keep the cookie fresh while any page is open, so page reloads and Blazor circuit reconnects stay authenticated.
    setInterval(() => { refresh().catch(() => { }); }, refreshIntervalMs);
})();
