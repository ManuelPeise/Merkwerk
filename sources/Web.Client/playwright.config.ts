import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
    testDir: 'e2e',
    testMatch: '**/*.spec.ts',
    timeout: 30_000,
    fullyParallel: false,
    reporter: [['list'], ['html', { open: 'never' }]],
    use: {
        baseURL: 'http://localhost:65350',
        testIdAttribute: 'data-testid',
        trace: 'on-first-retry',
        video: 'retain-on-failure',
        screenshot: 'only-on-failure',
    },
    projects: [
        {
            name: 'chromium-desktop',
            use: { ...devices['Desktop Chrome'] },
        },
        {
            name: 'chromium-pixel-7',
            use: { ...devices['Pixel 7'] },
        },
    ],
    webServer: {
        command: 'npm run dev -- --host 127.0.0.1 --strictPort',
        port: 65350,
        reuseExistingServer: true,
        timeout: 120_000,
    },
});
