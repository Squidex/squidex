/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Locator, Page } from '@playwright/test';
import { escapeRegex } from '../utils';

export class SchemasPage {
    constructor(private readonly page: Page) {}

    public async goto(appName: string) {
        await this.page.goto(`/app/${appName}/schemas`);
    }

    public async getSchemaLink(schemaName: string) {
        const locator = this.page.locator('a.nav-link', { hasText: escapeRegex(schemaName) });

        return new SchemaLink(this.page, locator);
    }

    public async openSchemaDialog() {
        await this.page.getByLabel('Create Schema').click();

        return new CreateDialog(this.page);
    }
}

export class SchemaLink {
    constructor(private readonly page: Page,
        public readonly root: Locator,
    ) {
    }
}

export class CreateDialog {
    constructor(private readonly page: Page) {}

    public async enterName(name: string) {
        await this.page.getByLabel('Name (required)').fill(name);
    }

    public async save() {
        await this.page.getByRole('button', { name: 'Create', exact: true }).click();
    }
}