/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Locator, Page } from '@playwright/test';
import { escapeRegex } from '../utils';
import { Dropdown } from './dropdown';

export class ContentsPage {
    constructor(private readonly page: Page) {}

    public async goto(appName: string, schemaName: string) {
        await this.page.goto(`/app/${appName}/content/${schemaName}`);
    }

    public async increasePageSize() {
        await this.page.getByRole('combobox').selectOption('3: 50');
    }

    public async addContent() {
        await this.page.getByRole('button', { name: /New/ }).click();
    }

    public async changeSelectedStatus(status: string) {
        await this.page.getByRole('button', { name: status }).click();
        await this.page.getByRole('button', { name: 'Confirm' }).click();
    }

    public async deleteSelected() {
        await this.page.getByRole('button', { name: 'Delete' }).click();
        await this.page.getByRole('button', { name: 'Yes' }).click();
    }

    public async getContentRow(text: string) {
        const locator = this.page.locator('tr', { hasText: escapeRegex(text) });

        return new ContentRow(this.page, locator);
    }
}

export class ContentRow {
    constructor(private readonly page: Page,
        public readonly root: Locator,
    ) {
    }

    public async edit() {
        await this.root.click();
    }

    public async select() {
        await this.root.getByRole('checkbox').click();
    }

    public async openOptionsDropdown() {
        await this.root.getByLabel('Options').click();

        return new Dropdown(this.page);
    }
}