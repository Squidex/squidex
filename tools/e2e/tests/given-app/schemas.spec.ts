/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { SchemaPage, SchemasPage } from '../pages';
import { getRandomId } from '../utils';
import { expect, test } from './_fixture';

test.beforeEach(async ({ appName, schemasPage }) => {
    await schemasPage.goto(appName);
});

test('create schema', async ({ schemasPage }) => {
    const schemaName = await createRandomSchema(schemasPage);
    const schemaLink = await schemasPage.getSchemaLink(schemaName);

    await expect(schemaLink.root).toBeVisible();
});

test('delete schema', async ({ schemasPage, schemaPage }) => {
    const schemaName = await createRandomSchema(schemasPage);
    const schemaLink = await schemasPage.getSchemaLink(schemaName);

    const dropdown = await schemaPage.openOptionsDropdown();
    await dropdown.delete();

    await expect(schemaLink.root).not.toBeVisible();
});

test('publish schema', async ({ schemasPage, schemaPage }) => {
    await createRandomSchema( schemasPage);

    await schemaPage.publish();
});

test('unpublish schema', async ({ schemasPage, schemaPage }) => {
    await createRandomSchema( schemasPage);

    await schemaPage.publish();
    await schemaPage.unpublish();
});

test('add field', async ({ schemasPage, schemaPage }) => {
    await createRandomSchema( schemasPage);

    const fieldName = await createRandomField(schemaPage);
    const fieldRow = await schemaPage.getFieldRow(fieldName);

    await expect(fieldRow.root).toBeVisible();
});

test('add field and edit', async ({ schemasPage, schemaPage }) => {
    await createRandomSchema( schemasPage);

    const fieldName = `field-${getRandomId()}`;
    const fieldLabel = `field-${getRandomId()}`;

    const fieldDialog = await schemaPage.openFieldWizard();
    await fieldDialog.enterName(fieldName);
    await fieldDialog.createAndEdit();

    await fieldDialog.enterLabel(fieldLabel);
    await fieldDialog.saveAndClose();

    const fieldRow = await schemaPage.getFieldRow(fieldLabel);

    await expect(fieldRow.root).toBeVisible();
});

test('add field and add another', async ({ schemasPage, schemaPage }) => {
    await createRandomSchema( schemasPage);

    const fieldName1 = `field-${getRandomId()}`;
    const fieldName2 = `field-${getRandomId()}`;

    const fieldDialog = await schemaPage.openFieldWizard();
    await fieldDialog.enterName(fieldName1);
    await fieldDialog.createAndAdd();

    await fieldDialog.enterName(fieldName2);
    await fieldDialog.createAndAdd();

    const fieldRow1 = await schemaPage.getFieldRow(fieldName1);
    const fieldRow2 = await schemaPage.getFieldRow(fieldName2);

    await expect(fieldRow1.root).toBeVisible();
    await expect(fieldRow2.root).toBeVisible();
});

test('add field to array', async ({ schemasPage, schemaPage }) => {
    await createRandomSchema( schemasPage);

    const rootFieldName = `field-${getRandomId()}`;

    const rootDialog = await schemaPage.openFieldWizard();
    await rootDialog.enterName(rootFieldName);
    await rootDialog.enterType('Array');
    await rootDialog.createAndClose();

    const nestedFieldName = `field-${getRandomId()}`;

    const nestedDialog = await schemaPage.openNestedFieldWizard();
    await nestedDialog.enterName(nestedFieldName);
    await nestedDialog.createAndClose();

    const fieldRow = await schemaPage.getFieldRow(nestedFieldName);

    await expect(fieldRow.root).toBeVisible();
});

test('add field to array and ed', async ({ schemasPage, schemaPage }) => {
    await createRandomSchema(schemasPage);

    const rootFieldName = `field-${getRandomId()}`;

    const rootDialog = await schemaPage.openFieldWizard();
    await rootDialog.enterName(rootFieldName);
    await rootDialog.enterType('Array');
    await rootDialog.createAndClose();

    const nestedFieldName = `field-${getRandomId()}`;
    const nestedFieldLabel = `field-${getRandomId()}`;

    const nestedDialog = await schemaPage.openNestedFieldWizard();
    await nestedDialog.enterName(nestedFieldName);
    await nestedDialog.createAndEdit();

    await nestedDialog.enterLabel(nestedFieldLabel);
    await nestedDialog.saveAndClose();

    const fieldRow = await schemaPage.getFieldRow(nestedFieldLabel);

    await expect(fieldRow.root).toBeVisible();
});

test('add field to array and another', async ({ schemasPage, schemaPage }) => {
    await createRandomSchema(schemasPage);

    const rootFieldName = `field-${getRandomId()}`;

    const rootDialog = await schemaPage.openFieldWizard();
    await rootDialog.enterName(rootFieldName);
    await rootDialog.enterType('Array');
    await rootDialog.createAndClose();

    const nestedFieldName1 = `field-${getRandomId()}`;
    const nestedFieldName2 = `field-${getRandomId()}`;

    const nestedDialog = await schemaPage.openNestedFieldWizard();
    await nestedDialog.enterName(nestedFieldName1);
    await nestedDialog.createAndAdd();

    await nestedDialog.enterName(nestedFieldName2);
    await nestedDialog.createAndClose();

    const fieldRow1 = await schemaPage.getFieldRow(nestedFieldName1);
    const fieldRow2 = await schemaPage.getFieldRow(nestedFieldName2);

    await expect(fieldRow1.root).toBeVisible();
    await expect(fieldRow2.root).toBeVisible();
});

test('delete field', async ({ schemasPage, schemaPage }) => {
    await createRandomSchema(schemasPage);

    const fieldName = await createRandomField(schemaPage);
    const fieldRow = await schemaPage.getFieldRow(fieldName);

    const dropdown = await fieldRow.openOptionsDropdown();
    await dropdown.delete();

    await expect(fieldRow.root).not.toBeVisible();
});

async function createRandomField(schemaPage: SchemaPage) {
    const name = `field-${getRandomId()}`;

    const fieldDialog = await schemaPage.openFieldWizard();
    await fieldDialog.enterName(name);
    await fieldDialog.enterType('String');
    await fieldDialog.createAndClose();

    return name;
}

async function createRandomSchema(schemasPage: SchemasPage) {
    const name = `schema-${getRandomId()}`;

    const schemaDialog = await schemasPage.openSchemaDialog();
    await schemaDialog.enterName(name);
    await schemaDialog.save();

    return name;
}