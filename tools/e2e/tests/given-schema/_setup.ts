/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { test as setup } from '../given-app/_fixture';
import { getRandomId, writeJsonAsync } from '../utils';

const fields = [{
    name: 'my-string',
}];

setup('prepare schema', async ({ appName, schemasPage, schemaPage }) => {
    const schemaName = `my-schema-${getRandomId()}`;

    await schemasPage.goto(appName);

    const createDialog = await schemasPage.openSchemaDialog();
    await createDialog.enterName(schemaName);
    await createDialog.save();

    await schemaPage.publish();

    for (const field of fields) {
        const fieldDialog = await schemaPage.openFieldWizard();
        await fieldDialog.enterName(field.name);
        await fieldDialog.enterType('String');
        await fieldDialog.createAndClose();
    }

    await writeJsonAsync('schema', { schemaName });
    await writeJsonAsync('fields', { fields });
});