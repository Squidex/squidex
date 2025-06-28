/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { expect, Locator, Page } from '@playwright/test';

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

        return new AppDialog(this.page, this.page.getByTestId('dialog'));
    }

    public async createNewApp(appName: string) {
        await this.goto();
        const appDialog = await this.openAppDialog();
        await appDialog.enterName(appName);
        await appDialog.save();

        const newApp = this.page.getByRole('heading', { name: appName });
        await expect(newApp).toBeVisible();
    }
}

class AppDialog {
    constructor(private readonly page: Page,
        public readonly root: Locator,
    ) {
    }

    public async enterName(name: string) {
        await this.root.locator('#name').fill(name);
    }

    public async save() {
        await this.root.getByRole('button', { name: 'Create' }).click();
    }
}