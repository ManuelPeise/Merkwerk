import type { Page } from '@playwright/test';
import { expect } from './fixtures';

interface IDuplicateTestId {
    id: string;
    count: number;
}

export const expectUniqueTestIds = async (page: Page): Promise<void> => {
    const duplicates = await page.evaluate(() => {
        const counts = new Map<string, number>();

        document.querySelectorAll<HTMLElement>('[data-testid]').forEach((element) => {
            const value = element.getAttribute('data-testid');

            if (!value) {
                return;
            }

            counts.set(value, (counts.get(value) ?? 0) + 1);
        });

        return [...counts.entries()]
            .filter(([, count]) => count > 1)
            .map(([id, count]) => ({ id, count }));
    });

    expect(duplicates as IDuplicateTestId[]).toEqual([]);
};
