/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { expect, test } from './_fixture';

test.beforeEach(async ({ appsPage }) => {
    await appsPage.goto();
});

test('has title', async ({ page }) => {
    await expect(page).toHaveTitle(/Apps/);
});