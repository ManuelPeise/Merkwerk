import { expectUniqueTestIds } from './support/expectUniqueTestIds';
import { expect, test, uniqueName } from './support/fixtures';
import { t } from './support/i18n';

test.describe('devices smoke', () => {
    test('pairs in second context and switches child', async ({
        admin,
        page,
        browser,
    }, testInfo) => {
        // A family has one active pairing code: a new code replaces the old one. Both projects share
        // the test family and run in parallel, so the flow runs in the desktop project only.
        test.skip(
            testInfo.project.name !== 'chromium-desktop',
            'One active pairing code per family.',
        );
        void admin;

        const learnerName = uniqueName('Profil');

        await page.goto('/admin/family');
        await page.getByRole('button', { name: t('labelAddChild') }).click();
        await page.getByLabel(t('labelChildName')).fill(learnerName);
        await page.getByRole('button', { name: t('labelSave') }).click();
        await expect(page.getByText(learnerName)).toBeVisible();

        await page.goto('/admin/devices');
        await expectUniqueTestIds(page);
        await page.getByRole('button', { name: t('labelCreatePairingCode') }).click();

        // Wait until the API answered and the code is on screen.
        const codeValue = page.getByTestId('devices-pairing-code-value');
        await expect(codeValue).toHaveText(/^\d{6}$/);

        const code = (await codeValue.textContent())?.trim();
        if (!code) {
            throw new Error('Pairing code not found in pairing section.');
        }

        const contextB = await browser.newContext();
        const pageB = await contextB.newPage();

        await pageB.goto(`/practice/pair?code=${code}`);
        await expect(pageB).toHaveURL(/\/practice\/profiles$/);
        await expectUniqueTestIds(pageB);

        await pageB.getByRole('button', { name: learnerName }).click();
        await expect(pageB).toHaveURL(/\/practice$/);
        await expect(pageB.getByTestId('practice-home-page')).toContainText(learnerName);

        await pageB.getByRole('button', { name: t('labelSwitchChild') }).click();
        await expect(pageB).toHaveURL(/\/practice\/profiles$/);

        await contextB.close();
    });
});
