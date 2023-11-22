import { expect, test } from './_fixture';

test('visual test', async ({ page, appName }) => {
    await page.goto(`/app/${appName}`);

    await expect(page).toHaveScreenshot();
});