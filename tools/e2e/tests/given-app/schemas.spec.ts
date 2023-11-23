import { Page } from '@playwright/test';
import { escapeRegex, getRandomId } from '../utils';
import { expect, test } from './_fixture';

test('create schema', async ({ page, appName }) => {
    const schemaName = await createRandomSchema(page, appName);
    const schemaLink = page.locator('a.nav-link', { hasText: new RegExp(escapeRegex(schemaName)) });

    await expect(schemaLink).toBeVisible();
});

test('delete schema', async ({ page, appName }) => {
    const schemaName = await createRandomSchema(page, appName);
    const schemaLink = page.locator('a.nav-link', { hasText: new RegExp(escapeRegex(schemaName)) });

    await page.getByTestId('options').click();
    await page.getByText('Delete').click();
    await page.getByRole('button', { name: /Yes/ }).click();

    await expect(schemaLink).not.toBeVisible();
});

test('publish schema', async ({ page, appName }) => {
    await createRandomSchema(page, appName);

    await page.getByRole('button', { name: 'Published', exact: true }).click();

    await expect(page.getByRole('button', { name: 'Published', exact: true })).toBeDisabled();
});

test('add field', async ({ page, appName }) => {
    await createRandomSchema(page, appName);

    const fieldName = await createRandomField(page);
    const fieldRow = page.getByText(fieldName);

    await expect(fieldRow).toBeVisible();
});

test('delete field', async ({ page, appName }) => {
    await createRandomSchema(page, appName);

    const fieldName = await createRandomField(page);
    const fieldRow = page.locator('div.table-items-row-summary', { hasText: new RegExp(escapeRegex(fieldName)) });

    await fieldRow.getByTestId('options').click();

    // The delete button is added globally.
    await page.getByText('Delete').click();
    await page.getByRole('button', { name: /Yes/ }).click();

    await expect(fieldRow).not.toBeVisible();
});

async function createRandomField(page: Page) {
    const fieldName = `field-${getRandomId()}`;

    await page.locator('button').filter({ hasText: /^Add Field$/ }).click();
    await page.getByPlaceholder('Enter field name').fill(fieldName);
    await page.getByRole('button', { name: 'Create and close' }).click();

    return fieldName;
}

async function createRandomSchema(page: Page, appName: string) {
    const schemaName = `schema-${getRandomId()}`;

    await page.goto(`/app/${appName}`);

    await page.getByTestId('schemas').click();

    await page.getByTestId('new-schema').click();
    await page.getByLabel('Name (required)').fill(schemaName);
    await page.getByRole('button', { name: 'Create' }).click();

    return schemaName;
}