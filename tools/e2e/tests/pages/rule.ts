/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Locator, Page } from '@playwright/test';

export class RulePage {
    constructor(private readonly page: Page) {}

    public async enterName(name: string) {
        await this.page.getByRole('textbox', { name: 'Name' }).fill(name);
    }

    public async addTrigger() {
        await this.page.getByLabel('Add Trigger').click();

        return new TriggerDialog(this.page, this.page.getByTestId('dialog'));
    }

    public async addStep() {
        await this.page.getByLabel('Add Step').click();

        return new StepDialog(this.page, this.page.getByTestId('dialog'));
    }

    public async save() {
        await this.page.getByRole('button', { name: 'Save' }).click();
        await this.page.getByText('Enabled').waitFor({ state: 'visible' });
    }

    public async back() {
        await this.page.getByLabel('Back').click();
    }
}

export class TriggerDialog {
    constructor(private readonly page: Page,
        public readonly root: Locator,
    ) {
    }

    public async selectContentChangedTrigger() {
        await this.page.getByText('Content changed').click();
    }

    public async add() {
        await this.root.getByRole('button', { name: 'Add' }).click();
    }

    public async save() {
        await this.root.getByRole('button', { name: 'Save' }).click();
    }
}

export class StepDialog {
    constructor(private readonly page: Page,
        public readonly root: Locator,
    ) {
    }

    public async selectWebhookAction() {
        await this.page.getByText('Webhook').click();
        await this.page.locator('sqx-formattable-input').first().getByRole('textbox').fill('https:/squidex.io');
    }

    public async add() {
        await this.root.getByRole('button', { name: 'Add' }).click();
    }

    public async save() {
        await this.root.getByRole('button', { name: 'Save' }).click();
    }
}