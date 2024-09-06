/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Page } from '@playwright/test';

export class Dropdown {
    constructor(private readonly page: Page) {}

    public async delete(cancel = false) {
        await this.page.getByText('Delete').click();

        if (cancel) {
            await this.page.getByRole('button', { name: /No/ }).click();
        } else {
            await this.page.getByRole('button', { name: /Yes/ }).click();
        }
    }

    public async action(name: string) {
        await this.page.getByText(name).click();
        await this.page.locator('sqx-dropdown-menu').waitFor({ state: 'hidden' });
    }

    public async actionConfirm(name: string) {
        await this.action(name);
        await this.page.getByRole('button', { name: 'Confirm' }).click();
    }
}