import {
  browser,
  element,
  by,
  ExpectedConditions,
  protractor,
  ElementFinder
} from "protractor";

/**
 * Utility class for commonly called Protractor.browser methods.
 * The methods should be static and
 * not require new instance of class to use.
 */
export class BrowserUtil {
  constructor() {}

  //waits for element to be present on the DOM
  waitForElementPresent(elementLocator: ElementFinder) {
    const until = protractor.ExpectedConditions;
    browser.wait(until.presenceOf(elementLocator), 100000);
  }

  //switching between windows
  switchToChildWindow() {
    browser.getAllWindowHandles().then(function(handles) {
      var count = handles.length;
      var newWindow = handles[count - 1];
      browser.switchTo().window(newWindow);
    });
  }

  switchToParentWindow() {
    browser.getAllWindowHandles().then(function(handles) {
      browser.switchTo().window(handles[0]);
      browser.driver.executeScript("window.focus();");
    });
  }
  //waits for the page to load before performing any further operations. waits until the document.ready state becomes interactive or complete and returns the same.
  async getReadyState() {
    let states;
    const until = protractor.ExpectedConditions;
    await browser.wait(() => {
      return browser.executeScript("return document.readyState").then(state => {
        states = state;
        if (state === "interactive" || state === "complete") {
          return true;
        }
      });
    }, 100000);
    return states;
  }

  //get current url of the page
  public async getCurrentURL() {
    return await browser.getCurrentUrl().then(url => url);
    //alternatively we can use : return window.location.href;
  }

  //wait for angular enabled
  waitForAngularEnabledOnCurrentWindow() {
    return browser.waitForAngularEnabled(true).then(async () => {
      browser.waitForAngular();
    });
  }

  waitForAngularDisabledOnCurrentWindow() {
    return browser.waitForAngularEnabled(false);
  }
}
