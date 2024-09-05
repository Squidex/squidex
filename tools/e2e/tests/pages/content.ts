/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Page } from '@playwright/test';
import { Dropdown } from './dropdown';

export class ContentPage {
    constructor(private readonly page: Page) {}

    public async back() {
        await this.page.getByLabel('Back').click();
    }

    public async enterField(value: string) {
        await this.page.locator('sqx-field-editor').getByRole('textbox').fill(value);
    }

    public async saveAndAdd() {
        await this.page.getByLabel('Save', { exact: true }).getByLabel('More').click();
        await this.page.getByText('Save & add another').click();
        await this.waitForCreation();
    }

    public async saveAndClose() {
        await this.page.getByLabel('Save', { exact: true }).getByLabel('More').click();
        await this.page.getByText('Save & close').click();
        await this.waitForCreation();
    }

    public async saveAndEdit() {
        await this.page.getByRole('button', { name: 'Save', exact: true }).click();
        await this.waitForCreation();
    }

    public async savePublishAndAdd() {
        await this.page.getByLabel('Save and Publish').getByLabel('More').click();
        await this.page.getByText('Save and Publish & add another').click();
        await this.waitForCreation();
    }

    public async savePublishAndClose() {
        await this.page.getByLabel('Save and Publish').getByLabel('More').click();
        await this.page.getByText('Save and Publish & close').click();
        await this.waitForCreation();
    }

    public async savePublishAndEdit() {
        await this.page.getByRole('button', { name: 'Save and Publish', exact: true }).click();
        await this.waitForCreation();
    }

    public async save() {
        await this.page.getByRole('button', { name: 'Save', exact: true }).click();
        await this.waitForCreation();
    }

    public async openStatusDropdown(status: string) {
        await this.page.getByRole('button', { name: status }).click();

        return new Dropdown(this.page);
    }

    public async openOptionsDropdown() {
        await this.page.getByLabel('Options').click();

        return new Dropdown(this.page);
    }

    private async waitForCreation() {
        await this.page.getByRole('alert').getByText('Content created successfully.').waitFor({ state: 'visible' });
    }
}