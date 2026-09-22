/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { readFile } from 'fs/promises';
import path from 'path';
import { test as setup } from '../given-login/_fixture';
import { writeJsonAsync } from '../utils';

setup('prepare visual app', async ({ page, appsPage, assetsPage, contentsPage, contentPage, rulesPage, rulePage, schemasPage, schemaPage }) => {
    setup.setTimeout(120_000);

    // Use a dedicated app with a fixed name, so that other tests cannot change the screenshots.
    const appName = 'visual-tests';
    const schemaName = 'visual-schema';

    await writeJsonAsync('visual', { appName, schemaName });

    await appsPage.goto();

    // The app is only created once per database, because the name cannot be reused.
    if (await page.getByRole('heading', { name: appName, exact: true }).isVisible()) {
        return;
    }

    await appsPage.createNewApp(appName);

    await schemasPage.goto(appName);

    const schemaDialog = await schemasPage.openSchemaDialog();
    await schemaDialog.enterName(schemaName);
    await schemaDialog.save();

    await schemaPage.publish();

    for (const [name, type] of [['title', 'String'], ['count', 'Number'], ['active', 'Boolean'], ['text', 'RichText']]) {
        const fieldDialog = await schemaPage.openFieldWizard();
        await fieldDialog.enterName(name);
        await fieldDialog.enterType(type);
        await fieldDialog.createAndClose();

        // Wait for the field, otherwise the request is cancelled when navigating away.
        const fieldRow = await schemaPage.getFieldRow(name);
        await fieldRow.root.waitFor({ state: 'visible' });
    }

    await contentsPage.goto(appName, schemaName);
    await contentsPage.addContent();

    await page.locator('sqx-field-editor').first().getByRole('textbox').fill('Hello Squidex');
    await contentPage.savePublishAndClose();

    await assetsPage.goto(appName);
    await assetsPage.uploadFile({
        name: 'logo.png',
        mimeType: 'image/png',
        buffer: await readFile(path.join(__dirname, '../../assets/logo-squared.png')),
    });

    await assetsPage.getAssetCard('logo.png').then(x => x.root.waitFor({ state: 'visible' }));

    await rulesPage.goto(appName);
    await rulesPage.addRule();

    const triggerDialog = await rulePage.addTrigger();
    await triggerDialog.selectContentChangedTrigger();
    await triggerDialog.add();

    const stepDialog = await rulePage.addStep();
    await stepDialog.selectWebhookAction();
    await stepDialog.add();

    await rulePage.enterName('visual-rule');
    await rulePage.save();
});
