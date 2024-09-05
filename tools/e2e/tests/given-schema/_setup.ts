/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { expect, test as setup } from '../given-app/_fixture';
import { createField, createSchema, getRandomId, writeJsonAsync } from '../utils';

setup('prepare schema', async ({ page, appName }) => {
    const schemaName = `my-schema-${getRandomId()}`;

    const fields = [{
        name: 'my-string',
    }];

    await page.goto(`/app/${appName}/schemas`);

    // Add schema.
    await createSchema(page, { name: schemaName });

    // Add fields.
    for (const field of fields) {
        await createField(page, { name: field.name });
    }

    // Publish schema.
    await page.getByRole('button', { name: 'Published', exact: true }).click();

    // Just wait for the publish operation to complete.
    await expect(page.getByRole('button', { name: 'Published', exact: true })).toBeDisabled();

    await writeJsonAsync('schema', { schemaName });
    await writeJsonAsync('fields', { fields });
});