/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { STORAGE_STATE } from '../../playwright.config';
import { test as setup } from './_fixture';

setup('prepare login', async ({ page, userEmail, userPassword }) => {
    await page.goto('/');

    // Start waiting for popup before clicking.
    const popupPromise = page.waitForEvent('popup');

    await page.getByTestId('login').click();

    const popup = await popupPromise;
    await popup.waitForLoadState();

    await popup.getByTestId('login-button').waitFor();

    await popup.getByPlaceholder('Enter Email').fill(userEmail);
    await popup.getByPlaceholder('Enter Password').fill(userPassword);
    await popup.getByTestId('login-button').click();

    await page.waitForURL(/app/);

    await page.context().storageState({ path: STORAGE_STATE });
});
