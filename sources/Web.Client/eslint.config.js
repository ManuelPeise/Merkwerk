import js from '@eslint/js';
import globals from 'globals';
import reactHooks from 'eslint-plugin-react-hooks';
import reactRefresh from 'eslint-plugin-react-refresh';
import tseslint from 'typescript-eslint';
import prettier from 'eslint-config-prettier';
import { defineConfig, globalIgnores } from 'eslint/config';

export default defineConfig([
  globalIgnores(['dist', 'node_modules']),
  {
    files: ['**/*.{ts,tsx}'],
    extends: [
      js.configs.recommended,
      tseslint.configs.recommended,
      reactHooks.configs.flat.recommended,
      reactRefresh.configs.vite,
    ],
    languageOptions: {
      globals: globals.browser,
    },
  },
  {
    // Every import inside the app starts at 'src/', never './' or '../'.
    // Texts only through src/hooks/useTranslation, never react-i18next directly.
    files: ['src/**/*.{ts,tsx}'],
    rules: {
      'no-restricted-imports': [
        'error',
        {
          paths: [
            {
              name: 'react-i18next',
              importNames: ['useTranslation'],
              message: "Use useTranslation from 'src/hooks/useTranslation' (getResource).",
            },
          ],
          patterns: [
            {
              regex: '^\\.{1,2}/',
              message: "Use an absolute import from 'src/...' instead of a relative path.",
            },
          ],
        },
      ],
    },
  },
  {
    // The wrapper itself is the one place that may use react-i18next's hook.
    files: ['src/hooks/useTranslation.ts'],
    rules: {
      'no-restricted-imports': [
        'error',
        {
          patterns: [
            {
              regex: '^\\.{1,2}/',
              message: "Use an absolute import from 'src/...' instead of a relative path.",
            },
          ],
        },
      ],
    },
  },
  // Must stay last: turns off every rule that would fight with Prettier.
  prettier,
]);
