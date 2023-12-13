/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { expect, test } from './_fixture';

test.beforeEach(async ({ page }) => {
    await page.goto('/app');
});

test('has title', async ({ page }) => {
    await expect(page).toHaveTitle(/Apps/);
});