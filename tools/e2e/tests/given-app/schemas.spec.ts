/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Page } from '@playwright/test';
import { createField, createNestedField, createSchema, escapeRegex, FieldSaveMode, getRandomId, saveField } from '../utils';
import { expect, test } from './_fixture';

test.beforeEach(async ({ page, appName }) => {
    await page.goto(`/app/${appName}/schemas`);
});

test('create schema', async ({ page }) => {
    // Add schema.
    const schemaName = await createRandomSchema(page);
    const schemaLink = page.locator('a.nav-link', { hasText: escapeRegex(schemaName) });

    await expect(schemaLink).toBeVisible();
});

test('delete schema', async ({ dropdown, page }) => {
    // Add schema.
    const schemaName = await createRandomSchema(page);
    const schemaLink = page.locator('a.nav-link', { hasText: escapeRegex(schemaName) });

    // Delete schema.
    await page.getByLabel('Options').click();
    await dropdown.delete();

    await expect(schemaLink).not.toBeVisible();
});

test('publish schema', async ({ page }) => {
    // Add schema.
    await createRandomSchema(page);

    // Publish schema.
    await page.getByRole('button', { name: 'Published', exact: true }).click();

    await expect(page.getByRole('button', { name: 'Published', exact: true })).toBeDisabled();
});

test('add field', async ({ page }) => {
    // Add schema.
    await createRandomSchema(page);

    const fieldName = await createRandomField(page, 'CreateAndClose');
    const fieldRow = page.getByText(fieldName);

    await expect(fieldRow).toBeVisible();
});

test('add field and edit', async ({ page }) => {
    const fieldLabel = `field-${getRandomId()}`;

    // Add schema.
    await createRandomSchema(page);

    // Add field.
    await createRandomField(page, 'CreateAndEdit');

    // Edit field.
    await page.getByLabel('Label').fill(fieldLabel);
    await saveField(page, 'SaveAndClose');

    const fieldRow = page.getByText(fieldLabel);

    await expect(fieldRow).toBeVisible();
});

test('add field and add another', async ({ page }) => {
    // Add schema.
    await createRandomSchema(page);

    // Add field.
    const fieldName1 = await createRandomField(page, 'CreateAndAdd');
    const fieldName2 = `field-${getRandomId()}`;

    // Add another field.
    await page.getByPlaceholder('Enter field name').fill(fieldName2);
    await saveField(page, 'CreateAndClose');

    const fieldRow1 = page.getByText(fieldName1);
    const fieldRow2 = page.getByText(fieldName2);

    await expect(fieldRow1).toBeVisible();
    await expect(fieldRow2).toBeVisible();
});

test('add field to array', async ({ page }) => {
    // Add schema.
    await createRandomSchema(page);

    // Add array array.
    await createRandomField(page, 'CreateAndClose', 'Array');

    // Add field to array.
    const fieldName = await createRandomNestedField(page, 'CreateAndClose');
    const fieldRow = page.getByText(fieldName);

    await expect(fieldRow).toBeVisible();
});

test('add field to array and ed', async ({ page }) => {
    const fieldLabel = `field-${getRandomId()}`;

    // Add schema.
    await createRandomSchema(page);

    // Add array array.
    await createRandomField(page, 'CreateAndClose', 'Array');

    // Add field to array.
    await createRandomNestedField(page, 'CreateAndEdit');

    // Edit field.
    await page.getByLabel('Label').fill(fieldLabel);
    await saveField(page, 'SaveAndClose');

    const fieldRow = page.getByText(fieldLabel);

    await expect(fieldRow).toBeVisible();
});

test('add field to array and another', async ({ page }) => {
    // Add schema.
    await createRandomSchema(page);

    // Add array array.
    await createRandomField(page, 'CreateAndClose', 'Array');

    // Add field to array.
    const fieldName1 = await createRandomNestedField(page, 'CreateAndAdd');
    const fieldName2 = `field-${getRandomId()}`;

    // Add another field.
    await page.getByPlaceholder('Enter field name').fill(fieldName2);
    await saveField(page, 'CreateAndClose');

    const fieldRow1 = page.getByText(fieldName1);
    const fieldRow2 = page.getByText(fieldName2);

    await expect(fieldRow1).toBeVisible();
    await expect(fieldRow2).toBeVisible();
});

test('delete field', async ({ dropdown, page }) => {
    // Add schema.
    await createRandomSchema(page);

    // Add field.
    const fieldName = await createRandomField(page, 'CreateAndClose');
    const fieldRow = page.locator('div.table-items-row-summary', { hasText: escapeRegex(fieldName) });

    // Delete field.
    await fieldRow.getByLabel('Options').click();
    await dropdown.delete();

    await expect(fieldRow).not.toBeVisible();
});

async function createRandomField(page: Page, mode: FieldSaveMode, type = 'String') {
    const name = `field-${getRandomId()}`;

    await createField(page, { name, mode, type });
    return name;
}

async function createRandomNestedField(page: Page, mode: FieldSaveMode, type = 'String') {
    const name = `field-${getRandomId()}`;

    await createNestedField(page, { name, mode, type });
    return name;
}

async function createRandomSchema(page: Page) {
    const name = `schema-${getRandomId()}`;

    await createSchema(page, { name });
    return name;
}