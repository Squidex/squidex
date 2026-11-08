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
        // The onboarding dialog is only shown when the user has no apps yet, so skip it whenever it blocks an action.
        const onboarding = this.page.locator('sqx-onboarding-dialog');

        // goto() can be called multiple times for the same page.
        await this.page.removeLocatorHandler(onboarding);
        await this.page.addLocatorHandler(onboarding, async () => {
            await onboarding.getByRole('button', { name: /Skip/ }).click();
        });

        await this.page.goto('/app');

        // The app keeps polling, therefore the network is never idle. Wait for the page content instead.
        await expect(this.page.getByTestId('new-app').first()).toBeVisible();
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