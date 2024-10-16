/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Page } from '@playwright/test';

export class RulePage {
    constructor(private readonly page: Page) {}

    public async selectContentChangedTrigger() {
        await this.page.getByText('Content changed').click();
    }

    public async selectWebhookAction() {
        await this.page.getByText('Webhook').click();
        await this.page.locator('sqx-formattable-input').first().getByRole('textbox').fill('https:/squidex.io');
    }

    public async save() {
        await this.page.getByRole('button', { name: 'Save' }).click();
        await this.page.getByText('Enabled').waitFor({ state: 'visible' });
    }

    public async back() {
        await this.page.getByLabel('Back').click();
    }
}