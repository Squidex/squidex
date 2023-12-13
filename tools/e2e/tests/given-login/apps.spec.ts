/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { getRandomId } from '../utils';
import { expect, test } from './_fixture';

test.beforeEach(async ({ page }) => {
    await page.goto('/app');
});

test('create app', async ({ page }) => {
    const appName = `my-app-${getRandomId()}`;

    await page.getByTestId('new-app').click();

    await page.locator('#name').fill(appName);
    await page.getByRole('button', { name: 'Create' }).click();

    const newApp = page.getByRole('heading', { name: appName });

    await expect(newApp).toBeVisible();
});