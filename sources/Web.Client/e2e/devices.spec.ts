import { expect, test, uniqueName } from './support/fixtures';
import { t } from './support/i18n';
import { testIds } from '../src/lib/testing/testIds';

test.describe('devices smoke', () => {
    test('pairs in second context and switches child', async ({ admin, page, browser }) => {
        void admin;

        const learnerName = uniqueName('Profil');

        await page.goto('/admin/family');
        await page.getByRole('button', { name: t('labelAddChild') }).click();
        await page.getByLabel(t('labelChildName')).fill(learnerName);
        await page.getByRole('button', { name: t('labelSave') }).click();
        await expect(page.getByText(learnerName)).toBeVisible();

        await page.goto('/admin/devices');
        await page.getByRole('button', { name: t('labelCreatePairingCode') }).click();

        const pairingSection = page.getByTestId(testIds.devices.pairingCode);
        await expect(pairingSection).toBeVisible();

        const pairingText = await pairingSection.textContent();
        const code = pairingText?.match(/\b\d{6}\b/)?.[0];

        expect(code).toBeTruthy();
        if (!code) {
            throw new Error('Pairing code not found in pairing section.');
        }

        const contextB = await browser.newContext();
        const pageB = await contextB.newPage();

        await pageB.goto(`/practice/pair?code=${code}`);
        await expect(pageB).toHaveURL(/\/practice\/profiles$/);

        await pageB.getByRole('button', { name: learnerName }).click();
        await expect(pageB).toHaveURL(/\/practice$/);
        await expect(pageB.getByTestId(testIds.practice.home)).toContainText(learnerName);

        await pageB.getByRole('button', { name: t('labelSwitchChild') }).click();
        await expect(pageB).toHaveURL(/\/practice\/profiles$/);

        await contextB.close();
    });
});
