/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Page } from '@playwright/test';
import { expect, test } from './_fixture';

test.beforeEach(async ({ page }) => {
    await page.goto('/app');
});

test('show proper English frontend', async ({ page }) => {
    await changeLanguage(page, 'English');

    await expect(page.getByText('Welcome to Squidex')).toBeVisible();
});

test('show proper French frontend', async ({ page }) => {
    await changeLanguage(page, 'Français');

    await expect(page.getByText('Bienvenue sur Squidex.')).toBeVisible();
});

test('show proper Dutch frontend', async ({ page }) => {
    await changeLanguage(page, 'Nederlands');

    await expect(page.getByText('Welkom bij Squidex.')).toBeVisible();
});

test('show proper Italian frontend', async ({ page }) => {
    await changeLanguage(page, 'Italiano');

    await expect(page.getByText('Benvenuto su Squidex.')).toBeVisible();
});

test('show proper Portugese frontend', async ({ page }) => {
    await changeLanguage(page, 'Portuguese');

    await expect(page.getByText('Bem-vindo a Squidex.')).toBeVisible();
});

test('show proper Chinese frontend', async ({ page }) => {
    await changeLanguage(page, '简体中文');

    await expect(page.getByText('欢迎来到 Squidex。')).toBeVisible();
});

async function changeLanguage(page: Page, name: string) {
    await page.locator('sqx-profile-menu span').first().click();
    await page.getByText('Language').click();
    await page.getByText(name).click();
}