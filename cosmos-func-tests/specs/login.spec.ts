import { ILoginData } from "../data/loginData";
import * as fs from "fs";
import { find } from "lodash";
import { BasePage } from "../pages/basePage.po";
import { browser, element, ExpectedConditions, by, Browser } from "protractor";
import { LoginPage } from "../pages/loginPage.po";
import { BrowserUtil } from "../utils/browser.util";
import { dataProvider } from "../data/dataProvider";
import { protractor } from "protractor/built/ptor";
import { HomePage } from "../pages/homePage.po";
import { config } from "../config";

describe("Login To Squidex", () => {
  let loginData = require("../../data/login.json");
  var authors: Array<ILoginData> = loginData.authors;
  let loginPg: LoginPage;
  let homePg: HomePage;
  let browserPg: BrowserUtil;

  beforeEach(() => {
    loginPg = new LoginPage();
    homePg = new HomePage();
    browserPg = new BrowserUtil();
  });

  afterEach(() => {
    homePg.userLogout();
    browser.sleep(2000);
  });

  it("Login with Vega Editor credentials", () => {
    loginPg.navigateTo();
    loginPg.loginButton().then(() => {
      browser.waitForAngularEnabled(false);
      browserPg.switchToChildWindow();
      loginPg.login(
        authors.find(function(obj) {
          return obj.name === "vegaEditor";
        })
      );
    });
    browser.waitForAngularEnabled(true).then(async () => {
      browser.waitForAngular();
      browserPg.switchToParentWindow();
      browser.sleep(5000);
      await expect(browserPg.getCurrentURL()).toBe(
        config.params.expectedUrlAfterNavigation
      );
      await expect(homePg.userNameDisplay().getText()).toBe(
        config.params.editorWelcomeMessage
      );
      await expect(homePg.commentaryDisplay().isDisplayed());
      await expect(
        homePg.userProfileIconDisplay().isDisplayed() &&
          homePg.userProfileIconDisplay().isEnabled()
      );
    });
  });
  it("Login with Vega Reviewer credentials", () => {
    loginPg.navigateTo();
    loginPg.loginButton().then(() => {
      browser.waitForAngularEnabled(false);
      browserPg.switchToChildWindow();
      loginPg.login(
        authors.find(function(obj) {
          return obj.name === "vegaReviewer";
        })
      );
    });
    browser.waitForAngularEnabled(true).then(async () => {
      browser.waitForAngular();
      browserPg.switchToParentWindow();
      browser.sleep(5000);
      await expect(browserPg.getCurrentURL()).toBe(
        config.params.expectedUrlAfterNavigation
      );
      await expect(homePg.userNameDisplay().getText()).toBe(
        config.params.reviewerWelcomeMessage
      );
      await expect(homePg.commentaryDisplay().isDisplayed());
      await expect(
        homePg.userProfileIconDisplay().isDisplayed() &&
          homePg.userProfileIconDisplay().isEnabled()
      );
    });
  });
});
