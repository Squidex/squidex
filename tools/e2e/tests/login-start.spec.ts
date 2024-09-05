/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { expect, test } from './_fixture';

test.beforeEach(async ({ loginPage }) => {
    await loginPage.goto();
});

test('has title', async ({ page }) => {
    await expect(page).toHaveTitle(/Squidex/);
});

test('visual test', async ({ page }) => {
    await expect(page).toHaveScreenshot();
});