/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ContentPage, ContentsPage } from '../pages';
import { getRandomId } from '../utils';
import { expect, test } from './_fixture';

test.beforeEach(async ({ appName, schemaName, contentsPage }) => {
    await contentsPage.goto(appName, schemaName);
    await contentsPage.increasePageSize();
});

test('create content and close', async ({ contentsPage, contentPage }) => {
    await contentsPage.addContent();

    const contentText = `content-${getRandomId()}`;
    await contentPage.enterField(contentText);
    await contentPage.saveAndClose();

    const contentRow = await contentsPage.getContentRow(contentText);

    await expect(contentRow.root.getByLabel('Draft')).toBeVisible();
});

test('create content and edit', async ({ page, contentsPage, contentPage }) => {
    await contentsPage.addContent();

    const contentText = `content-${getRandomId()}`;
    await contentPage.enterField(contentText);
    await contentPage.saveAndEdit();

    await expect(page.getByRole('button', { name: /Draft/ })).toBeVisible();
    await expect(page.getByLabel('Identity')).toBeVisible();
});

test('create content and add another', async ({ page, contentsPage, contentPage }) => {
    await contentsPage.addContent();

    const contentText = `content-${getRandomId()}`;
    await contentPage.enterField(contentText);
    await contentPage.saveAndAdd();

    await expect(page.locator('sqx-field-editor').getByRole('textbox')).toBeEmpty();
});

test('create content as published and edit', async ({ page, contentsPage, contentPage }) => {
    await contentsPage.addContent();

    const contentText = `content-${getRandomId()}`;
    await contentPage.enterField(contentText);
    await contentPage.savePublishAndEdit();

    await expect(page.getByRole('button', { name: /Published/ })).toBeVisible();
    await expect(page.getByLabel('Identity')).toBeVisible();
});

test('create content as published and close', async ({ contentsPage, contentPage }) => {
    await contentsPage.addContent();

    const contentText = `content-${getRandomId()}`;
    await contentPage.enterField(contentText);
    await contentPage.savePublishAndClose();

    const contentRow = await contentsPage.getContentRow(contentText);

    await expect(contentRow.root.getByLabel('Published')).toBeVisible();
});

test('update content', async ({ page, contentsPage, contentPage }) => {
    const contentText = await createRandomContent(contentsPage, contentPage);
    const contentRow = await contentsPage.getContentRow(contentText);
    await contentRow.edit();

    const contentUpdate = `content-${getRandomId()}`;

    await contentPage.enterField(contentUpdate);
    await contentPage.save();

    await page.getByText('Version: 1').waitFor({ state: 'visible' });
    await contentPage.back();

    const updateRow = await contentsPage.getContentRow(contentUpdate);

    await expect(updateRow.root).toBeVisible();
});

const states = [{
    status: 'Archived',
    currentStatus: 'Draft',
}, {
    status: 'Draft',
    currentStatus: 'Published',
}, {
    status: 'Published',
    currentStatus: 'Draft',
}];

states.forEach(({ status, currentStatus }) => {
    test(`change content from <${currentStatus}> to <${status}>`, async ({ contentsPage, contentPage }) => {
        await contentsPage.addContent();

        const contentText = `content-${getRandomId()}`;
        await contentPage.enterField(contentText);

        if (currentStatus === 'Published') {
            await contentPage.savePublishAndClose();
        } else {
            await contentPage.saveAndClose();
        }

        const contentRow = await contentsPage.getContentRow(contentText);
        const dropdown = await contentRow.openOptionsDropdown();
        await dropdown.actionConfirm(`Change to ${status}`);

        await expect(contentRow.root.getByLabel(status)).toBeVisible();
    });

    test(`change content from <${currentStatus}> to <${status}> by checkbox`, async ({ contentsPage, contentPage }) => {
        await contentsPage.addContent();

        const contentText = `content-${getRandomId()}`;
        await contentPage.enterField(contentText);

        if (currentStatus === 'Published') {
            await contentPage.savePublishAndClose();
        } else {
            await contentPage.saveAndClose();
        }

        const contentRow = await contentsPage.getContentRow(contentText);
        await contentRow.select();
        await contentsPage.changeSelectedStatus(status);

        await expect(contentRow.root.getByLabel(status)).toBeVisible();
    });

    test(`change content from <${currentStatus}> to <${status}> by detail page`, async ({ page, contentsPage, contentPage }) => {
        await contentsPage.addContent();

        const contentText = `content-${getRandomId()}`;
        await contentPage.enterField(contentText);

        if (currentStatus === 'Published') {
            await contentPage.savePublishAndEdit();
        } else {
            await contentPage.saveAndEdit();
        }

        const dropdown = await contentPage.openStatusDropdown(currentStatus);
        await dropdown.actionConfirm(`Change to ${status}`);

        await expect(page.getByRole('button', { name: status })).toBeVisible();
    });
});

test('delete content by details', async ({ page, contentsPage, contentPage }) => {
    const contentText = `field-${getRandomId()}`;

    await contentsPage.addContent();

    await contentPage.enterField(contentText);
    await contentPage.saveAndEdit();

    const dropdown = await contentPage.openOptionsDropdown();
    await dropdown.delete();

    await expect(page.getByLabel('Identity')).not.toBeVisible();
});

test('delete content by options', async ({ contentsPage, contentPage }) => {
    const contentText = await createRandomContent(contentsPage, contentPage);
    const contentRow = await contentsPage.getContentRow(contentText);

    const dropdown = await contentRow.openOptionsDropdown();
    await dropdown.delete();

    await expect(contentRow.root).not.toBeVisible();
});

test('delete content by checkbox', async ({ contentsPage, contentPage }) => {
    const contentText = await createRandomContent(contentsPage, contentPage);
    const contentRow = await contentsPage.getContentRow(contentText);

    await contentRow.select();
    await contentsPage.deleteSelected();

    await expect(contentRow.root).not.toBeVisible();
});

async function createRandomContent(contentsPage: ContentsPage, contentPage: ContentPage) {
    const contentText = `content-${getRandomId()}`;

    await contentsPage.addContent();

    await contentPage.enterField(contentText);
    await contentPage.saveAndClose();

    return contentText;
}
