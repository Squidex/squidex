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

    public async openFieldWizard() {
        await this.page.locator('button').filter({ hasText: /^Add Field$/ }).click();

        return new FieldDialog(this.page, this.page.getByTestId('dialog'));
    }

    public async openNestedFieldWizard() {
        await this.page.locator('button').filter({ hasText: /Add Nested Field/ }).click();

        return new FieldDialog(this.page, this.page.getByTestId('dialog'));
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
    constructor(private readonly page: Page,
        public readonly root: Locator,
    ) {}

    public async enterName(name: string) {
        await this.root.getByPlaceholder('Enter field name').fill(name);
    }

    public async enterType(type: string) {
        await this.root.getByText(type, { exact: true }).click();
    }

    public async enterLabel(label: string) {
        await this.root.getByLabel('Label').fill(label);
    }

    public async createAndClose() {
        await this.root.getByRole('button', { name: 'Create' }).click();
    }

    public async createAndAdd() {
        await this.root.getByLabel('Add field').getByLabel('More').click();

        const dropdown = new Dropdown(this.page);
        await dropdown.action('Create & add another');
    }

    public async createAndEdit() {
        await this.root.getByLabel('Add field').getByLabel('More').click();

        const dropdown = new Dropdown(this.page);
        await dropdown.action('Create & edit properties');
    }

    public async saveAndClose() {
        await this.root.getByRole('button', { name: 'Save and close' }).click();
    }

    public async saveAndAdd() {
        await this.root.getByLabel('Save field').getByLabel('More').click();

        const dropdown = new Dropdown(this.page);
        await dropdown.action('Save and add field');
    }
}