/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { expect, test } from '../_fixture';
import { getRandomId } from '../utils';

test.beforeEach(async ({ appsPage, settingsPage }) => {
    const appName = `app-${getRandomId()}`;
    await appsPage.createNewApp(appName);

    await settingsPage.goto(appName);
});

test('has header', async ({ page }) => {
    const header = page.getByRole('heading', { name: /UI Settings/ });

    // See: https://viralsfire.com/post/playwright-to-be-visible-timeout-is-ignored
    expect(header).toBeVisible({ timeout: 5000 });
});

test('add pattern', async ({ settingsPage })=> {
    const patternName = `name-${getRandomId()}`;
    const patternRegex = `regex-${getRandomId()}`;

    const newRow = await settingsPage.getPatternNewRow();
    await newRow.enterName(patternName);
    await newRow.enterRegex(patternRegex);
    await settingsPage.save();

    const patternRow = await settingsPage.getPatternRow(patternName);

    expect(patternRow.root).toBeVisible();
});

test('delete pattern', async ({ settingsPage })=> {
    const patternRow = await settingsPage.getPatternRow('Email');
    await patternRow.delete();
    await settingsPage.save();

    expect(patternRow.root).not.toBeVisible();
});