import { expect, test as base } from '@playwright/test';
import { getSetupStatus, initializeInstance, loginAsAdult, type ISetupOwner } from './api';

interface IFixtures {
    admin: void;
}

export const owner: ISetupOwner = {
    familyName: 'E2E Family',
    displayName: 'E2E Owner',
    email: 'owner.e2e@merkwerk.local',
    password: 'E2E-Owner-Password-123!',
};

export const uniqueName = (prefix: string): string => {
    return `${prefix}-${Date.now()}-${Math.random().toString(36).slice(2, 8)}`;
};

export const test = base.extend<IFixtures>({
    admin: async ({ page }, runFixture) => {
        const setupStatus = await getSetupStatus(page.request);

        if (setupStatus.isSetupRequired) {
            await initializeInstance(page.request, owner);
        }

        await loginAsAdult(page.request, { email: owner.email, password: owner.password });
        await page.goto('/admin');
        await expect(page).toHaveURL(/\/admin$/);

        await runFixture();
    },
});

export { expect };
