/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Locator, Page } from '@playwright/test';

export class RenameDialog {
    public root: Locator;

    constructor(private readonly page: Page) {
        this.root = this.page.locator('sqx-editable-title');
    }

    public async enterName(name: string) {
        await this.root.locator('form').getByRole('textbox').fill(name);
    }

    public async save() {
        await this.root.locator('form').getByLabel('Save').click();
    }
}