import { expectUniqueTestIds } from './support/expectUniqueTestIds';
import { expect, test, uniqueName } from './support/fixtures';
import { t } from './support/i18n';

test.describe('subjects smoke', () => {
    test('shows standard subjects and creates a subject with color and icon', async ({
        admin,
        page,
    }) => {
        void admin;
        const subjectName = uniqueName('Fach');

        await page.goto('/admin/subjects');
        await expectUniqueTestIds(page);

        const subjectItems = page.locator('[data-testid^="subjects-item-"]');
        await expect(subjectItems.first()).toBeVisible();

        await page.getByRole('button', { name: t('labelAddSubject') }).click();
        await page.getByLabel(t('labelSubjectName')).fill(subjectName);
        await page.getByRole('radio', { name: t('labelSubjectColorBlue') }).click();
        await page.getByRole('radio', { name: t('labelSubjectIconStar') }).click();
        await page.getByRole('button', { name: t('labelSave') }).click();

        await expect(page.getByText(subjectName)).toBeVisible();
    });
});
