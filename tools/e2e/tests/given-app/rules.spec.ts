import { expect } from '@playwright/test';
import { RulePage, RulesPage } from '../pages';
import { getRandomId } from '../utils';
import { test } from './_fixture';

// We have no easy way to identity rules. Therefore run them sequentially.
test.describe.configure({ mode: 'serial' });

test.beforeEach(async ({ appName, rulesPage }) => {
    await rulesPage.goto(appName);
});

test('create rule', async ({ rulesPage, rulePage }) => {
    const ruleName = await createRandomRule(rulesPage, rulePage);
    const ruleCard = await rulesPage.getRule(ruleName);

    await expect(ruleCard.root).toBeVisible();
});

test('delete rule', async ({ rulesPage, rulePage }) => {
    const ruleName = await createRandomRule(rulesPage, rulePage);
    const ruleCard = await rulesPage.getRule(ruleName);

    const dropdown = await ruleCard.openOptionsDropdown();
    await dropdown.delete();

    await expect(ruleCard.root).not.toBeVisible();
});

test('disable rule', async ({ rulePage, rulesPage }) => {
    const ruleName = await createRandomRule(rulesPage, rulePage);
    const ruleCard = await rulesPage.getRule(ruleName);

    const dropdown = await ruleCard.openOptionsDropdown();
    await dropdown.action('Disable');

    await expect(ruleCard.root.locator('sqx-toggle .toggle-container')).toHaveAttribute('data-state', 'unchecked');
});

test('enable rule', async ({ rulePage, rulesPage }) => {
    const ruleName = await createRandomRule(rulesPage, rulePage);
    const ruleCard = await rulesPage.getRule(ruleName);

    const dropdown1 = await ruleCard.openOptionsDropdown();
    await dropdown1.action('Disable');

    await expect(ruleCard.root.locator('sqx-toggle .toggle-container')).toHaveAttribute('data-state', 'unchecked');

    const dropdown2 = await ruleCard.openOptionsDropdown();
    await dropdown2.action('Enable');

    await expect(ruleCard.root.locator('sqx-toggle .toggle-container')).toHaveAttribute('data-state', 'checked');
});

test('edit rule', async ({ page, rulePage, rulesPage }) => {
    const ruleName = await createRandomRule(rulesPage, rulePage);
    const ruleCard = await rulesPage.getRule(ruleName);

    const dropdown = await ruleCard.openOptionsDropdown();
    await dropdown.action('Edit');

    await expect(page.getByText('Enabled')).toBeVisible();
});

async function createRandomRule(rulesPage: RulesPage, rulePage: RulePage) {
    const ruleName = `rule-${getRandomId()}`;

    await rulesPage.addRule();

    await rulePage.selectContentChangedTrigger();
    await rulePage.selectWebhookAction();
    await rulePage.save();
    await rulePage.back();

    const rename = await rulesPage.renameRule(/Unnamed Rule/);
    await rename.enterName(ruleName);
    await rename.save();

    return ruleName;
}