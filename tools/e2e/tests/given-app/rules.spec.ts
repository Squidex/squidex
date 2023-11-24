import { expect, Page } from '@playwright/test';
import { escapeRegex, getRandomId } from '../utils';
import { test } from './_fixture';

test.beforeEach(async ({ page, appName }) => {
    await page.goto(`/app/${appName}`);
});

test('create rule', async ({ page }) => {
    const ruleName = await createRandomRule(page);
    const ruleCard = page.locator('div.card', { hasText: new RegExp(escapeRegex(ruleName)) });

    await expect(ruleCard).toBeVisible();
});

test('delete rule', async ({ dropdown, page }) => {
    const ruleName = await createRandomRule(page);
    const ruleCard = page.locator('div.card', { hasText: new RegExp(escapeRegex(ruleName)) });

    await ruleCard.getByTestId('options').click();
    await dropdown.delete();

    await expect(ruleCard).not.toBeVisible();
});

test('disable rule', async ({ dropdown, page }) => {
    const ruleName = await createRandomRule(page);
    const ruleCard = page.locator('div.card', { hasText: new RegExp(escapeRegex(ruleName)) });

    await ruleCard.getByTestId('options').click();
    await dropdown.action('Disable');

    await expect(ruleCard.locator('sqx-toggle .toggle-container')).toHaveAttribute('data-state', 'unchecked');
});

test('enable rule', async ({ dropdown, page }) => {
    const ruleName = await createRandomRule(page);
    const ruleCard = page.locator('div.card', { hasText: new RegExp(escapeRegex(ruleName)) });

    const disableRequest = page.waitForResponse(/rules/);

    await ruleCard.getByTestId('options').click();
    await dropdown.action('Disable');

    await disableRequest;

    await ruleCard.getByTestId('options').click();
    await dropdown.action('Enable');

    await expect(ruleCard.locator('sqx-toggle .toggle-container')).toHaveAttribute('data-state', 'checked');
});

test('edit rule', async ({ dropdown, page }) => {
    const ruleName = await createRandomRule(page);
    const ruleCard = page.locator('div.card', { hasText: new RegExp(escapeRegex(ruleName)) });

    await ruleCard.getByTestId('options').click();
    await dropdown.action('Edit');

    await expect(page.getByText('Enabled')).toBeVisible();
});

async function createRandomRule(page: Page) {
    const ruleName = `rule-${getRandomId()}`;

    await page.getByTestId('rules').click();
    await page.getByRole('link', { name: /New Rule/ }).click();

    // Setup rule action
    await page.getByText('Content changed').click();

    // Setup rule trigger
    await page.getByText('Webhook').click();
    await page.locator('sqx-formattable-input').first().getByRole('textbox').fill('https:/squidex.io');

    await page.getByRole('button', { name: 'Save' }).click();

    await page.getByText('Enabled').waitFor({ state: 'visible' });

    // Go back
    await page.getByTestId('back').click();

    // Setup name.
    await page.locator('div.card', { hasText: /Unnamed Rule/ }).getByRole('heading').first().dblclick();
    await page.locator('form').getByRole('textbox').fill(ruleName);
    await page.locator('form').getByTestId('save').click();

    return ruleName;
}