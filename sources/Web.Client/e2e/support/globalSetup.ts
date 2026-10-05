import { request, type FullConfig } from '@playwright/test';
import { deleteFamilyData, getSetupStatus, initializeInstance } from './api';
import { owner } from './fixtures';

/**
 * Runs once before all tests: sets the instance up with the E2E owner if it is still new. Doing this per test made
 * parallel workers race for the first-run setup (409). An instance set up by hand with another owner cannot be
 * used - the tests then stop with a clear message instead of failing every login with 401. Children and groups of
 * earlier runs are deleted (a family has at most 10 children), so the tests run again without a fresh database.
 */
const globalSetup = async (config: FullConfig): Promise<void> => {
    const baseURL = config.projects[0]?.use.baseURL;
    const context = await request.newContext({ baseURL });

    try {
        if ((await getSetupStatus(context)).isSetupRequired) {
            await initializeInstance(context, owner);
        }

        const login = await context.post('/api/v1/authentication/login', {
            data: { email: owner.email, password: owner.password },
        });

        if (!login.ok()) {
            throw new Error(
                `The E2E owner ${owner.email} cannot sign in (HTTP ${login.status()}). The instance was probably set ` +
                    'up by hand - start the tests with a fresh database (dotnet ef database drop, restart Web.Core).',
            );
        }

        await deleteFamilyData(context);
    } finally {
        await context.dispose();
    }
};

export default globalSetup;
