/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { getRandomId, writeJsonAsync } from '../utils';
import { test as setup } from './../given-login/_fixture';

setup('prepare app', async ({ appsPage }) => {
    const appName = `my-app-${getRandomId()}`;

    await appsPage.goto();

    const dialog = await appsPage.openAppDialog();
    await dialog.enterName(appName);
    await dialog.save();

    await appsPage.gotoApp(appName);

    await writeJsonAsync('app', { appName });
});