/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { expect, test } from '../_fixture';
import { getRandomId } from '../utils';

test.beforeEach(async ({ appsPage, settingsPage }) => {
    const appName = `my-app-${getRandomId()}`;

    await appsPage.goto();
    const appDialog = await appsPage.openAppDialog();
    await appDialog.enterName(appName);
    await appDialog.save();

    await settingsPage.goto(appName);
});

test('has header', async ({ page }) => {
    const header = page.getByRole('heading', { name: /UI Settings/ });

    expect(header).toBeVisible();
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