/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Page } from '@playwright/test';
import { expect, test } from './_fixture';

test.beforeEach(async ({ appsPage }) => {
    await appsPage.goto();
});

test('switch to dark theme', async ({ page }) => {
    await selectTheme(page, 'Dark');

    await expect(page.locator('html')).toHaveAttribute('data-bs-theme', 'dark');
});

test('switch to light theme', async ({ page }) => {
    await selectTheme(page, 'Light');

    await expect(page.locator('html')).toHaveAttribute('data-bs-theme', 'light');
});

test('keep theme after reload', async ({ page }) => {
    await selectTheme(page, 'Dark');
    await page.reload();

    await expect(page.locator('html')).toHaveAttribute('data-bs-theme', 'dark');
});

test('use system theme', async ({ page }) => {
    await page.emulateMedia({ colorScheme: 'dark' });
    await selectTheme(page, 'System');

    await expect(page.locator('html')).toHaveAttribute('data-bs-theme', 'dark');

    await page.emulateMedia({ colorScheme: 'light' });

    await expect(page.locator('html')).toHaveAttribute('data-bs-theme', 'light');
});

async function selectTheme(page: Page, theme: string) {
    await page.locator('sqx-profile-menu .dropdown-toggle').click();
    await page.getByRole('button', { name: 'Theme' }).click();
    await page.getByRole('button', { name: theme, exact: true }).click();
}
