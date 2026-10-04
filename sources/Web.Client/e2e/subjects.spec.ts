import { expect, test, uniqueName } from './support/fixtures';
import { t } from './support/i18n';
import { testIds } from '../src/lib/testing/testIds';

test.describe('subjects smoke', () => {
    test('shows standard subjects and creates a subject with color and icon', async ({
        admin,
        page,
    }) => {
        void admin;
        const subjectName = uniqueName('Fach');

        await page.goto('/admin/subjects');

        const subjectItems = page.locator(`[data-testid^="${testIds.subjects.list}.item."]`);
        await expect(subjectItems.first()).toBeVisible();

        await page.getByRole('button', { name: t('labelAddSubject') }).click();
        await page.getByLabel(t('labelSubjectName')).fill(subjectName);
        await page.getByRole('radio', { name: t('labelSubjectColorBlue') }).click();
        await page.getByRole('radio', { name: t('labelSubjectIconStar') }).click();
        await page.getByRole('button', { name: t('labelSave') }).click();

        await expect(page.getByText(subjectName)).toBeVisible();
    });
});
