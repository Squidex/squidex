/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Locator, Page } from '@playwright/test';
import { escapeRegex } from '../utils';
import { Dropdown } from './dropdown';
import { RenameDialog } from './rename';

export class RulesPage {
    constructor(private readonly page: Page) {}

    public async goto(appName: string) {
        await this.page.goto(`/app/${appName}/rules`);
    }

    public async addRule() {
        await this.page.getByRole('link', { name: /New Rule/ }).click();
    }

    public async getRuleCard(name: string) {
        const locator = this.page.locator('div.card', { hasText: escapeRegex(name) });

        return new RuleCard(this.page, locator);
    }
}

export class RuleCard {
    constructor(private readonly page: Page,
        public readonly root: Locator,
    ) {
    }

    public async openOptionsDropdown() {
        await this.root.getByLabel('Options').click();

        return new Dropdown(this.page);
    }

    public async startRenameDblClick() {
        await this.root.getByRole('heading').first().dblclick();

        return new RenameDialog(this.page);
    }

    public async startRenameButton() {
        await this.root.getByLabel('Rename').click();

        return new RenameDialog(this.page);
    }
}