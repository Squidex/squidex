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

test('login', async ({ page, loginPage }) => {
    const popup = await loginPage.openPopup();
    await popup.enterEmail('hello@squidex.io');
    await popup.enterPassword('1q2w3e$R'),
    await popup.login();

    await page.waitForURL(/app/);

    await expect(page).toHaveTitle(/Apps/);
});

test('visual test', async ({ loginPage }) => {
    const popup = await loginPage.openPopup();

    await expect(popup.root).toHaveScreenshot({ fullPage: true });
});