/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Locator, Page } from '@playwright/test';
import { escapeRegex } from '../utils';

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

    public async getAssetCard(name: string) {
        const locator = this.page.locator('sqx-asset', { hasText: escapeRegex(name) });

        return new AssetCard(this.page, locator);
    }
}

export class AssetCard {
    constructor(private readonly page: Page,
        public readonly root: Locator,
    ) {
    }

    public async delete(cancel = false) {
        await this.root.getByLabel('Delete').click();

        if (cancel) {
            await this.page.getByRole('button', { name: /No/ }).click();
        } else {
            await this.page.getByRole('button', { name: /Yes/ }).click();
        }
    }

    public async edit() {
        await this.root.getByLabel('Edit').click();

        return new AssetDialog(this.page, this.page.getByTestId('dialog'));
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