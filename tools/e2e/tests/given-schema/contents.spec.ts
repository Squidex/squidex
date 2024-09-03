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
    await page.getByRole('combobox').selectOption('3: 50');
});

test('create content and close', async ({ page }) => {
    const contentText = await createRandomContent(page, 'SaveAndClose');
    const contentRow = page.locator('tr', { hasText: escapeRegex(contentText) });

    await expect(contentRow).toBeVisible();
});

test('create content and edit', async ({ page }) => {
    await createRandomContent(page, 'SaveAndEdit');

    await expect(page.getByRole('button', { name: /Draft/ })).toBeVisible();
    await expect(page.getByLabel('Identity')).toBeVisible();
});

test('create content and add another', async ({ page }) => {
    await createRandomContent(page, 'SaveAndAdd');

    await expect(page.locator('sqx-field-editor').getByRole('textbox')).toBeEmpty();
});

test('create content as published and edit', async ({ page }) => {
    await createRandomContent(page, 'SavePublishAndEdit');

    await expect(page.getByRole('button', { name: /Published/ })).toBeVisible();
    await expect(page.getByLabel('Identity')).toBeVisible();
});

test('create content as published and close', async ({ page }) => {
    const contentText = await createRandomContent(page, 'SavePublishAndClose');
    const contentRow = page.locator('tr', { hasText: escapeRegex(contentText) });

    await expect(contentRow.getByLabel('Published')).toBeVisible();
});

test('update content', async ({ page }) => {
    const contentText = await createRandomContent(page, 'SaveAndClose');
    const contentRow = page.locator('tr', { hasText: escapeRegex(contentText) });

    await contentRow.click();

    const contentUpdate = `content-${getRandomId()}`;

    // Define content value.
    await page.locator('sqx-field-editor').getByRole('textbox').fill(contentUpdate);
    await saveContent(page, 'Save');

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
    const mode: SaveMode = initialPublished ? 'SavePublishAndClose' : 'SaveAndClose';

    test(`change content to ${state}`, async ({ dropdown, page }) => {
        const contentText = await createRandomContent(page, mode);
        const contentRow = page.locator('tr', { hasText: escapeRegex(contentText) });

        await contentRow.getByLabel('Options').click();
        await dropdown.action(`Change to ${state}`);
        await page.getByRole('button', { name: 'Confirm' }).click();

        await expect(contentRow.getByLabel(state)).toBeVisible();
    });

    test(`change content to ${state} by checkbox`, async ({ page }) => {
        const contentText = await createRandomContent(page, mode);
        const contentRow = page.locator('tr', { hasText: escapeRegex(contentText) });

        await contentRow.getByRole('checkbox').click();
        await page.getByRole('button', { name: state }).click();
        await page.getByRole('button', { name: 'Confirm' }).click();

        await expect(contentRow.getByLabel(state)).toBeVisible();
    });

    test(`change content to ${state} by detail page`, async ({ page }) => {
        const contentText = await createRandomContent(page, mode);
        const contentRow = page.locator('tr', { hasText: escapeRegex(contentText) });

        await contentRow.click();
        await page.getByRole('button', { name: currentState }).click();
        await page.getByText(`Change to ${state}`).click();
        await page.getByRole('button', { name: 'Confirm' }).click();

        await expect(page.getByRole('button', { name: state })).toBeVisible();
    });
});

test('delete content', async ({ dropdown, page }) => {
    await createRandomContent(page, 'SaveAndEdit');

    await page.getByLabel('Options').click();
    await dropdown.delete();

    await expect(page.getByLabel('Identity')).not.toBeVisible();
});

test('delete content by options', async ({ dropdown, page }) => {
    const contentText = await createRandomContent(page, 'SaveAndClose');
    const contentRow = page.locator('tr', { hasText: escapeRegex(contentText) });

    await contentRow.getByLabel('Options').click();
    await dropdown.delete();

    await expect(contentRow).not.toBeVisible();
});

test('delete content by checkbox', async ({  page }) => {
    const contentText = await createRandomContent(page, 'SaveAndClose');
    const contentRow = page.locator('tr', { hasText: escapeRegex(contentText) });

    await contentRow.getByRole('checkbox').click();
    await page.getByRole('button', { name: 'Delete' }).click();
    await page.getByRole('button', { name: 'Yes' }).click();

    await expect(contentRow).not.toBeVisible();
});

async function createRandomContent(page: Page, mode: SaveMode) {
    const contentText = `content-${getRandomId()}`;

    await page.getByRole('button', { name: /New/ }).click();

    // Define content value.
    await page.locator('sqx-field-editor').getByRole('textbox').fill(contentText);
    await saveContent(page, mode);

    // Wait for the success alert.
    await page.getByRole('alert').getByText('Content created successfully.').waitFor({ state: 'visible' });

    return contentText;
}

type SaveMode =
    'Save' |
    'SaveAndAdd' |
    'SaveAndClose' |
    'SaveAndEdit' |
    'SavePublishAndAdd' |
    'SavePublishAndClose' |
    'SavePublishAndEdit';

async function saveContent(page: Page, mode: SaveMode) {
    switch (mode) {
        case 'SaveAndAdd':
            await page.getByLabel('Save', { exact: true }).getByLabel('More').click();
            await page.getByText('Save & add another').click();
            break;
        case 'SaveAndClose':
            await page.getByLabel('Save', { exact: true }).getByLabel('More').click();
            await page.getByText('Save & close').click();
            break;
        case 'SaveAndEdit':
            await page.getByRole('button', { name: 'Save', exact: true }).click();
            break;
        case 'SavePublishAndAdd':
            await page.getByLabel('Save and Publish').getByLabel('More').click();
            await page.getByText('Save and Publish & add another').click();
            break;
        case 'SavePublishAndClose':
            await page.getByLabel('Save and Publish').getByLabel('More').click();
            await page.getByText('Save and Publish & close').click();
            break;
        case 'SavePublishAndEdit':
            await page.getByRole('button', { name: 'Save and Publish' }).click();
            break;
        case 'Save':
            await page.getByRole('button', { name: 'Save', exact: true }).click();
            break;
    }
}
