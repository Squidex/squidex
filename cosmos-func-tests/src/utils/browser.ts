import {
    browser,
    ElementFinder,
    protractor
} from 'protractor';

/**
 * Utility class for commonly called Protractor.browser methods.
 * The methods should be static and
 * not require new instance of class to use.
 */
export class BrowserUtil {
    // waits for element to be present on the DOM - angular
    public async waitForElementToBePresent(locator: ElementFinder, timeout = 100000) {
        const until = protractor.ExpectedConditions;
        await browser.wait(until.visibilityOf(locator), timeout);
        return await locator;
    }

    // waits for the element to be clickable and clicks
    public async waitForElementToBeVisibleAndClick(locator: ElementFinder, timeout = 100000) {
        const until = protractor.ExpectedConditions;
        await browser.wait(until.elementToBeClickable(locator), timeout, 'Element not clickable');
        return await locator.click();
    }

    // waits for the element to be present and writes
    public async waitForElementToBePresentAndWrite(locator: ElementFinder, text: string, timeout = 100000) {
        const until = protractor.ExpectedConditions;
        await browser.wait(until.presenceOf(locator), timeout);
        await locator.clear();
        await locator.sendKeys(text);
    }

    // brings the element to focus and clicks
    public async mouseMoveAndClick(locator: ElementFinder) {
        await browser.actions().mouseMove(locator).perform();
        await locator.click();
    }

    // brings the element to focus and writes
    public async mouseMoveAndWrite(locator: ElementFinder, text: string) {
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
    public async scrollIntoViewAndClick(webelement: ElementFinder) {
        await browser
            .executeScript('arguments[0].scrollIntoView()', webelement)
            .then(() => {
                webelement.click();
            });
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
        return await states;
    }

    // get current url of the page
    public async getCurrentURL() {
        return await browser.getCurrentUrl().then(url => url);
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
            .keyDown(protractor.Key.ALT)
            .sendKeys('a')
            .keyUp(protractor.Key.ALT)
            .perform();
    }

}
