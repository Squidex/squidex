/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Page } from '@playwright/test';

export class AppsPage {
    constructor(private readonly page: Page) {}

    public async goto() {
        await this.page.goto('/app');
    }

    public async gotoApp(name: string) {
        await this.page.getByRole('heading', { name }).click();
    }

    public async openAppDialog() {
        await this.page.getByTestId('new-app').click();

        return new AppDialog(this.page);
    }
}

class AppDialog {
    constructor(private readonly page: Page) {}

    public async enterName(name: string) {
        await this.page.locator('#name').fill(name);
    }

    public async save() {
        await this.page.getByRole('button', { name: 'Create' }).click();
    }
}