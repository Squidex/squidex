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

test('should upload asset', async ({ assetsPage }) => {
    const assetName = await uploadRandomAsset(assetsPage);
    const assetCard = await assetsPage.getAssetCard(assetName);

    expect(assetCard.root).toBeVisible();
});

test('should delete asset', async ({ assetsPage }) => {
    const assetName = await uploadRandomAsset(assetsPage);
    const assetCard = await assetsPage.getAssetCard(assetName);
    await assetCard.delete();

    expect(assetCard.root).not.toBeVisible();
});

test('should not delete asset if cancelled', async ({ assetsPage }) => {
    const assetName = await uploadRandomAsset(assetsPage);
    const assetCard = await assetsPage.getAssetCard(assetName);

    await assetCard.delete(true);

    expect(assetCard.root).toBeVisible();
});

test('should edit asset name', async ({ assetsPage }) => {
    const assetName = await uploadRandomAsset(assetsPage);
    const assetCard = await assetsPage.getAssetCard(assetName);

    const newName = `file-${getRandomId()}`;
    const assetDialog = await assetCard.edit();
    await assetDialog.enterName(newName);
    await assetDialog.save();

    const newCard = await assetsPage.getAssetCard(newName);

    expect(newCard.root).toBeVisible();
});

test('should edit asset metadata', async ({ assetsPage }) => {
    const assetName = await uploadRandomAsset(assetsPage);
    const assetCard = await assetsPage.getAssetCard(assetName);

    const w = '42';
    const h = '13';

    const assetDialog = await assetCard.edit();
    await assetDialog.enterMetadata('pixelWidth', w);
    await assetDialog.enterMetadata('pixelHeight', h);
    await assetDialog.save();

    const newCard = await assetsPage.getAssetCard(`${w}x${h}px`);

    expect(newCard.root).toBeVisible();
});

async function uploadRandomAsset(assetsPage: AssetsPage) {
    const fileName = `file-${getRandomId()}`;
    const fileBuffer = await readFile(path.join(__dirname, '../../assets/logo-squared.png'));

    await assetsPage.uploadFile({ name: fileName, mimeType: 'image/png', buffer: fileBuffer });

    return fileName;
}