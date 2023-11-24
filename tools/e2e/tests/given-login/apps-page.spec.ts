/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { expect, test } from '@playwright/test';

test('has title', async ({ page }) => {
    await page.goto('/app');

    await expect(page).toHaveTitle(/Apps/);
});