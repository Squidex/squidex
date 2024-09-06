/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { getRandomId } from '../utils';
import { expect, test } from './_fixture';

test.beforeEach(async ({ appsPage }) => {
    await appsPage.goto();
});

test('create app', async ({ page, appsPage }) => {
    const appName = `my-app-${getRandomId()}`;

    const appDialog = await appsPage.openAppDialog();
    await appDialog.enterName(appName);
    await appDialog.save();

    const newApp = page.getByRole('heading', { name: appName });

    await expect(newApp).toBeVisible();
});