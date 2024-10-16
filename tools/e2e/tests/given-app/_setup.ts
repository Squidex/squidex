/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { getRandomId, writeJsonAsync } from '../utils';
import { test as setup } from './../given-login/_fixture';

setup('prepare app', async ({ appsPage }) => {
    const appName = `app-${getRandomId()}`;
    await appsPage.createNewApp(appName);

    await appsPage.gotoApp(appName);

    await writeJsonAsync('app', { appName });
});