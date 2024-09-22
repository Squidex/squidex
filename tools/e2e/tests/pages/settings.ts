/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Locator, Page } from '@playwright/test';

export class SettingsPage {
    constructor(private readonly page: Page) {}

    public async goto(appName: string) {
        await this.page.goto(`/app/${appName}/settings/settings`);
    }

    public async save() {
        await this.page.getByRole('button', { name: 'Save' }).click();
    }

    public async getPatternRow(name: string) {
        const locator = this.page.getByTestId(`pattern_${name}`);

        return new PatternRow(this.page, locator);
    }

    public async getPatternNewRow() {
        const locator = this.page.getByTestId(/pattern_/).last();

        return new PatternRow(this.page, locator);
    }

    public async addPattern() {
        await this.page.getByTestId('patterns').getByRole('button', { name: 'Add' }).click();
    }
}

export class PatternRow {
    constructor(private readonly page: Page,
        public readonly root: Locator,
    ) {
    }

    public async enterName(value: string) {
        await this.root.getByPlaceholder('Name').fill(value);
    }

    public async enterRegex(value: string) {
        await this.root.getByPlaceholder('Pattern').fill(value);
    }

    public async delete() {
        await this.root.getByRole('button', { name: 'Delete' }).click();
        await this.page.getByRole('button', { name: 'Yes' }).click();
    }
}