/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Locator, Page } from '@playwright/test';
import { escapeRegex } from '../utils';

export class LanguagesPage {
    constructor(private readonly page: Page) {}

    public async goto(appName: string) {
        await this.page.goto(`/app/${appName}/settings/languages`);
    }

    public async enterLanguage(input: string) {
        await this.page.locator('#language').fill(input);
    }

    public async selectLanguage(selection: string) {
        await this.page.getByText(selection).click();
    }

    public async save() {
        await this.page.getByRole('button', { name: 'Add Language' }).click();
    }

    public async getLanguageCard(name: string) {
        const locator = this.page.locator('sqx-language', { hasText: escapeRegex(name) });

        return new LanguageCard(this.page, locator);
    }
}

export class LanguageCard {
    constructor(private readonly page: Page,
        public readonly root: Locator,
    ) {
    }

    public async delete(button = /Yes/) {
        await this.root.getByLabel('Delete').click();
        await this.page.getByRole('button', { name: button }).click();
    }

    public async toggle() {
        await this.root.getByLabel('Options').click();
    }

    public async save() {
        await this.root.getByRole('button', { name: 'Save' }).click();
    }

    public async makeMaster() {
        await this.root.getByLabel('Is Master').check();
    }

    public async addFallbackLanguage(language: string) {
        await this.root.getByLabel('Fallback').selectOption({ label: language });
        await this.root.getByRole('button', { name: 'Add Language' }).click();
    }
}