/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { readJsonAsync } from '../utils';
import { test as base } from './../given-login/_fixture';
export { expect } from '@playwright/test';

type VisualFixture = {
    appName: string;
    schemaName: string;
};

export const test = base.extend<{}, VisualFixture>({
    appName: [async ({}, use) => {
        const config = await readJsonAsync<VisualFixture>('visual', null!);

        await use(config.appName);
    }, { scope: 'worker' }],

    schemaName: [async ({}, use) => {
        const config = await readJsonAsync<VisualFixture>('visual', null!);

        await use(config.schemaName);
    }, { scope: 'worker' }],
});
