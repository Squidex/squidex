/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { expect, test as setup } from '../given-app/_fixture';
import { getRandomId, writeJsonAsync } from '../utils';

setup('prepare schema', async ({ page, appName }) => {
    const schemaName = `my-schema-${getRandomId()}`;

    const fields = [{
        name: 'my-string',
    }];

    await page.goto(`/app/${appName}/schemas`);

    await page.getByLabel('Create Schema').click();

    // Define schema name.
    await page.getByLabel('Name (required)').fill(schemaName);

    // Save schema.
    await page.getByRole('button', { name: 'Create', exact: true }).click();

    for (const field of fields) {
        await page.locator('button').filter({ hasText: /^Add Field$/ }).click();

        // Define field name.
        await page.getByPlaceholder('Enter field name').fill(field.name);

        // Save field.
        await page.getByTestId('dialog').getByRole('button', { name: 'Create' }).click();
    }

    // Publish schema.
    await page.getByRole('button', { name: 'Published', exact: true }).click();

    // Just wait for the publish operation to complete.
    await expect(page.getByRole('button', { name: 'Published', exact: true })).toBeDisabled();

    await writeJsonAsync('schema', { schemaName });
    await writeJsonAsync('fields', { fields });
});