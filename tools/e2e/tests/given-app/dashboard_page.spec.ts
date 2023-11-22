import { expect, test } from './_fixture';

test('visual test', async ({ page, appName }) => {
    await page.goto(`/app/${appName}`);

    await page.waitForLoadState('networkidle');

    await expect(page).toHaveScreenshot({ maxDiffPixelRatio: 0.05 });
});