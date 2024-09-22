/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { getRandomId } from '../utils';
import { expect, test } from './_fixture';

test.beforeEach(async ({ languagesPage, appsPage }) => {
    const appName = `app-${getRandomId()}`;
    await appsPage.createNewApp(appName);

    await languagesPage.goto(appName);
});

test('has header', async ({ page }) => {
    const header = page.getByRole('heading', { name: /Languages/ });

    await expect(header).toBeVisible();
});

test('add random language', async ({ languagesPage }) => {
    const randomName = `language-${getRandomId()}`;

    await languagesPage.enterLanguage(randomName);
    await languagesPage.save();

    const languagesCard = await languagesPage.getLanguageCard(randomName);

    await expect(languagesCard.root).toBeVisible();
});

test('add language with dropdown', async ({ languagesPage }) => {
    await languagesPage.enterLanguage('de-');
    await languagesPage.selectLanguage('de-DE (German (Germany))');
    await languagesPage.save();

    const languagesCard = await languagesPage.getLanguageCard('German (Germany)');

    await expect(languagesCard.root).toBeVisible();
});

test('delete language', async ({ languagesPage }) => {
    const randomName = `language-${getRandomId()}`;

    await languagesPage.enterLanguage(randomName);
    await languagesPage.save();

    const languagesCard = await languagesPage.getLanguageCard(randomName);

    await expect(languagesCard.root).toBeVisible();

    await languagesCard.delete();
    await expect(languagesCard.root).not.toBeVisible();
});

test('add default language', async ({ languagesPage }) => {
    const randomName1 = `language-${getRandomId()}`;
    const randomName2 = `language-${getRandomId()}`;

    await languagesPage.enterLanguage(randomName1);
    await languagesPage.save();

    const languagesCard1 = await languagesPage.getLanguageCard(randomName1);
    await expect(languagesCard1.root).toBeVisible();

    await languagesPage.enterLanguage(randomName2);
    await languagesPage.save();

    const languagesCard2 = await languagesPage.getLanguageCard(randomName2);
    await languagesCard2.toggle();
    await languagesCard2.addFallbackLanguage(randomName1);
    await languagesCard2.save();

    await expect(languagesCard2.root.getByText(randomName2)).toBeVisible();
});

test('make master', async ({ languagesPage }) => {
    const randomName = `language-${getRandomId()}`;

    await languagesPage.enterLanguage(randomName);
    await languagesPage.save();

    const languagesCard = await languagesPage.getLanguageCard(randomName);
    await languagesCard.toggle();
    await languagesCard.makeMaster();
    await languagesCard.save();

    await expect(languagesCard.root.getByLabel('Delete')).toBeDisabled();
});