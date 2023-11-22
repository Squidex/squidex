import { expect, test } from '@playwright/test';

test('has title', async ({ page }) => {
    await page.goto('/');
    await expect(page).toHaveTitle(/Squidex/);
});

test('visual test', async ({ page }) => {
    await page.goto('/');
    await expect(page).toHaveScreenshot();
});