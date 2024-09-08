/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Locator, Page } from '@playwright/test';
import { escapeRegex } from '../utils';
import { Dropdown } from './dropdown';

export class AssetsPage {
    constructor(private readonly page: Page) {}

    public async goto(appName: string) {
        await this.page.goto(`/app/${appName}/assets`);
    }

    public async uploadFile(file: { name: string; mimeType: string; buffer: Buffer }) {
        const fileChooserPromise = this.page.waitForEvent('filechooser');

        await this.page.getByText(/Drop files here to upload/).locator('..').click();

        const fileChooser = await fileChooserPromise;
        await fileChooser.setFiles(file);
    }

    public async openAssetFolderDialog() {
        await this.page.getByLabel('Create Folder').click();

        return new AssetFolderDialog(this.page, this.page.getByTestId('dialog'));
    }

    public async getAssetCard(name: string) {
        const locator = this.page.locator('sqx-asset', { hasText: escapeRegex(name) });

        return new AssetCard(this.page, locator);
    }

    public async getAssetFolderCard(name: string) {
        const locator = this.page.locator('sqx-asset-folder', { hasText: escapeRegex(name) });

        return new AssetFolderCard(this.page, locator);
    }
}

export class AssetCard {
    constructor(private readonly page: Page,
        public readonly root: Locator,
    ) {
    }

    public async delete(button = /Yes/) {
        await this.root.getByLabel('Delete').click();
        await this.page.getByRole('button', { name: button }).click();
    }

    public async edit() {
        await this.root.getByLabel('Edit').click();

        return new AssetDialog(this.page, this.page.getByTestId('dialog'));
    }
}

export class AssetFolderCard {
    constructor(private readonly page: Page,
        public readonly root: Locator,
    ) {
    }

    public async open() {
        await this.root.dblclick();
    }

    public async rename() {
        const dropdown = await this.openOptionsDropdown();
        await dropdown.action('Rename');

        return new AssetFolderDialog(this.page, this.page.getByTestId('dialog'));
    }

    public async openOptionsDropdown() {
        await this.root.getByLabel('Options').click();

        return new Dropdown(this.page);
    }
}

export class AssetDialog {
    constructor(private readonly page: Page,
        public readonly root: Locator,
    ) {
    }

    public async enterName(name: string) {
        await this.root.getByLabel('Name').fill(name);
    }

    public async enterMetadata(name: string, value: string) {
        const rows = this.root.getByPlaceholder('Name');
        await rows.first().waitFor({ state: 'attached' });

        for (const row of await rows.all()) {
            if (await row.inputValue() === name) {
                await row.locator('../..').locator('input').nth(1).fill(value);
                return;
            }
        }

        throw new Error(`Cannot find row with name '${name}'`);
    }

    public async save() {
        await this.root.getByRole('button', { name: 'Save' }).click();
        await this.page.getByText('Asset has been updated.').waitFor({ state: 'visible' });
        await this.root.getByLabel('Close').click();

    }
}

export class AssetFolderDialog {
    constructor(private readonly page: Page,
        public readonly root: Locator,
    ) {
    }

    public async enterName(name: string) {
        await this.root.getByLabel('Folder Name (required)').fill(name);
    }

    public async save() {
        await this.root.getByRole('button', { name: 'Create' }).click();
    }

    public async rename() {
        await this.root.getByRole('button', { name: 'Rename' }).click();
    }
}