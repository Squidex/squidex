/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { expect, test } from '../_fixture';
import { ClientsPage } from '../pages';
import { getRandomId } from '../utils';

test.beforeEach(async ({ context, appsPage, clientsPage }) => {
    await context.grantPermissions(['clipboard-read', 'clipboard-write']);

    const appName = `app-${getRandomId()}`;
    await appsPage.createNewApp(appName);

    await clientsPage.goto(appName);
});

test('has header', async ({ page }) => {
    const header = page.getByRole('heading', { name: /Clients/ });

    await expect(header).toBeVisible();
});

test('add client', async ({ clientsPage }) => {
    const clientId = await createRandomClient(clientsPage);
    const clientCard = await clientsPage.getClientCard(clientId);

    await expect(clientCard.root).toBeVisible();
});

test('copy client ID', async ({ page, clientsPage }) => {
    const clientCard = await clientsPage.getClientCard('default');
    await clientCard.copyClientId();

    const handle = await page.evaluateHandle(() => navigator.clipboard.readText());
    const clipboardContent = await handle.jsonValue();

    await expect(clipboardContent).toContain(':default');
});

test('copy client Secret', async ({ page, clientsPage }) => {
    const clientCard = await clientsPage.getClientCard('default');
    await clientCard.copyClientSecret();

    const handle = await page.evaluateHandle(() => navigator.clipboard.readText());
    const clipboardContent = await handle.jsonValue();

    await expect(clipboardContent.length).toBeGreaterThan(40);
});

test('rename rule with dbclick', async ({ clientsPage }) => {
    const clientId = await createRandomClient(clientsPage);
    const clientCard = await clientsPage.getClientCard(clientId);

    const newName = `client-${getRandomId()}`;

    const renameDialog = await clientCard.startRenameDblClick();
    await renameDialog.enterName(newName);
    await renameDialog.save();
    const renamedCard = await clientsPage.getClientCard(newName);

    await expect(renamedCard.root).toBeVisible();
});

test('rename rule with button', async ({ clientsPage }) => {
    const clientId = await createRandomClient(clientsPage);
    const clientCard = await clientsPage.getClientCard(clientId);

    const newName = `client-${getRandomId()}`;

    const renameDialog = await clientCard.startRenameButton();
    await renameDialog.enterName(newName);
    await renameDialog.save();
    const renamedCard = await clientsPage.getClientCard(newName);

    await expect(renamedCard.root).toBeVisible();
});

async function createRandomClient(clientsPage: ClientsPage) {
    const clientId = `client-${getRandomId()}`;

    await clientsPage.enterClientId(clientId);
    await clientsPage.save();

    return clientId;
}
