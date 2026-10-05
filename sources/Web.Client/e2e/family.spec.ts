import { expectUniqueTestIds } from './support/expectUniqueTestIds';
import { expect, test, uniqueName } from './support/fixtures';
import { t } from './support/i18n';

test.describe('family smoke', () => {
    test('adds, edits and deletes a child and keeps group membership in sync', async ({
        admin,
        page,
    }) => {
        void admin;

        const childOneName = uniqueName('Kind-Eins');
        const childOneEditedName = uniqueName('Kind-Neu');
        const childTwoName = uniqueName('Kind-Zwei');
        const groupName = uniqueName('Gruppe');

        await page.goto('/admin/family');
        await expectUniqueTestIds(page);

        await page.getByRole('button', { name: t('labelAddChild') }).click();
        await page.getByLabel(t('labelChildName')).fill(childOneName);
        await page.getByRole('button', { name: t('labelSave') }).click();
        await expect(page.getByText(childOneName)).toBeVisible();

        await page
            .getByRole('button', { name: t('labelEditChild', { name: childOneName }) })
            .click();
        await page.getByLabel(t('labelChildName')).fill(childOneEditedName);
        await page.getByRole('button', { name: t('labelSave') }).click();
        await expect(page.getByText(childOneEditedName)).toBeVisible();

        await page.getByRole('button', { name: t('labelAddChild') }).click();
        await page.getByLabel(t('labelChildName')).fill(childTwoName);
        await page.getByRole('button', { name: t('labelSave') }).click();
        await expect(page.getByText(childTwoName)).toBeVisible();

        await page.getByRole('button', { name: t('labelAddGroup') }).click();
        await page.getByLabel(t('labelGroupName')).fill(groupName);
        // Scoped to the dialog: the list rows behind it have "<name> bearbeiten/löschen" buttons.
        const groupDialog = page.getByRole('dialog');
        await groupDialog.getByRole('checkbox', { name: childOneEditedName }).click();
        await groupDialog.getByRole('checkbox', { name: childTwoName }).click();
        await page.getByRole('button', { name: t('labelSave') }).click();

        const groupItem = page
            .locator('[data-testid^="family-group-item-"]')
            .filter({ hasText: groupName })
            .first();
        await expect(groupItem).toContainText(childOneEditedName);
        await expect(groupItem).toContainText(childTwoName);

        await page
            .getByRole('button', { name: t('labelDeleteChild', { name: childTwoName }) })
            .click();
        await page
            .getByRole('dialog')
            .getByRole('button', { name: t('labelDelete'), exact: true })
            .click();

        await expect(page.getByText(childTwoName)).toHaveCount(0);
        await expect(groupItem).toContainText(childOneEditedName);
        await expect(groupItem).not.toContainText(childTwoName);
    });
});
