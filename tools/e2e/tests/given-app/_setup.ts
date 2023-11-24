/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { test as setup } from '@playwright/test';
import { getRandomId, writeJsonAsync } from '../utils';

setup('prepare app', async ({ page }) => {
    const appName = `my-app-${getRandomId()}`;

    await page.goto('/app');

    await page.getByTestId('new-app').click();

    await page.locator('#name').fill(appName);
    await page.getByRole('button', { name: 'Create' }).click();

    await page.getByRole('heading', { name: appName }).click();

    await writeJsonAsync('app', { appName });
});