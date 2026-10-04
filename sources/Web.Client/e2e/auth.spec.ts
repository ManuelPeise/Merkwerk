import { loginAsAdult } from './support/api';
import { expect, owner, test } from './support/fixtures';
import { t } from './support/i18n';

test.describe('auth smoke', () => {
    test('logs in with wrong and correct password', async ({ admin, page }) => {
        void admin;
        await page.getByRole('button', { name: t('labelLogout') }).click();
        await expect(page).toHaveURL(/\/login$/);

        await page.getByLabel(t('labelEmail')).fill(owner.email);
        await page.getByLabel(t('labelPassword')).fill('wrong-password');
        await page.getByRole('button', { name: t('labelLogin') }).click();

        await expect(page.getByText(t('notificationLoginFailed'))).toBeVisible();

        await page.getByLabel(t('labelPassword')).fill(owner.password);
        await page.getByRole('button', { name: t('labelLogin') }).click();

        await expect(page).toHaveURL(/\/admin$/);
        await expect(page.getByRole('heading', { name: t('captionAdminArea') })).toBeVisible();
    });

    test('keeps the session after reload', async ({ admin, page }) => {
        void admin;
        await page.goto('/admin');
        await expect(page).toHaveURL(/\/admin$/);

        await page.reload();

        await expect(page).toHaveURL(/\/admin$/);
        await expect(page.getByRole('heading', { name: t('captionAdminArea') })).toBeVisible();
    });

    test('logs out and requires login again', async ({ admin, page }) => {
        void admin;
        await page.goto('/admin');
        await page.getByRole('button', { name: t('labelLogout') }).click();

        await expect(page).toHaveURL(/\/login$/);
        await expect(page.getByRole('heading', { name: t('captionLogin') })).toBeVisible();

        await loginAsAdult(page.request, { email: owner.email, password: owner.password });
        await page.goto('/admin');
        await expect(page).toHaveURL(/\/admin$/);
    });
});
