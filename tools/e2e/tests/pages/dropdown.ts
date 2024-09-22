/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Page } from '@playwright/test';

export class Dropdown {
    constructor(private readonly page: Page) {}

    public async delete(button = /Yes/) {
        await this.actionAndConfirm('Delete', button);
    }

    public async action(name: string) {
        await this.page.getByText(name).click();
        await this.page.locator('sqx-dropdown-menu').waitFor({ state: 'hidden' });
    }

    public async actionAndConfirm(name: string, button = /Yes/) {
        await this.action(name);
        await this.page.getByRole('button', { name: button }).click();
    }
}