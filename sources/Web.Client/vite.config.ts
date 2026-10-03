import { fileURLToPath } from 'node:url';
import { defineConfig } from 'vite';
import plugin from '@vitejs/plugin-react';

// https://vitejs.dev/config/
export default defineConfig({
    plugins: [plugin()],
    resolve: {
        // 'src/...' imports, same mapping as "paths" in tsconfig.app.json.
        alias: {
            src: fileURLToPath(new URL('./src', import.meta.url)),
        },
    },
    server: {
        port: 65350,
        proxy: {
            // Web.Core (launch profile "http"). Same origin for the browser, so the auth cookies just work.
            '/api': 'http://localhost:5138',
        },
    },
});
