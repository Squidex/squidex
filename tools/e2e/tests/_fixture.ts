/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { test as base, Page } from '@playwright/test';

type BaseFixture = {
    dropdown: Dropdown;
};

class Dropdown {
    constructor(
        private readonly page: Page,
    ) {
    }

    public async delete() {
        await this.page.getByText('Delete').click();
        await this.page.getByRole('button', { name: /Yes/ }).click();
    }

    public async action(name: string) {
        await this.page.getByText(name).click();
        await this.page.locator('sqx-dropdown-menu').waitFor({ state: 'hidden' });
    }
}

export const test = base.extend<BaseFixture>({
    dropdown: async ({ page }, use) => {
        const dropdown = new Dropdown(page);

        await use(dropdown);
    },
});

export { expect } from '@playwright/test';

