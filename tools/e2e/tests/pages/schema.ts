/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { expect, Locator, Page } from '@playwright/test';
import { escapeRegex } from '../utils';
import { Dropdown } from './dropdown';

export class SchemaPage {
    constructor(private readonly page: Page) {}

    public async goto(appName: string, schemaName: string) {
        await this.page.goto(`/app/${appName}/schemas/${schemaName}`);
    }

    public async addField() {
        await this.page.locator('button').filter({ hasText: /^Add Field$/ }).click();

        return new FieldDialog(this.page);
    }

    public async addNestedField() {
        await this.page.locator('button').filter({ hasText: /Add Nested Field/ }).click();

        return new FieldDialog(this.page);
    }

    public async openOptionsDropdown() {
        await this.page.getByLabel('Options').click();

        return new Dropdown(this.page);
    }

    public async getFieldRow(fieldName: string) {
        const locator = this.page.locator('div.table-items-row-summary', { hasText: escapeRegex(fieldName) });

        return new FieldRow(this.page, locator);
    }

    public async publish() {
        const button = this.page.getByRole('button', { name: 'Published', exact: true });
        await button.click();

        await expect(button).toBeDisabled();
    }

    public async unpublish() {
        const button = this.page.getByRole('button', { name: 'Unpublished', exact: true });
        await button.click();

        await expect(button).toBeDisabled();
    }
}

export class FieldRow {
    constructor(private readonly page: Page,
        public readonly root: Locator,
    ) {
    }

    public async openOptionsDropdown() {
        await this.root.getByLabel('Options').click();

        return new Dropdown(this.page);
    }
}

export class FieldDialog {
    constructor(private readonly page: Page) {}

    public async enterName(name: string) {
        await this.page.getByPlaceholder('Enter field name').fill(name);
    }

    public async enterType(type: string) {
        await this.page.getByText(type, { exact: true }).click();
    }

    public async enterLabel(label: string) {
        await this.page.getByLabel('Label').fill(label);
    }

    public async createAndClose() {
        await this.page.getByTestId('dialog').getByRole('button', { name: 'Create' }).click();
    }

    public async createAndAdd() {
        await this.page.getByTestId('dialog').getByLabel('Add field').getByLabel('More').click();
        await this.page.getByText('Create & add another').click();
    }

    public async createAndEdit() {
        await this.page.getByTestId('dialog').getByLabel('Add field').getByLabel('More').click();
        await this.page.getByText('Create & edit properties').click();
    }

    public async saveAndClose() {
        await this.page.getByTestId('dialog').getByRole('button', { name: 'Save and close' }).click();
    }

    public async saveAndAdd() {
        await this.page.getByTestId('dialog').getByLabel('Save field').getByLabel('More').click();
        await this.page.getByText('Save and add field').click();
    }
}