import { expect, test } from '@playwright/test';

test('login', async ({ page }, testInfo) => {
    try {
    await page.goto('/');

    // Start waiting for popup before clicking.
    const popupPromise = page.waitForEvent('popup');

    await page.getByTestId('login').click();

    const popup = await popupPromise;
    await popup.waitForLoadState();

    await popup.getByTestId('login-button').waitFor();

    await popup.getByPlaceholder('Enter Email').fill('hello@squidex.io');
    await popup.getByPlaceholder('Enter Password').fill('1q2w3e$R');
    await popup.getByTestId('login-button').click();

    await page.waitForURL(/app/);

    await expect(page).toHaveTitle(/Apps/);
    } finally {
        await testInfo.attach('login.png', {
            body: await page.screenshot(),
            contentType: 'image/png',
        });
    }
});

test('visual test', async ({ page }) => {
    await page.goto('/');

    // Start waiting for popup before clicking.
    const popupPromise = page.waitForEvent('popup');

    await page.getByTestId('login').click();

    const popup = await popupPromise;
    await expect(popup).toHaveScreenshot({ fullPage: true });
});
