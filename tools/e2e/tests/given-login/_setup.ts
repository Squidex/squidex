/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { STORAGE_STATE } from '../../playwright.config';
import { test as setup } from './_fixture';

setup('prepare login', async ({ page, userEmail, userPassword, loginPage }) => {
    await loginPage.goto();

    const popup = await loginPage.openPopup();
    await popup.enterEmail(userEmail);
    await popup.enterPassword(userPassword),
    await popup.login();

    await page.waitForURL(/app/);

    await page.context().storageState({ path: STORAGE_STATE });
});
