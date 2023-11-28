/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { expect, test } from './_fixture';

test.beforeEach(async ({ page, appName }) => {
    await page.goto(`/app/${appName}`);
});

test('visual test', async ({ page }) => {
    await expect(page).toHaveScreenshot({ maxDiffPixelRatio: 0.05 });
});