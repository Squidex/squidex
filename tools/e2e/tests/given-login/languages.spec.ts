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

    // See: https://viralsfire.com/post/playwright-to-be-visible-timeout-is-ignored
    expect(header).toBeVisible({ timeout: 5000 });
});

test('add random language', async ({ languagesPage }) => {
    const randomName = `language-${getRandomId()}`;

    await languagesPage.enterLanguage(randomName);
    await languagesPage.save();

    const languagesCard = await languagesPage.getLanguageCard(randomName);

    expect(languagesCard.root).toBeVisible();
});

test('add language with from dropdown', async ({ languagesPage }) => {
    await languagesPage.enterLanguage('de-');
    await languagesPage.selectLanguage('de-DE (German (Germany))');
    await languagesPage.save();

    const languagesCard = await languagesPage.getLanguageCard('German (Germany)');

    expect(languagesCard.root).toBeVisible();
});

test('delete language', async ({ languagesPage }) => {
    const randomName = `language-${getRandomId()}`;

    await languagesPage.enterLanguage(randomName);
    await languagesPage.save();

    const languagesCard = await languagesPage.getLanguageCard(randomName);

    expect(languagesCard.root).toBeVisible();

    await languagesCard.delete();
    expect(languagesCard.root).not.toBeVisible();
});

test('add default language', async ({ languagesPage }) => {
    const randomName1 = `language-${getRandomId()}`;
    const randomName2 = `language-${getRandomId()}`;

    await languagesPage.enterLanguage(randomName1);
    await languagesPage.save();

    const languagesCard1 = await languagesPage.getLanguageCard(randomName1);
    expect(languagesCard1.root).toBeVisible();

    await languagesPage.enterLanguage(randomName2);
    await languagesPage.save();

    const languagesCard2 = await languagesPage.getLanguageCard(randomName2);
    await languagesCard2.toggle();
    await languagesCard2.addFallbackLanguage(randomName1);
    await languagesCard2.save();

    expect(languagesCard2.root.getByText(randomName2)).toBeVisible();
});

test('make master', async ({ languagesPage }) => {
    const randomName = `language-${getRandomId()}`;

    await languagesPage.enterLanguage(randomName);
    await languagesPage.save();

    const languagesCard = await languagesPage.getLanguageCard(randomName);
    await languagesCard.toggle();
    await languagesCard.makeMaster();
    await languagesCard.save();

    expect(languagesCard.root.getByLabel('Delete')).toBeDisabled();
});