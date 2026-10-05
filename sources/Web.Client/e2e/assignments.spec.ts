import type { Page } from '@playwright/test';
import { expectUniqueTestIds } from './support/expectUniqueTestIds';
import { expect, test, uniqueName } from './support/fixtures';
import { t } from './support/i18n';

/** "yyyy-MM-dd" in a week - what the date input expects. */
const inOneWeek = (): string => {
    const date = new Date();
    date.setDate(date.getDate() + 7);

    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${date.getFullYear()}-${month}-${day}`;
};

const addChildInGroup = async (page: Page, childName: string, groupName: string) => {
    await page.goto('/admin/family');
    await page.getByRole('button', { name: t('labelAddChild') }).click();
    await page.getByLabel(t('labelChildName')).fill(childName);
    await page.getByRole('button', { name: t('labelSave') }).click();
    await expect(page.getByText(childName)).toBeVisible();

    await page.getByRole('button', { name: t('labelAddGroup') }).click();
    await page.getByLabel(t('labelGroupName')).fill(groupName);
    await page.getByRole('dialog').getByRole('checkbox', { name: childName }).click();
    await page.getByRole('button', { name: t('labelSave') }).click();
    await expect(page.getByRole('dialog')).toHaveCount(0);
};

const publishGeneratorExercise = async (page: Page, title: string) => {
    await page.goto('/admin/exercises/new');
    await page.getByLabel(t('labelExerciseTitle')).fill(title);
    await page.getByRole('combobox', { name: t('labelExerciseSubject') }).click();
    await page.getByRole('option', { name: 'Mathe', exact: true }).click();
    await page.getByRole('combobox', { name: t('labelGrade') }).click();
    await page.getByRole('option', { name: t('captionGrade', { grade: 2 }) }).click();
    await page.getByRole('button', { name: t('labelContentSourceGenerator'), exact: true }).click();
    await page.getByRole('button', { name: t('labelPublishExercise') }).click();
    await expect(page.getByText(t('notificationExercisePublished'))).toBeVisible();
};

test.describe('assignments', () => {
    test('assigns an exercise to a child and a group, then takes one back', async ({
        admin,
        page,
    }) => {
        void admin;
        const childName = uniqueName('Kind');
        const groupName = uniqueName('Team');
        const title = uniqueName('Rechnen');

        await addChildInGroup(page, childName, groupName);
        await publishGeneratorExercise(page, title);

        await page.goto('/admin/exercises');
        await page.getByRole('button', { name: t('labelAssignExercise', { title }) }).click();

        const dialog = page.getByTestId('exercises-assign-dialog');
        const list = page.getByTestId('exercises-assign-dialog-list');
        await expect(list).toContainText(t('captionNoAssignments'));

        await dialog.getByRole('checkbox', { name: childName }).click();
        await dialog
            .getByRole('checkbox', { name: t('captionAssignmentGroup', { name: groupName }) })
            .click();
        await dialog.getByLabel(t('labelDueDate')).fill(inOneWeek());
        await dialog.getByRole('checkbox', { name: t('labelAllowDotArray') }).click();
        await dialog.getByRole('checkbox', { name: t('labelOwnGeneratorSettings') }).click();
        await dialog
            .getByRole('button', { name: t('labelNumberRangeUpTo', { value: 100 }), exact: true })
            .click();
        await expectUniqueTestIds(page);

        await dialog.getByRole('button', { name: t('labelAssign'), exact: true }).click();

        await expect(dialog.getByText(t('notificationAssignmentSaved'))).toBeVisible();
        await expect(list.getByRole('listitem')).toHaveCount(2);
        await expect(list).toContainText(childName);
        await expect(list).toContainText(t('captionAssignmentGroup', { name: groupName }));
        await expect(list).toContainText(t('captionAidDotArray'));
        await expect(list).toContainText(t('captionAssignmentOwnSettings'));

        await dialog
            .getByRole('button', { name: t('labelRevokeAssignment', { name: childName }) })
            .click();

        await expect(dialog.getByText(t('notificationAssignmentRevoked'))).toBeVisible();
        await expect(list.getByRole('listitem')).toHaveCount(1);
        await expect(list).not.toContainText(childName);
    });

    test('offers assigning only for published exercises', async ({ admin, page }) => {
        void admin;
        const title = uniqueName('Entwurf');

        await page.goto('/admin/exercises/new');
        await page.getByLabel(t('labelExerciseTitle')).fill(title);
        await page.getByRole('combobox', { name: t('labelExerciseSubject') }).click();
        await page.getByRole('option', { name: 'Mathe', exact: true }).click();
        await page.getByRole('combobox', { name: t('labelGrade') }).click();
        await page.getByRole('option', { name: t('captionGrade', { grade: 2 }) }).click();
        await page
            .getByRole('button', { name: t('labelContentSourceGenerator'), exact: true })
            .click();
        await page.getByRole('button', { name: t('labelSaveDraft') }).click();
        await expect(page.getByText(t('notificationExerciseSaved'))).toBeVisible();

        await page.goto('/admin/exercises');
        await expect(
            page.locator('[data-testid^="exercises-item-"]').filter({ hasText: title }),
        ).toBeVisible();
        await expect(
            page.getByRole('button', { name: t('labelAssignExercise', { title }) }),
        ).toHaveCount(0);
    });
});
