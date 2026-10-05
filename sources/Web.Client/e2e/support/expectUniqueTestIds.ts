import type { Page } from '@playwright/test';
import { expect } from './fixtures';

/** Our own ids (naming rule). MUI icons add ids like "AddIcon" on their own – those are ignored. */
const ownTestIdPattern = '^[a-z0-9]+(-[a-z0-9]+)+$';

interface IDuplicateTestId {
    id: string;
    count: number;
}

export const expectUniqueTestIds = async (page: Page): Promise<void> => {
    const duplicates = await page.evaluate((pattern) => {
        const ownTestId = new RegExp(pattern);
        const counts = new Map<string, number>();

        document.querySelectorAll<HTMLElement>('[data-testid]').forEach((element) => {
            const value = element.getAttribute('data-testid');

            if (!value || !ownTestId.test(value)) {
                return;
            }

            counts.set(value, (counts.get(value) ?? 0) + 1);
        });

        return [...counts.entries()]
            .filter(([, count]) => count > 1)
            .map(([id, count]) => ({ id, count }));
    }, ownTestIdPattern);

    expect(duplicates as IDuplicateTestId[]).toEqual([]);
};
