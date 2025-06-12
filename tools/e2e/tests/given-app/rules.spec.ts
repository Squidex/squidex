import { expect } from '@playwright/test';
import { RulePage, RulesPage } from '../pages';
import { getRandomId } from '../utils';
import { test } from './_fixture';

test.beforeEach(async ({ appName, rulesPage }) => {
    await rulesPage.goto(appName);
});

test('has header', async ({ page }) => {
    const header = page.getByRole('heading', { name: /Rules/ });

    await expect(header).toBeVisible();
});

test('create rule', async ({ rulesPage, rulePage }) => {
    const ruleName = await createRandomRule(rulesPage, rulePage, true);
    const ruleCard = await rulesPage.getRuleCard(ruleName);

    await expect(ruleCard.root).toBeVisible();
});

test('create disabled rule', async ({ rulesPage, rulePage }) => {
    const ruleName = await createRandomRule(rulesPage, rulePage, true);
    const ruleCard = await rulesPage.getRuleCard(ruleName);

    await expect(ruleCard.root.locator('sqx-toggle div').first()).toHaveAttribute('data-state', 'unchecked');
});

test('delete rule', async ({ rulesPage, rulePage }) => {
    const ruleName = await createRandomRule(rulesPage, rulePage);
    const ruleCard = await rulesPage.getRuleCard(ruleName);

    const dropdown = await ruleCard.openOptionsDropdown();
    await dropdown.delete();

    await expect(ruleCard.root).not.toBeVisible();
});

test('disable rule', async ({ rulePage, rulesPage }) => {
    const ruleName = await createRandomRule(rulesPage, rulePage);
    const ruleCard = await rulesPage.getRuleCard(ruleName);

    const dropdown = await ruleCard.openOptionsDropdown();
    await dropdown.action('Disable');

    await expect(ruleCard.root.locator('sqx-toggle .toggle-container')).toHaveAttribute('data-state', 'unchecked');
});

test('enable rule', async ({ rulePage, rulesPage }) => {
    const ruleName = await createRandomRule(rulesPage, rulePage);
    const ruleCard = await rulesPage.getRuleCard(ruleName);

    const dropdown1 = await ruleCard.openOptionsDropdown();
    await dropdown1.action('Disable');

    await expect(ruleCard.root.locator('sqx-toggle .toggle-container')).toHaveAttribute('data-state', 'unchecked');

    const dropdown2 = await ruleCard.openOptionsDropdown();
    await dropdown2.action('Enable');

    await expect(ruleCard.root.locator('sqx-toggle .toggle-container')).toHaveAttribute('data-state', 'checked');
});

test('navigate to edit page', async ({ page, rulePage, rulesPage }) => {
    const ruleName = await createRandomRule(rulesPage, rulePage);
    const ruleCard = await rulesPage.getRuleCard(ruleName);

    const dropdown = await ruleCard.openOptionsDropdown();
    await dropdown.action('Edit');

    await expect(page.getByText('Enabled')).toBeVisible();
});

test('rename rule', async ({ rulePage, rulesPage }) => {
    const ruleName = await createRandomRule(rulesPage, rulePage);
    const ruleCard = await rulesPage.getRuleCard(ruleName);

    const newName = `rule-${getRandomId()}`;

    const renameDialog = await ruleCard.startRenameDblClick();
    await renameDialog.enterName(newName);
    await renameDialog.save();

    const newCard = await rulesPage.getRuleCard(newName);
    await expect(newCard.root).toBeVisible();
});

async function createRandomRule(rulesPage: RulesPage, rulePage: RulePage, disabled = false) {
    const ruleName = `rule-${getRandomId()}`;

    await rulesPage.addRule();

    const triggerDialog = await rulePage.addTrigger();
    await triggerDialog.selectContentChangedTrigger();
    await triggerDialog.add();

    const stepDialog = await rulePage.addStep();
    await stepDialog.selectWebhookAction();
    await stepDialog.add();

    await rulePage.enterName(ruleName);

    if (disabled) {
        await rulePage.toggleEnabled();
    }

    await rulePage.save();
    await rulePage.back();

    return ruleName;
}