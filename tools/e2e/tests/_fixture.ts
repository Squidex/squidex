/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { test as base } from '@playwright/test';
import { AssetsPage, ContentPage, ContentsPage, LoginPage, RulePage, RulesPage, SchemaPage, SchemasPage } from './pages';
import { AppsPage } from './pages/apps';

export type BaseFixture = {
    appsPage: AppsPage;
    assetsPage: AssetsPage;
    contentPage: ContentPage;
    contentsPage: ContentsPage;
    loginPage: LoginPage;
    rulePage: RulePage;
    rulesPage: RulesPage;
    schemaPage: SchemaPage;
    schemasPage: SchemasPage;
};

export const test = base.extend<BaseFixture>({
    appsPage: async ({ page }, use) => {
        await use(new AppsPage(page));
    },
    assetsPage: async ({ page }, use) => {
        await use(new AssetsPage(page));
    },
    contentPage: async ({ page }, use) => {
        await use(new ContentPage(page));
    },
    contentsPage: async ({ page }, use) => {
        await use(new ContentsPage(page));
    },
    loginPage: async ({ page }, use) => {
        await use(new LoginPage(page));
    },
    rulePage: async ({ page }, use) => {
        await use(new RulePage(page));
    },
    rulesPage: async ({ page }, use) => {
        await use(new RulesPage(page));
    },
    schemaPage: async ({ page }, use) => {
        await use(new SchemaPage(page));
    },
    schemasPage: async ({ page }, use) => {
        await use(new SchemasPage(page));
    },
});

export { expect } from '@playwright/test';

