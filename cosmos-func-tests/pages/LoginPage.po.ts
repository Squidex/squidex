import { LoginData } from "../data/LoginData";
import {
  browser,
  element,
  by,
  ExpectedConditions,
  ElementFinder,
  $
} from "protractor";
import { BasePage } from "./BasePage.po";
import { config } from "../config";

/**
 * Class representing login page.
 * Login window which opens after clicking on Login button on Squidex base page
 */
export class LoginPage extends BasePage {
  /**
   * signs in using specified username/password in login.json file.
   */
  public login(loginData: LoginData) {
    const usernameInput = element(by.id("userNameInput"));
    const passwordInput = element(by.id("passwordInput"));
    const submitButton = element(by.id("submitButton"));
    usernameInput.sendKeys(loginData.username);
    passwordInput.sendKeys(loginData.password);
    submitButton.click();
  }
  // navigating to Squidex base page
  public navigateTo() {
    browser.get(config.params.baseUrl);
  }

  public closePopUp() {
    element(by.xpath("//span[@class='ng-tns-c3-5']")).click();
  }
}
