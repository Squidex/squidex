/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Locator, Page } from '@playwright/test';
import { expect, test } from './_fixture';

type VisualPage = {
    name: string;
    url: string;
    intercept?: (page: Page, appName: string) => Promise<void>;
    prepare?: (page: Page) => Promise<void>;
    mask?: (page: Page) => Locator[];

    // The selector of an inner container that scrolls, because a full page screenshot does not include it.
    scroll?: string;

    // False, if the page renders more content when it is scrolled and would never be stable.
    fullPage?: boolean;
};

const pages: VisualPage[] = [
    {
        name: 'apps',
        url: '/app',
        intercept: async (page, appName) => {
            // Other tests create apps, therefore only show the visual app.
            await page.route('**/api/apps', async route => {
                const response = await route.fetch();
                const apps = await response.json();

                await route.fulfill({ response, json: apps.filter((x: any) => x.name === appName) });
            });
        },
    },
    {
        name: 'dashboard',
        url: '',
        mask: page => [page.locator('sqx-dashboard-page canvas, sqx-dashboard-page .event-created')],
    },
    {
        name: 'schemas',
        url: 'schemas',
    },
    {
        name: 'schema',
        url: 'schemas/visual-schema',
    },
    {
        name: 'schema-field-dialog',
        url: 'schemas/visual-schema',
        prepare: async page => {
            await page.getByRole('button', { name: /Add Field/ }).first().click();
            await page.getByTestId('dialog').waitFor();
        },
    },
    {
        name: 'contents',
        url: 'content/visual-schema',
        mask: page => [page.locator('sqx-content-list-field small')],
    },
    {
        name: 'content-new',
        url: 'content/visual-schema/new',
    },
    {
        name: 'content-editors',
        url: 'content/visual-editors/new',
        scroll: '.list-content',
        mask: page => [page.locator('sqx-geolocation-editor')],
    },
    {
        name: 'assets',
        url: 'assets',
    },
    {
        name: 'rules',
        url: 'rules',
    },
    {
        name: 'rule-new',
        url: 'rules/new',
    },
    {
        name: 'api',
        url: 'api',
    },
    {
        name: 'api-graphql',
        url: 'api/graphql',
        prepare: async page => {
            await page.locator('.graphiql-container').waitFor();
        },
    },
    {
        name: 'settings-more',
        url: 'settings/more',
    },
    {
        name: 'settings-clients',
        url: 'settings/clients',
        mask: page => [page.locator('sqx-client input')],
    },
    {
        name: 'settings-contributors',
        url: 'settings/contributors',
    },
    {
        name: 'settings-languages',
        url: 'settings/languages',
    },
    {
        name: 'settings-roles',
        url: 'settings/roles',
    },
    {
        name: 'settings-workflows',
        url: 'settings/workflows',
    },
    {
        name: 'settings-settings',
        url: 'settings/settings',
    },
    {
        name: 'settings-plans',
        url: 'settings/plans',
    },
    {
        name: 'settings-backups',
        url: 'settings/backups',
    },
    {
        name: 'settings-jobs',
        url: 'settings/jobs',
    },
    {
        name: 'settings-asset-scripts',
        url: 'settings/asset-scripts',
    },
    {
        name: 'settings-script-logs',
        url: 'settings/script-logs',
    },
    {
        name: 'administration-users',
        url: '/app/administration/users',
        intercept: async page => {
            // Other tests create users with random names, therefore only show the admin user.
            await page.route(/\/api\/user-management(\?.*)?$/, async route => {
                const response = await route.fetch();
                const users = await response.json();

                const items = users.items.filter((x: any) => x.email === 'hello@squidex.io');

                await route.fulfill({ response, json: { ...users, items, total: items.length } });
            });
        },
    },
    {
        name: 'administration-event-consumers',
        url: '/app/administration/event-consumers',
        mask: page => [page.locator('sqx-event-consumers-page tbody')],
    },
    {
        name: 'administration-restore',
        url: '/app/administration/restore',
    },
    {
        name: 'identity-login',
        url: '/identity-server/account/login',
    },
    {
        name: 'identity-profile',
        url: '/identity-server/account/profile',
    },
    {
        name: 'api-docs',
        url: '/api/docs',
        fullPage: false,
        prepare: async page => {
            await page.locator('#redoc-container h1').first().waitFor();
        },
    },
];

for (const { name, url, intercept, prepare, mask, scroll, fullPage } of pages) {
    test(name, async ({ page, appName }) => {
        if (intercept) {
            await intercept(page, appName);
        }

        // Absolute URLs are not scoped to the app.
        await page.goto(url.startsWith('/') ? url : `/app/${appName}/${url}`);
        await page.waitForLoadState('networkidle');

        if (prepare) {
            await prepare(page);
        }

        if (scroll) {
            await resizeToContent(page, scroll);
        }

        await expect(page).toHaveScreenshot(`${name}.png`, {
            animations: 'disabled',
            caret: 'hide',
            fullPage: fullPage !== false,
            mask: mask?.(page),
            maxDiffPixelRatio: 0.01,
        });
    });
}

// Screenshots do not include the content of inner scroll containers, therefore the window is resized to fit.
async function resizeToContent(page: Page, selector: string) {
    const viewport = page.viewportSize()!;

    const hiddenHeight = await page.locator(selector).evaluate(element => element.scrollHeight - element.clientHeight);

    if (hiddenHeight > 0) {
        await page.setViewportSize({ width: viewport.width, height: Math.min(viewport.height + hiddenHeight, 8000) });
        await page.waitForLoadState('networkidle');
    }
}
