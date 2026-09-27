/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { getRandomId } from '../utils';
import { expect, test } from './_fixture';

test('sidebar plugin receives theme', async ({ page, appName, schemasPage, schemaPage, baseURL }) => {
    const schemaName = `schema-${getRandomId()}`;

    await schemasPage.goto(appName);

    const schemaDialog = await schemasPage.openSchemaDialog();
    await schemaDialog.enterName(schemaName);
    await schemaDialog.save();

    await schemaPage.publish();

    await page.getByRole('link', { name: 'More' }).click();

    const form = page.locator('sqx-schema-edit-form form');
    await form.locator('#contentsSidebarUrl').fill(`${baseURL}/scripts/sidebar-context.html`);

    const saved = page.waitForResponse(x => x.url().includes(`/schemas/${schemaName}`) && x.request().method() === 'PUT');
    await form.getByRole('button', { name: 'Save' }).click();
    await saved;

    await page.goto(`/app/${appName}/content/${schemaName}/sidebar`);

    const plugin = page.frameLocator('sqx-content-extension iframe').locator('html');

    await expect(plugin).toHaveAttribute('data-bs-theme', 'light');

    await page.locator('sqx-profile-menu .dropdown-toggle').click();
    await page.getByRole('button', { name: 'Theme' }).click();
    await page.getByRole('button', { name: 'Dark', exact: true }).click();

    await expect(plugin).toHaveAttribute('data-bs-theme', 'dark');
});
