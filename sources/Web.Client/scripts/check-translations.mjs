// Checks the translation resources in src/lib/translations/resources/<lang>/<namespace>.<lang>.json:
// - every key starts with 'caption', 'label' or 'notification'
// - every language has the same namespaces and the same keys
import { readdirSync, readFileSync } from 'node:fs';
import { join } from 'node:path';
import { fileURLToPath } from 'node:url';

const root = fileURLToPath(new URL('../src/lib/translations/resources', import.meta.url));
const keyPattern = /^(caption|label|notification)[A-Z0-9]/;
const errors = [];

const flatten = (value, prefix = '') =>
  Object.entries(value).flatMap(([key, child]) => {
    const path = prefix ? `${prefix}.${key}` : key;
    return child !== null && typeof child === 'object' ? flatten(child, path) : [path];
  });

const languages = readdirSync(root, { withFileTypes: true })
  .filter((entry) => entry.isDirectory())
  .map((entry) => entry.name);

/** @type {Map<string, Map<string, Set<string>>>} namespace -> language -> keys */
const namespaces = new Map();

for (const language of languages) {
  for (const file of readdirSync(join(root, language))) {
    const match = file.match(/^(.+)\.([a-z]{2})\.json$/);
    if (!match || match[2] !== language) {
      errors.push(`${language}/${file}: expected the name <namespace>.${language}.json`);
      continue;
    }

    const keys = flatten(JSON.parse(readFileSync(join(root, language, file), 'utf8')));
    for (const key of keys) {
      const leaf = key.split('.').at(-1);
      if (!keyPattern.test(leaf)) {
        errors.push(`${language}/${file}: "${key}" must start with caption, label or notification`);
      }
    }

    const perLanguage = namespaces.get(match[1]) ?? new Map();
    perLanguage.set(language, new Set(keys));
    namespaces.set(match[1], perLanguage);
  }
}

for (const [namespace, perLanguage] of namespaces) {
  const allKeys = new Set([...perLanguage.values()].flatMap((keys) => [...keys]));
  for (const language of languages) {
    const keys = perLanguage.get(language);
    if (!keys) {
      errors.push(`${language}: namespace "${namespace}" is missing`);
      continue;
    }
    for (const key of allKeys) {
      if (!keys.has(key)) errors.push(`${language}/${namespace}: "${key}" is missing`);
    }
  }
}

if (errors.length > 0) {
  console.error(`Translation check failed:\n  ${errors.join('\n  ')}`);
  process.exit(1);
}

console.log(`Translations ok: ${languages.join(', ')} / ${[...namespaces.keys()].join(', ')}`);
