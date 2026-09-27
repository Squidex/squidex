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
import { EDITORS_SCHEMA } from './_editors';

setup('prepare visual app', async ({ page, appsPage, assetsPage, contentsPage, contentPage, rulesPage, rulePage, schemasPage, schemaPage }) => {
    setup.setTimeout(120_000);

    // Use a dedicated app with a fixed name, so that other tests cannot change the screenshots.
    const appName = 'visual-tests';
    const schemaName = 'visual-schema';
    const editorsSchemaName = 'visual-editors';

    await writeJsonAsync('visual', { appName, schemaName, editorsSchemaName });

    await appsPage.goto();

    // Everything is only created once per database, because the names cannot be reused.
    const hasApp = await page.getByRole('heading', { name: appName, exact: true }).isVisible();

    if (!hasApp) {
        await appsPage.createNewApp(appName);
    }

    await schemasPage.goto(appName);

    if (!await hasSchema(schemaName)) {
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

        await schemasPage.goto(appName);
    }

    if (!await hasSchema(editorsSchemaName)) {
        const editorsDialog = await schemasPage.openSchemaDialog();
        await editorsDialog.enterName(editorsSchemaName);
        await editorsDialog.save();
    }

    // The schema defines a field for every editor, which is faster and more stable than the field wizard.
    await page.goto(`/app/${appName}/schemas/${editorsSchemaName}?tab=json`);
    await page.locator('.ace_editor').waitFor({ state: 'visible' });
    await page.locator('.ace_editor').evaluate((element, json) => {
        (window as any).ace.edit(element).setValue(json, -1);
    }, JSON.stringify(EDITORS_SCHEMA, undefined, 2));

    // The editor notifies the form with a delay.
    await page.waitForTimeout(1000);

    const synchronized = page.waitForResponse(x => x.url().includes(`/schemas/${editorsSchemaName}/sync`) && x.request().method() === 'PUT');
    await page.getByRole('button', { name: 'Synchronize' }).click();
    await synchronized;

    if (hasApp) {
        return;
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

    // The schemas are loaded asynchronously, therefore we have to wait for them.
    async function hasSchema(name: string) {
        const link = await schemasPage.getSchemaLink(name);

        return await link.root.waitFor({ timeout: 5000 }).then(() => true, () => false);
    }
});
