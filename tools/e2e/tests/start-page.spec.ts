/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { expect, test } from '@playwright/test';

test.beforeEach(async ({ page }) => {
    await page.goto('/');
});

test('has title', async ({ page }) => {
    await expect(page).toHaveTitle(/Squidex/);
});

test('visual test', async ({ page }) => {
    await expect(page).toHaveScreenshot();
});