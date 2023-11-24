/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { expect, test } from '@playwright/test';
import { getRandomId } from '../utils';

test('should create app', async ({ page }) => {
    const appName = `my-app-${getRandomId()}`;

    await page.goto('/app');

    await page.getByTestId('new-app').click();

    await page.locator('#name').fill(appName);
    await page.getByRole('button', { name: 'Create' }).click();

    const newApp = page.getByRole('heading', { name: appName });

    await expect(newApp).toBeVisible();
});