/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { readJsonAsync } from '../utils';
import { test as base } from './../given-app/_fixture';
export { expect } from '@playwright/test';

type SchemaFixture = {
    schemaName: string;

    fields: [{
        name: string;
    }];
};

export const test = base.extend<{}, SchemaFixture>({
    schemaName: [async ({}, use) => {
        const config = await readJsonAsync<SchemaFixture>('schema', null!);

        await use(config.schemaName);
    }, { scope: 'worker', auto: true }],

    fields: [async ({}, use) => {
        const config = await readJsonAsync<SchemaFixture>('fields', null!);

        await use(config.fields);
    }, { scope: 'worker', auto: true }],
});