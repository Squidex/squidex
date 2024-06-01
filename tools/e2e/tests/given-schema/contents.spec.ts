/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Page } from '@playwright/test';
import { escapeRegex, getRandomId } from '../utils';
import { expect, test } from './_fixture';

test.beforeEach(async ({ page, appName, schemaName }) => {
    await page.goto(`/app/${appName}/content/${schemaName}`);
});

test('create content', async ({ page }) => {
    const contentText = await createRandomContent(page);
    const contentRow = page.locator('tr', { hasText: escapeRegex(contentText) });

    await expect(contentRow).toBeVisible();
});

test('create content as published', async ({ page }) => {
    const contentText = await createRandomContent(page, true);
    const contentRow = page.locator('tr', { hasText: escapeRegex(contentText) });

    await expect(contentRow.getByLabel('Published')).toBeVisible();
});

test('update content', async ({ page }) => {
    const contentText = await createRandomContent(page);
    const contentRow = page.locator('tr', { hasText: escapeRegex(contentText) });

    await contentRow.click();

    const contentUpdate = `content-${getRandomId()}`;

    // Define content value.
    await page.locator('sqx-field-editor').getByRole('textbox').fill(contentUpdate);
    await saveContent(page, false);

    await page.getByRole('button', { name: 'Save', exact: true }).click();

    // Wait for update of the version
    await page.getByText('Version: 1').waitFor({ state: 'visible' });

    // Go back
    await page.getByLabel('Back').click();

    const updateRow = page.locator('tr', { hasText: escapeRegex(contentUpdate) });

    await expect(updateRow).toBeVisible();
});

const states = [{
    state: 'Archived',
    currentState: 'Draft',
    initialPublished: false,
}, {
    state: 'Draft',
    currentState: 'Published',
    initialPublished: true,
}, {
    state: 'Published',
    currentState: 'Draft',
    initialPublished: false,
}];

states.forEach(({ state, currentState, initialPublished }) => {
    test(`change content to ${state}`, async ({ dropdown, page }) => {
        const contentText = await createRandomContent(page, initialPublished);
        const contentRow = page.locator('tr', { hasText: escapeRegex(contentText) });

        await contentRow.getByLabel('Options').click();
        await dropdown.action(`Change to ${state}`);
        await page.getByRole('button', { name: 'Confirm' }).click();

        await expect(contentRow.getByLabel(state)).toBeVisible();
    });

    test(`change content to ${state} by checkbox`, async ({ page }) => {
        const contentText = await createRandomContent(page, initialPublished);
        const contentRow = page.locator('tr', { hasText: escapeRegex(contentText) });

        await contentRow.getByRole('checkbox').click();
        await page.getByRole('button', { name: state }).click();
        await page.getByRole('button', { name: 'Confirm' }).click();

        await expect(contentRow.getByLabel(state)).toBeVisible();
    });

    test(`change content to ${state} by detail page`, async ({ page }) => {
        const contentText = await createRandomContent(page, initialPublished);
        const contentRow = page.locator('tr', { hasText: escapeRegex(contentText) });

        await contentRow.click();
        await page.getByRole('button', { name: currentState }).click();
        await page.getByText(`Change to ${state}`).click();
        await page.getByRole('button', { name: 'Confirm' }).click();

        await expect(page.getByRole('button', { name: state })).toBeVisible();
    });
});

test('delete content', async ({ dropdown, page }) => {
    const contentText = await createRandomContent(page);
    const contentRow = page.locator('tr', { hasText: escapeRegex(contentText) });

    await contentRow.getByLabel('Options').click();
    await dropdown.delete();

    await expect(contentRow).not.toBeVisible();
});

test('delete content by checkbox', async ({  page }) => {
    const contentText = await createRandomContent(page);
    const contentRow = page.locator('tr', { hasText: escapeRegex(contentText) });

    await contentRow.getByRole('checkbox').click();
    await page.getByRole('button', { name: 'Delete' }).click();
    await page.getByRole('button', { name: 'Yes' }).click();

    await expect(contentRow).not.toBeVisible();
});

async function createRandomContent(page: Page, publish = false) {
    const contentText = `content-${getRandomId()}`;

    await page.getByRole('button', { name: /New/ }).click();

    // Define content value.
    await page.locator('sqx-field-editor').getByRole('textbox').fill(contentText);
    await saveContent(page, publish);

    // Wait for the success alert.
    await page.getByLabel('Identity').waitFor({ state: 'visible' });

    // Go back
    await page.getByLabel('Back').click();

    return contentText;
}

async function saveContent(page: Page, publish: boolean) {
    if (publish) {
        await page.getByRole('button', { name: 'Save and Publish', exact: true }).click();
    } else {
        await page.getByRole('button', { name: 'Save', exact: true }).click();
    }
}
