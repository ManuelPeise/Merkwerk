import { readdirSync, readFileSync, statSync } from 'node:fs';
import { join, relative } from 'node:path';

const projectRoot = process.cwd();
const sourceRoot = join(projectRoot, 'src');
const kebabCaseRegex = /^[a-z0-9]+(?:-[a-z0-9]+)*$/;
const uiTestIdCallRegex = /\buiTestId\(\s*(['"])([^'"]+)\1\s*\)/g;

const collectFiles = (directory) => {
    const entries = readdirSync(directory);
    const files = [];

    for (const entry of entries) {
        const fullPath = join(directory, entry);
        const stats = statSync(fullPath);

        if (stats.isDirectory()) {
            files.push(...collectFiles(fullPath));
            continue;
        }

        if (fullPath.endsWith('.tsx') || fullPath.endsWith('.ts')) {
            files.push(fullPath);
        }
    }

    return files;
};

const getLineNumber = (content, index) => content.slice(0, index).split('\n').length;

const files = collectFiles(sourceRoot);
const invalidIds = [];
const idOccurrences = new Map();

for (const filePath of files) {
    const content = readFileSync(filePath, 'utf8');
    let match;

    while ((match = uiTestIdCallRegex.exec(content)) !== null) {
        const fullMatch = match[0];
        const value = match[2];
        const startIndex = match.index + fullMatch.indexOf(value);
        const line = getLineNumber(content, startIndex);
        const location = `${relative(projectRoot, filePath)}:${line}`;

        if (!kebabCaseRegex.test(value)) {
            invalidIds.push(`${location} -> "${value}"`);
        }

        const occurrences = idOccurrences.get(value) ?? [];
        occurrences.push(location);
        idOccurrences.set(value, occurrences);
    }
}

const duplicates = [...idOccurrences.entries()].filter(([, occurrences]) => occurrences.length > 1);

if (invalidIds.length === 0 && duplicates.length === 0) {
    console.log('UI test IDs ok: kebab-case and unique.');
    process.exit(0);
}

if (invalidIds.length > 0) {
    console.error('Invalid UI test IDs (must be kebab-case):');
    for (const invalid of invalidIds) {
        console.error(`- ${invalid}`);
    }
}

if (duplicates.length > 0) {
    console.error('Duplicate UI test IDs found:');
    for (const [id, occurrences] of duplicates) {
        console.error(`- "${id}"`);
        for (const occurrence of occurrences) {
            console.error(`  - ${occurrence}`);
        }
    }
}

process.exit(1);
