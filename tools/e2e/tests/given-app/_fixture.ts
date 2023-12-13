/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { readJsonAsync } from '../utils';
import { test as base } from './../given-login/_fixture';
export { expect } from '@playwright/test';

type AppFixture = {
    appName: string;
};

export const test = base.extend<{}, AppFixture>({
    appName: [async ({}, use) => {
        const config = await readJsonAsync<AppFixture>('app', null!);

        await use(config.appName);
    }, { scope: 'worker', auto: true }],
});