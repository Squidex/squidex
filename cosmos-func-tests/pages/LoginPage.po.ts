import { BrowserUtil } from "./../utils/Browser.util";
import { LoginData } from "../data/LoginData";
import { browser, element, by } from "protractor";
import { config } from "../config";

/**
 * Class representing login page.
 * Login window which opens after clicking on Login button on Squidex base page
 */
export class LoginPage extends BrowserUtil {
  /**
   * signs in using specified username/password in login.json file.
   */


  public async login(loginData: LoginData) {
    this.waitForAngularDisabledOnCurrentWindow();
    this.switchToChildWindow();
    const usernameInput = element(by.id("userNameInput"));
    const passwordInput = element(by.id("passwordInput"));
    const submitButton = element(by.id("submitButton"));
    usernameInput.sendKeys(loginData.username);
    passwordInput.sendKeys(loginData.password);
    submitButton.click();
    this.waitForAngularEnabledOnCurrentWindow().then(async () => {
    this.switchToParentWindow();
    });
  }
  // navigating to Squidex base page
  public navigateTo() {
    browser.get(config.params.baseUrl);
  }
}
