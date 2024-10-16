/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { readFile } from 'fs/promises';
import path from 'path';
import { AssetsPage } from '../pages';
import { getRandomId } from '../utils';
import { expect, test } from './_fixture';

test.beforeEach(async ({ appName, assetsPage }) => {
    await assetsPage.goto(appName);
});

test('has header', async ({ page }) => {
    const header = page.getByRole('heading', { name: /Assets/ });

    await expect(header).toBeVisible();
});

test('upload asset', async ({ assetsPage }) => {
    const assetName = await uploadRandomAsset(assetsPage);
    const assetCard = await assetsPage.getAssetCard(assetName);

    await expect(assetCard.root).toBeVisible();
});

test('delete asset', async ({ assetsPage }) => {
    const assetName = await uploadRandomAsset(assetsPage);
    const assetCard = await assetsPage.getAssetCard(assetName);
    await assetCard.delete();

    await expect(assetCard.root).not.toBeVisible();
});

test('not delete asset if cancelled', async ({ assetsPage }) => {
    const assetName = await uploadRandomAsset(assetsPage);
    const assetCard = await assetsPage.getAssetCard(assetName);
    await assetCard.delete(/No/);

    await expect(assetCard.root).toBeVisible();
});

test('edit asset name', async ({ assetsPage }) => {
    const assetName = await uploadRandomAsset(assetsPage);
    const assetCard = await assetsPage.getAssetCard(assetName);

    const newName = `file-${getRandomId()}`;
    const assetDialog = await assetCard.edit();
    await assetDialog.enterName(newName);
    await assetDialog.save();

    const newCard = await assetsPage.getAssetCard(newName);

    await expect(newCard.root).toBeVisible();
});

test('edit asset metadata', async ({ assetsPage }) => {
    const assetName = await uploadRandomAsset(assetsPage);
    const assetCard = await assetsPage.getAssetCard(assetName);

    const w = '42';
    const h = '13';

    const assetDialog = await assetCard.edit();
    await assetDialog.enterMetadata('pixelWidth', w);
    await assetDialog.enterMetadata('pixelHeight', h);
    await assetDialog.save();

    const newCard = await assetsPage.getAssetCard(`${w}x${h}px`);

    await expect(newCard.root).toBeVisible();
});

test('add asset folder', async ({ assetsPage }) => {
    const folderName = `folder-${getRandomId()}`;

    const folderDialog = await assetsPage.openAssetFolderDialog();
    await folderDialog.enterName(folderName);
    await folderDialog.save();

    const folderCard = await assetsPage.getAssetFolderCard(folderName);

    await expect(folderCard.root).toBeVisible();
});

test('open asset folder', async ({ assetsPage }) => {
    const folderName = `folder-${getRandomId()}`;

    const folderDialog = await assetsPage.openAssetFolderDialog();
    await folderDialog.enterName(folderName);
    await folderDialog.save();

    const folderCard = await assetsPage.getAssetFolderCard(folderName);
    await folderCard.open();

    const moveUpCard = await assetsPage.getAssetFolderCard('<Parent>');

    await expect(moveUpCard.root).toBeVisible();
});

test('rename asset folder', async ({ assetsPage }) => {
    const folderName = `folder-${getRandomId()}`;

    const folderDialog = await assetsPage.openAssetFolderDialog();
    await folderDialog.enterName(folderName);
    await folderDialog.save();
    const folderCard = await assetsPage.getAssetFolderCard(folderName);

    const newName = `folder-${getRandomId()}`;

    const renameDialog = await folderCard.rename();
    await renameDialog.enterName(newName);
    await renameDialog.rename();
    const renamedCard = await assetsPage.getAssetFolderCard(newName);

    await expect(renamedCard.root).toBeVisible();
});

test('delete asset folder', async ({ assetsPage }) => {
    const folderName = `folder-${getRandomId()}`;

    const folderDialog = await assetsPage.openAssetFolderDialog();
    await folderDialog.enterName(folderName);
    await folderDialog.save();

    const folderCard = await assetsPage.getAssetFolderCard(folderName);

    const dropdown = await folderCard.openOptionsDropdown();
    await dropdown.delete();

    await expect(folderCard.root).not.toBeVisible();
});

test('add asset folder to parent', async ({ assetsPage }) => {
    const parentName = `folder-${getRandomId()}`;

    const parentDialog = await assetsPage.openAssetFolderDialog();
    await parentDialog.enterName(parentName);
    await parentDialog.save();

    const parentcard = await assetsPage.getAssetFolderCard(parentName);
    await parentcard.open();

    const childName = `folder-${getRandomId()}`;

    const childDialog = await assetsPage.openAssetFolderDialog();
    await childDialog.enterName(childName);
    await childDialog.save();

    const childCard = await assetsPage.getAssetFolderCard(childName);

    const moveUpCard = await assetsPage.getAssetFolderCard('<Parent>');
    await moveUpCard.open();

    await expect(childCard.root).not.toBeVisible();

    await parentcard.open();

    await expect(childCard.root).toBeVisible();
});

async function uploadRandomAsset(assetsPage: AssetsPage) {
    const fileName = `file-${getRandomId()}`;
    const fileBuffer = await readFile(path.join(__dirname, '../../assets/logo-squared.png'));

    await assetsPage.uploadFile({ name: fileName, mimeType: 'image/png', buffer: fileBuffer });

    return fileName;
}