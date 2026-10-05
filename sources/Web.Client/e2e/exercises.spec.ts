import type { Page } from '@playwright/test';
import { expectUniqueTestIds } from './support/expectUniqueTestIds';
import { expect, test, uniqueName } from './support/fixtures';
import { t } from './support/i18n';

/** Title, subject "Mathe" and grade 2 - the fields every exercise needs. */
const fillDetails = async (page: Page, title: string) => {
    await page.getByLabel(t('labelExerciseTitle')).fill(title);
    await page.getByRole('combobox', { name: t('labelExerciseSubject') }).click();
    await page.getByRole('option', { name: 'Mathe', exact: true }).click();
    await page.getByRole('combobox', { name: t('labelGrade') }).click();
    await page.getByRole('option', { name: t('captionGrade', { grade: 2 }) }).click();
};

const question = (page: Page, number: number) =>
    page.getByTestId(`exercise-editor-questions-item-${number}`);

test.describe('exercises', () => {
    test('creates an exercise with every question type, reorders and publishes it', async ({
        admin,
        page,
    }) => {
        void admin;
        const title = uniqueName('Übung');

        await page.goto('/admin/exercises');
        await page.getByRole('button', { name: t('labelAddExercise') }).click();
        await expect(page).toHaveURL(/\/admin\/exercises\/new$/);
        await fillDetails(page, title);

        // 1 · choice
        await page.getByTestId('exercise-editor-questions-add-choice').click();
        await question(page, 1).getByLabel(t('labelQuestionPrompt')).fill('Was ist 2 + 2?');
        await question(page, 1)
            .getByLabel(t('labelAnswerOption', { number: 1 }), { exact: true })
            .fill('3');
        await question(page, 1)
            .getByLabel(t('labelAnswerOption', { number: 2 }), { exact: true })
            .fill('4');
        await question(page, 1)
            .getByLabel(t('labelAnswerOptionCorrect', { number: 2 }), { exact: true })
            .check();

        // 2 · text
        await page.getByTestId('exercise-editor-questions-add-text').click();
        await question(page, 2).getByLabel(t('labelQuestionPrompt')).fill('Was heißt Hund?');
        await question(page, 2)
            .getByLabel(t('labelAcceptedAnswer', { number: 1 }), { exact: true })
            .fill('dog');

        // 3 · cloze
        await page.getByTestId('exercise-editor-questions-add-cloze').click();
        await question(page, 3).getByLabel(t('labelQuestionPrompt')).fill('Setze ein.');
        await question(page, 3).getByLabel(t('labelClozeText')).fill('Der [Hund|Dackel] bellt.');
        await expect(question(page, 3)).toContainText(t('captionClozeGapCount', { count: 1 }));

        // 4 · match
        await page.getByTestId('exercise-editor-questions-add-match').click();
        await question(page, 4).getByLabel(t('labelQuestionPrompt')).fill('Ordne zu.');
        await question(page, 4)
            .getByLabel(t('labelMatchLeft', { number: 1 }), { exact: true })
            .fill('dog');
        await question(page, 4)
            .getByLabel(t('labelMatchRight', { number: 1 }), { exact: true })
            .fill('Hund');
        await question(page, 4)
            .getByLabel(t('labelMatchLeft', { number: 2 }), { exact: true })
            .fill('cat');
        await question(page, 4)
            .getByLabel(t('labelMatchRight', { number: 2 }), { exact: true })
            .fill('Katze');

        // 5 · flashcard
        await page.getByTestId('exercise-editor-questions-add-flashcard').click();
        await question(page, 5).getByLabel(t('labelQuestionPrompt')).fill('Weißt du es?');
        await question(page, 5).getByLabel(t('labelFlashcardFront')).fill('house');
        await question(page, 5).getByLabel(t('labelFlashcardBack')).fill('Haus');

        // Move the flashcard one up, look at the child's view of question 1.
        await page.getByRole('button', { name: t('labelMoveQuestionUp', { number: 5 }) }).click();
        await expect(question(page, 4)).toContainText(t('labelQuestionTypeFlashcard'));
        await page.getByRole('button', { name: t('labelShowPreview', { number: 1 }) }).click();
        await expect(page.getByTestId('exercise-editor-questions-item-1-preview')).toContainText(
            'Was ist 2 + 2?',
        );
        await expectUniqueTestIds(page);

        await page.getByRole('button', { name: t('labelSaveDraft') }).click();
        await expect(page.getByText(t('notificationExerciseSaved'))).toBeVisible();
        await expect(page).toHaveURL(/\/admin\/exercises\/\d+$/);

        await page.getByRole('button', { name: t('labelPublishExercise') }).click();
        await expect(page.getByText(t('notificationExercisePublished'))).toBeVisible();
        await expect(page.getByTestId('exercise-editor-state')).toContainText(
            t('captionExercisePublished', { version: 1 }),
        );

        // After a reload the order is still the new one.
        await page.reload();
        await expect(question(page, 4)).toContainText(t('labelQuestionTypeFlashcard'));
        await expect(question(page, 5)).toContainText(t('labelQuestionTypeMatch'));

        await page.getByRole('link', { name: t('labelBackToExercises') }).click();
        const item = page.locator('[data-testid^="exercises-item-"]').filter({ hasText: title });
        await expect(item).toContainText(t('captionExerciseTaskCount', { count: 5 }));
        await expect(item).toContainText(t('captionExercisePublished', { version: 1 }));
    });

    test('creates a generator exercise with a preview', async ({ admin, page }) => {
        void admin;
        const title = uniqueName('Rechnen');

        await page.goto('/admin/exercises/new');
        await fillDetails(page, title);
        await page
            .getByRole('button', { name: t('labelContentSourceGenerator'), exact: true })
            .click();
        await page.getByRole('button', { name: t('labelOperationSubtract') }).click();
        await page
            .getByRole('button', { name: t('labelNumberRangeUpTo', { value: 100 }), exact: true })
            .click();

        const tasks = page.getByTestId('exercise-editor-generator-preview-tasks');
        await expect(tasks.locator(':scope > *')).toHaveCount(10);
        await page.getByRole('button', { name: t('labelNewTasks') }).click();
        await expect(tasks.locator(':scope > *')).toHaveCount(10);
        await expectUniqueTestIds(page);

        await page.getByRole('button', { name: t('labelPublishExercise') }).click();
        await expect(page.getByText(t('notificationExercisePublished'))).toBeVisible();

        await page.goto('/admin/exercises');
        const item = page.locator('[data-testid^="exercises-item-"]').filter({ hasText: title });
        await expect(item).toContainText(t('captionExerciseTaskCount', { count: 10 }));
    });

    test('shows what is missing instead of saving', async ({ admin, page }) => {
        void admin;

        await page.goto('/admin/exercises/new');
        await page.getByTestId('exercise-editor-questions-add-choice').click();
        await page.getByRole('button', { name: t('labelSaveDraft') }).click();

        await expect(page.getByText(t('notificationExerciseHasErrors'))).toBeVisible();
        await expect(page.getByText(t('notificationExerciseTitleRequired'))).toBeVisible();
        await expect(question(page, 1)).toContainText(t('notificationChoiceCorrectRequired'));
        await expect(page).toHaveURL(/\/admin\/exercises\/new$/);
    });
});
