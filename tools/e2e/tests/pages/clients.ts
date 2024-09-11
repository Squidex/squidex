/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Locator, Page } from '@playwright/test';
import { escapeRegex } from '../utils';
import { RenameDialog } from './rename';

export class ClientsPage {
    constructor(private readonly page: Page) {}

    public async goto(appName: string) {
        await this.page.goto(`/app/${appName}/settings/clients`);
    }

    public async enterClientId(input: string) {
        await this.page.getByPlaceholder('Enter client name').fill(input);
    }

    public async save() {
        await this.page.getByRole('button', { name: 'Add Client' }).click();
    }

    public async getClientCard(name: string) {
        const locator = this.page.locator('sqx-client', { hasText: escapeRegex(name) });

        return new ClientCard(this.page, locator);
    }
}

class ClientCard {
    constructor(private readonly page: Page,
        public readonly root: Locator,
    ) {
    }

    public async copyClientId() {
        await this.root.getByLabel('Copy Client ID').click();
    }

    public async copyClientSecret() {
        await this.root.getByLabel('Copy Client Secret').click();
    }

    public async startRenameDblClick() {
        await this.root.getByRole('heading').first().dblclick();

        return new RenameDialog(this.page);
    }

    public async startRenameButton() {
        await this.root.getByRole('heading').hover();
        await this.root.getByLabel('Rename').click();

        return new RenameDialog(this.page);
    }
}