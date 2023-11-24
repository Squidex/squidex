/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Page } from '@playwright/test';
import { escapeRegex, getRandomId } from '../utils';
import { expect, test } from './_fixture';

test.beforeEach(async ({ page, appName }) => {
    await page.goto(`/app/${appName}`);
});

test('create schema', async ({ page }) => {
    const schemaName = await createRandomSchema(page);
    const schemaLink = page.locator('a.nav-link', { hasText: new RegExp(escapeRegex(schemaName)) });

    await expect(schemaLink).toBeVisible();
});

test('delete schema', async ({ dropdown, page }) => {
    const schemaName = await createRandomSchema(page);
    const schemaLink = page.locator('a.nav-link', { hasText: new RegExp(escapeRegex(schemaName)) });

    await page.getByTestId('options').click();
    await dropdown.delete();

    await expect(schemaLink).not.toBeVisible();
});

test('publish schema', async ({ page }) => {
    await createRandomSchema(page);

    await page.getByRole('button', { name: 'Published', exact: true }).click();

    await expect(page.getByRole('button', { name: 'Published', exact: true })).toBeDisabled();
});

test('add field', async ({ page }) => {
    await createRandomSchema(page);

    const fieldName = await createRandomField(page);
    const fieldRow = page.getByText(fieldName);

    await expect(fieldRow).toBeVisible();
});

test('delete field', async ({ dropdown, page }) => {
    await createRandomSchema(page);

    const fieldName = await createRandomField(page);
    const fieldRow = page.locator('div.table-items-row-summary', { hasText: new RegExp(escapeRegex(fieldName)) });

    await fieldRow.getByTestId('options').click();
    await dropdown.delete();

    await expect(fieldRow).not.toBeVisible();
});

async function createRandomField(page: Page) {
    const fieldName = `field-${getRandomId()}`;

    await page.locator('button').filter({ hasText: /^Add Field$/ }).click();

    // Setup name.
    await page.getByPlaceholder('Enter field name').fill(fieldName);

    // Save
    await page.getByRole('button', { name: 'Create and close' }).click();

    return fieldName;
}

async function createRandomSchema(page: Page) {
    const schemaName = `schema-${getRandomId()}`;

    await page.getByTestId('schemas').click();

    await page.getByTestId('new-schema').click();

    // Setup name.
    await page.getByLabel('Name (required)').fill(schemaName);

    // Save
    await page.getByRole('button', { name: 'Create' }).click();

    return schemaName;
}