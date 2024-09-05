/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import fs from 'fs/promises';
import path from 'path';
import { Page } from '@playwright/test';
import { TEMPORARY_PATH } from '../playwright.config';

let COUNTER = 0;

export function getRandomId() {
    const result = `${new Date().getTime()}-${COUNTER++}`;

    return result;
}

export function escapeRegex(string: string) {
    const result = string.replace(/[/\-\\^$*+?.()|[\]{}]/g, '\\$&');

    return new RegExp(result);
}

export async function writeJsonAsync(name: string, json: any) {
    const fullPath = await getPath(name);

    await fs.writeFile(fullPath, JSON.stringify(json), { encoding: 'utf8' });
}

export async function readJsonAsync<T>(name: string, defaultValue: T) {
    const fullPath = await getPath(name);

    const json = await fs.readFile(fullPath, 'utf8');

    if (json) {
        return JSON.parse(json) as T;
    } else {
        return defaultValue;
    }
}

async function getPath(name: string) {
    const fullPath = path.join(TEMPORARY_PATH, `${name}.json`);

    await fs.mkdir(TEMPORARY_PATH, { recursive: true });

    return fullPath;
}

export type FieldSaveMode =
    'CreateAndAdd' |
    'CreateAndClose' |
    'CreateAndEdit' |
    'SaveAndAdd' |
    'SaveAndClose';

export async function createSchema(page: Page, args: { name: string; mode?: FieldSaveMode }) {
    const { name } = args;

    await page.getByLabel('Create Schema').click();

    // Define schema name.
    await page.getByLabel('Name (required)').fill(name);

    // Save schema.
    await page.getByRole('button', { name: 'Create', exact: true }).click();
}

export async function createField(page: Page, args: { name: string; type?: string; mode?: FieldSaveMode }) {
    const { name, type, mode } = args;

    await page.locator('button').filter({ hasText: /^Add Field$/ }).click();

    // Define field type and name.
    await page.getByText(type || 'String', { exact: true }).click();
    await page.getByPlaceholder('Enter field name').fill(name);

    // Save schema.
    await saveField(page, mode || 'CreateAndClose');
}

export async function createNestedField(page: Page, args: { name: string; type?: string; mode?: FieldSaveMode }) {
    const { name, type, mode } = args;

    await page.locator('button').filter({ hasText: /Add Nested Field/ }).click();

    // Define field type and name.
    await page.getByText(type || 'String', { exact: true }).click();
    await page.getByPlaceholder('Enter field name').fill(name);

    // Save schema.
    await saveField(page, mode || 'CreateAndClose');
}

export async function saveField(page: Page, mode: FieldSaveMode) {
    switch (mode) {
        case 'CreateAndClose':
            await page.getByTestId('dialog').getByRole('button', { name: 'Create' }).click();
            break;
        case 'CreateAndAdd':
            await page.getByTestId('dialog').getByLabel('Add field').getByLabel('More').click();
            await page.getByText('Create & add another').click();
            break;
        case 'CreateAndEdit':
            await page.getByTestId('dialog').getByLabel('Add field').getByLabel('More').click();
            await page.getByText('Create & edit properties').click();
            break;
        case 'SaveAndClose':
            await page.getByTestId('dialog').getByRole('button', { name: 'Save and close' }).click();
            break;
        case 'SaveAndAdd':
            await page.getByTestId('dialog').getByLabel('Save field').getByLabel('More').click();
            await page.getByText('Save and add field').click();
            break;
    }
}