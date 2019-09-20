import {
    browser,
    ElementFinder,
    protractor,
    WebElement
} from 'protractor';

const until = protractor.ExpectedConditions;

/**
 * Utility class for commonly called Protractor.browser methods.
 * The methods should be static and
 * not require new instance of class to use.
 */
export class BrowserUtil {
    // waits for element to be present on the DOM - angular
    public async getWhenVisible(locator: ElementFinder, timeout = 5000): Promise<WebElement> {
        await this.waitForElementToBeVisible(locator, timeout);
        return await locator;
    }

    public async waitForElementToBeVisible(locator: ElementFinder, timeout = 5000) {
        await browser.wait(until.visibilityOf(locator), timeout, `Element ${locator.locator().toString()} not visible`);
    }

    public async waitForElementToBeClickable(locator: ElementFinder, timeout = 5000) {
        await browser.wait(until.elementToBeClickable(locator), timeout, `Element ${locator.locator().toString()} not clickable`);
    }

    public async waitForElementToBePresent(locator: ElementFinder, timeout = 5000) {
        await browser.wait(until.presenceOf(locator), timeout, `Element ${locator.locator().toString()} not present`);
    }

    // waits for the element to be clickable and clicks
    public async waitForElementToBeVisibleAndClick(locator: ElementFinder, timeout = 5000) {
        await this.waitForElementToBeClickable(locator, timeout);
        return await locator.click();
    }

    public async waitForElementToBeVisibleAndGetText(locator: ElementFinder, timeout = 5000) {
        await this.waitForElementToBeClickable(locator, timeout);
        return await locator.getText();
    }

    // waits for the element to be present and writes
    public async waitForElementToBePresentAndWrite(locator: ElementFinder, text: string, timeout = 20000) {
        await this.getWhenVisible(locator, timeout);
        await locator.clear();
        await locator.sendKeys(text);
    }

    // brings the element to focus and clicks
    public async mouseMoveAndClick(locator: ElementFinder) {
        await browser.actions().mouseMove(locator).perform();
        await locator.click();
    }

    // brings the element to focus and writes
    public async mouseMoveAndWrite(locator: ElementFinder | WebElement, text: string) {
        await browser.actions().mouseMove(locator).click().perform();
        await locator.sendKeys(text);
    }

    // switching between windows
    public async switchToChildWindow() {
        const handles = await browser.getAllWindowHandles();
        const count = handles.length;
        const newWindow = handles[count - 1];
        await browser.switchTo().window(newWindow);
    }

    public async switchToParentWindow() {
        const handles = await browser.getAllWindowHandles();
        browser.switchTo().window(handles[0]);
        await browser.driver.executeScript('window.focus();');
    }

    // scrolls down the page - vertically and brings the element into view
    public async scrollIntoViewAndClick(webElement: ElementFinder) {
        await browser.executeScript('arguments[0].scrollIntoView()', webElement);

        await new BrowserUtil().waitForElementToBeVisibleAndClick(webElement);
    }

    public async scrollIntoView(webElement: ElementFinder): Promise<ElementFinder | WebElement> {
        await browser.executeScript('arguments[0].scrollIntoView()', webElement);
    }

    // waits for the page to load before performing any further operations. waits until the document.ready state becomes interactive or complete and returns the same.
    public async getReadyState() {
        let states;
        await browser.wait(async () => {
            return await browser.executeScript('return document.readyState').then(state => {
                states = state;
                if (state === 'interactive' || state === 'complete') {
                    return true;
                }
            });
        }, 100000);
        return states;
    }

    // get current url of the page
    public async getCurrentURL() {
        return await browser.getCurrentUrl();
    }

    // wait for angular enabled
    public async waitForAngularEnabledOnCurrentWindow(value: boolean) {
        await browser.waitForAngularEnabled(value);
        return await browser.waitForAngular();
    }

    public async waitForAngularDisabledOnCurrentWindow() {
        return await browser.waitForAngularEnabled(false);
    }

    public async selectAllContent() {
        await browser
            .actions()
            .keyDown(protractor.Key.CONTROL)
            .sendKeys('a')
            .perform();
    }

    // refresh the chrome instance
    public async browserRefresh() {
        await browser.refresh();
    }

}