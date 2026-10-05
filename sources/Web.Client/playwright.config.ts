import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
    testDir: 'e2e',
    // Sets up the instance once (first-run setup with the E2E owner) before any worker starts.
    globalSetup: './e2e/support/globalSetup.ts',
    testMatch: '**/*.spec.ts',
    timeout: 30_000,
    fullyParallel: false,
    reporter: [['list'], ['html', { open: 'never' }]],
    use: {
        baseURL: 'http://localhost:65350',
        testIdAttribute: 'data-testid',
        // The tests use the German texts (e2e/support/i18n.ts); the UI follows the browser language.
        locale: 'de-DE',
        trace: 'on-first-retry',
        video: 'retain-on-failure',
        screenshot: 'only-on-failure',
    },
    projects: [
        {
            name: 'chromium-desktop',
            use: { ...devices['Desktop Chrome'], locale: 'de-DE' },
        },
        {
            name: 'chromium-pixel-7',
            use: { ...devices['Pixel 7'], locale: 'de-DE' },
        },
    ],
    webServer: {
        command: 'npm run dev -- --host 127.0.0.1 --strictPort',
        port: 65350,
        reuseExistingServer: true,
        timeout: 120_000,
    },
});
