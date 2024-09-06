/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Page } from '@playwright/test';

export class LoginPage {
    constructor(private readonly page: Page) {}

    public async goto() {
        await this.page.goto('/');
    }

    public async openPopup() {
        const popupPromise = this.page.waitForEvent('popup');

        await this.page.getByTestId('login').click();

        const popup = await popupPromise;
        await popup.waitForLoadState();

        await popup.getByTestId('login-button').waitFor();
        return new LoginPopup(popup);
    }
}

export class LoginPopup {
    constructor(
        public readonly root: Page,
    ) {
    }

    public async enterEmail(email: string) {
        await this.root.getByPlaceholder('Enter Email').fill(email);
    }

    public async enterPassword(password: string) {
        await this.root.getByPlaceholder('Enter Password').fill(password);
    }

    public async login() {
        await this.root.getByTestId('login-button').click();
    }
}