import { expect, test as base } from '@playwright/test';
import { loginAsAdult, type ISetupOwner } from './api';

interface IFixtures {
    admin: void;
}

export const owner: ISetupOwner = {
    familyName: 'E2E Family',
    displayName: 'E2E Owner',
    email: 'owner.e2e@merkwerk.local',
    password: 'E2E-Owner-Password-123!',
};

/** Short unique name (prefix + 8 chars) – child names may have at most 30 characters. */
export const uniqueName = (prefix: string): string => {
    const time = Date.now().toString(36).slice(-4);
    const random = Math.random().toString(36).slice(2, 6).padEnd(4, '0');

    return `${prefix}-${time}${random}`;
};

export const test = base.extend<IFixtures>({
    admin: async ({ page }, runFixture) => {
        // The instance itself is set up once in globalSetup.ts.
        await loginAsAdult(page.request, { email: owner.email, password: owner.password });
        await page.goto('/admin');
        await expect(page).toHaveURL(/\/admin$/);

        await runFixture();
    },
});

export { expect };
