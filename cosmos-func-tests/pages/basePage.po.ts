import { browser, element, by, ExpectedConditions } from 'protractor';

/**
 * This is like the landing URL which we re-direct
 * Methods/properties for global elements should go here. 
 * explain about default keyword
 */
export class BasePage {
  constructor() {}

    loginButton(){
        return element(by.className('btn btn-success btn-lg login-button login-element')).click();
    }

    switchToLoginWindow(){
      browser.getAllWindowHandles().then(function(handles){
        var count=handles.length;
        var newWindow = handles[count-1];
        browser.switchTo().window(newWindow);
      })
  }

    switchToParentWindow(){
      browser.getAllWindowHandles().then(function(handles){
      browser.switchTo().window(handles[0]);
      browser.driver.executeScript('window.focus();');
    })
  }
}